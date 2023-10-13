using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Codenade.Inputbinder
{
    internal class ProfileListPopulator : MonoBehaviour
    {
        internal event Action<string> DeletionRequested;

        protected FileSystemWatcher _profileDirWatcher;
        protected Dictionary<string, GameObject> _profiles;
        protected string _localeName;

        private void Awake() => _profiles = new Dictionary<string, GameObject>();

        private void OnEnable()
        {
            try
            {
                _localeName = Utils.GetUserLocaleName();
            }
            catch (Exception e)
            {
                QLog.Error(e);
                _localeName = null;
            }
            InputActionManager am = Inputbinder.Instance.ActionManager;
            _profileDirWatcher = new FileSystemWatcher(am.ProfileBasePath, '*' + am.ProfileExtension);
            _profileDirWatcher.Renamed += ProfileDirWatcher_Renamed;
            _profileDirWatcher.Deleted += ProfileDirWatcher_Deleted;
            StartCoroutine(ProfileDirLoadCoroutine());
        }

        private IEnumerator ProfileDirLoadCoroutine()
        {
            var am = Inputbinder.Instance.ActionManager;
            var list = new List<Tuple<string, long>>();
            foreach (var file in Directory.GetFiles(am.ProfileBasePath, '*' + am.ProfileExtension))
                list.Add(new Tuple<string, long>(file, File.GetLastWriteTime(file).Ticks));
            var orderedList = list.OrderByDescending(d => d.Item2);
            foreach (var path in orderedList)
            {
                Task<InputProfileData> lt = new Task<InputProfileData>(LoadSingle, path.Item1);
                lt.Start();
                yield return new WaitUntil(() => lt.IsCompleted);
                AddElement(Path.GetFileNameWithoutExtension(path.Item1), lt.Result);
            }
        }

        protected InputProfileData LoadSingle(object path) => ProfileDefinitions.LoadInfoVersion1((string)path);

        private void AddElement(string name, InputProfileData profile)
        {
            var profileElementObj = Instantiate(Inputbinder.Instance.BindingUI.Assets[BindingUI.PrefabKeys.ProfileElement], transform.Find("Content"));
            var profileElement = profileElementObj.transform;
            var grpInfo = profileElement.Find("GrpInfo");
            var grpBtns = profileElement.Find("GrpBtns");
            var grpInfoTop = grpInfo.Find("GrpTop");
            var grpBtnsTop = grpBtns.Find("GrpTop");
            var btnDel = grpBtnsTop.Find("BtnDel").GetComponent<Button>();
            var btnLoad = grpBtnsTop.Find("BtnLoad").GetComponent<Button>();
            var btnDefault = grpBtns.Find("BtnDflt").GetComponent<Button>();
            var lblName = grpInfoTop.Find("LblName").GetComponent<TextMeshProUGUI>();
            var isActive = grpInfoTop.Find("IsActive").gameObject;
            var lblDesc = grpInfo.Find("LblDetails").GetComponent<TextMeshProUGUI>();
            btnDefault.gameObject.SetActive(GlobalConfiguration.DefaultProfile != name);
            btnDefault.onClick.AddListener(() => SetDefault(name));
            isActive.SetActive(name == Inputbinder.Instance.ActionManager.ProfileName);
            lblName.text = name;
            btnDel.onClick.AddListener(() => DeletionRequested?.Invoke(name));
            btnLoad.onClick.AddListener(() => Load(name));
            if (profile.FileVersion > 0)
            {
                lblDesc.text = $"{DateTimeOffset.FromUnixTimeSeconds(profile.Timestamp).ToLocalTime().DateTime.ToString(Utils.GetFullDateTimeFormat(_localeName) ?? CultureInfo.CurrentCulture.DateTimeFormat.FullDateTimePattern)}\n" +
                    $"KSP 2 ver {profile.GameVersion}\n" +
                    $"File ver {profile.FileVersion}";
            }
            else
            {
                lblDesc.text = "Old file version: some bindings might not be assigned as expected";
                lblDesc.color = Color.yellow;
            }
            _profiles.Add(name, profileElementObj);
        }

        private void RemoveElement(string name)
        {
            if (_profiles.TryGetValue(name, out var profileElement))
            {
                if (profileElement != null)
                    Destroy(profileElement);
                _profiles.Remove(name);
            }
        }

        private void SetDefault(string name)
        {
            var prevName = GlobalConfiguration.DefaultProfile;
            GlobalConfiguration.DefaultProfile = name;
            GlobalConfiguration.SaveDefaultProfile();
            if (_profiles.TryGetValue(prevName, out var prev))
            {
                prev?.transform.Find("GrpBtns/BtnDflt").gameObject.SetActive(true);
            }
            _profiles[name].transform.Find("GrpBtns/BtnDflt").gameObject.SetActive(false);
        }

        private void Load(string name)
        {
            GlobalConfiguration.Load();
            InputActionManager am = Inputbinder.Instance.ActionManager;
            am.ProfileName = name;
            am.LoadOverrides();
            Inputbinder.Instance.BindingUI.ChangeStatus(BindingUI.Status.Default);
        }

        private void ProfileDirWatcher_Deleted(object sender, FileSystemEventArgs e) => RemoveElement(Path.GetFileNameWithoutExtension(e.Name));

        private void ProfileDirWatcher_Renamed(object sender, RenamedEventArgs e) => RemoveElement(Path.GetFileNameWithoutExtension(e.OldName));

        private void OnDisable()
        {
            StopCoroutine(ProfileDirLoadCoroutine());
            _profileDirWatcher?.Dispose();
            for (var j = transform.Find("Content").childCount - 1; j >= 0; j--)
            {
                Destroy(transform.Find("Content").GetChild(j).gameObject);
            }
            _profiles.Clear();
        }
    }
}
