using KSP.Game;
using KSP.UI;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Codenade.Inputbinder
{
    public class BindingUI : SettingsSubMenu
    {
        public event Action<bool> VisibilityChanged;
        public event Action InitializationFinished;

        public bool IsInitialized { get; private set; }
        public bool IsInitializing { get; private set; }
        public bool IsVisible
        {
            get => enabled;
            set => enabled = value;
        }
        public new Dictionary<string, GameObject> Assets { get; private set; }
        public GameObject ContentRoot { get; private set; }
        public Status CurrentStatus { get; private set; }

        private InputActionManager _actionManager;
        private bool _allPrefabsLoaded;
        private bool _allPrefabsQueued;
        private int _operationsInProgress;
        private GameObject _uiMain;
        private GameObject _uiOverlayRebind;
        private GameObject _uiPage1;
        private GameObject _uiPage2;
        private GameObject _uiPage3;
        private GameObject _uiOverlaySaveAs;
        private GameObject _uiOverlayLoad;
        private GameObject _grpBtns;
        private SaveButtonBehaviour _btnSaveDrp;
        private Button _btnLoad;

        public override void Revert() => _actionManager.RemoveAllOverrides();

        public void Initialize()
        {
            Assets = new Dictionary<string, GameObject>();
            _operationsInProgress = 0;
            _allPrefabsQueued = false;
            _allPrefabsLoaded = false;
            _actionManager = Inputbinder.Instance.ActionManager;
            IsInitializing = true;
            foreach (var key in PrefabKeys.AllKeys)
            {
                _operationsInProgress++;
                GameManager.Instance.Assets.LoadAssetAsync<GameObject>(key)
                    .Completed += operation => SinglePrefabLoadFinished(key, operation);
            }
            _allPrefabsQueued = true;
        }

        private void SinglePrefabLoadFinished(string key, AsyncOperationHandle<GameObject> operation)
        {
            if (operation.Status != AsyncOperationStatus.Succeeded)
            {
                QLog.Error($"Could not load asset {key}");
                return;
            }
            Assets.Add(key, operation.Result);
            _operationsInProgress--;
            if (_allPrefabsQueued && _operationsInProgress <= 0)
            {
                _allPrefabsLoaded = true;
                Initialized();
            }
        }

        private void Initialized()
        {
            if (!_allPrefabsLoaded)
                return;
            if (!enabled)
                gameObject.SetActive(false);
            ContentRoot = gameObject;
            Setup();
            _actionManager.RebindComplete += () => Game.SettingsMenuManager.ShowChangesAppliedNotification();
            IsInitializing = false;
            IsInitialized = true;
            InitializationFinished?.Invoke();
        }

        public void Show() => enabled = true;

        public void Hide() => enabled = false;

        private void Setup()
        {
            if (!_allPrefabsLoaded)
                return;
            for (var j = ContentRoot.transform.childCount - 1; j >= 0 ; j--)
                DestroyImmediate(ContentRoot.transform.GetChild(j).gameObject);
            gameObject.name = "BindingUI";
            _uiMain = Instantiate(new GameObject("Main", typeof(SetupVerticalLayout)), ContentRoot.transform);
            _uiMain.SetActive(false);
            _uiOverlayRebind = Instantiate(Assets[PrefabKeys.CurrentlyBindingOverlay], ContentRoot.transform.parent.parent);
            _uiOverlayRebind.SetActive(false);
            _uiPage1 = Instantiate(Assets[PrefabKeys.WindowProcessorsContent].GetChild("Viewport").GetChild("Content").GetChild("Page1"), ContentRoot.transform);
            _uiPage1.SetActive(false);
            _uiPage2 = Instantiate(Assets[PrefabKeys.WindowProcessorsContent].GetChild("Viewport").GetChild("Content").GetChild("Page2"), ContentRoot.transform);
            _uiPage2.SetActive(false);
            _uiPage3 = Instantiate(Assets[PrefabKeys.WindowProcessorsContent].GetChild("Viewport").GetChild("Content").GetChild("Page3"), ContentRoot.transform);
            _uiPage3.SetActive(false);
            _uiOverlaySaveAs = Instantiate(Assets[PrefabKeys.SaveAsDialogOverlay], ContentRoot.transform.parent.parent);
            _uiOverlaySaveAs.SetActive(false);
            _uiOverlaySaveAs.AddComponent<SaveAsDialogBehaviour>();
            _uiOverlayLoad = Instantiate(Assets[PrefabKeys.LoadDialogOverlay], ContentRoot.transform.parent.parent);
            _uiOverlayLoad.SetActive(false);
            _uiOverlayLoad.AddComponent<ProfileLoadDialogBehaviour>();
            _grpBtns = new GameObject("Inputbinder interaction");
            _grpBtns.transform.SetParent(Game.UI.GetPopupCanvas().transform.Find("SettingsMenu(Clone)/Frame/Body/Categories/Menu controls"));
            var btnsVlg = _grpBtns.AddComponent<VerticalLayoutGroup>();
            var saveBtnDrp = Instantiate(Assets[PrefabKeys.SaveButtonDropdown], _grpBtns.transform);
            _btnSaveDrp = saveBtnDrp.AddComponent<SaveButtonBehaviour>();
            _btnSaveDrp.Init();
            _btnSaveDrp.PrimaryClick += () => Inputbinder.Instance.ActionManager.SaveOverrides();
            _btnSaveDrp.SecondaryClick += () => ChangeStatus(Status.SaveDialog);
            var loadBtn = Instantiate(Assets[PrefabKeys.LoadBindingsButton], _grpBtns.transform);
            _btnLoad = loadBtn.GetComponent<Button>();
            _btnLoad.onClick.AddListener(() => ChangeStatus(Status.LoadDialog));
            foreach (var item in _actionManager.Actions)
            {
                var actionObj = Instantiate(Assets[PrefabKeys.ActionGroup], _uiMain.transform);
                actionObj.name = item.Value.Name;
                var actionHeader = actionObj.GetChild("ActionHeader");
                var actionName = actionHeader.GetChild("ActionName").GetComponent<TextMeshProUGUI>();
                actionName.text = item.Value.FriendlyName;
                var actionExpBtn = actionHeader.GetChild("ExpandableButton");
                actionExpBtn.AddComponent<ExpandCollapse>().Initialize(false);
                var actionBindings = actionObj.GetChild("Bindings");
                for (var idx = 0; idx < item.Value.Action.bindings.Count; idx++)
                {
                    var binding = item.Value.Action.bindings[idx];
                    if (binding.isComposite)
                    {
                        var compositeRoot = Instantiate(Assets[PrefabKeys.BindingGroupComposite], actionBindings.transform);
                        compositeRoot.AddComponent<CompositeBindingGroup>().Initialize(item.Value.Action, idx);
                        var bindingsContainer = compositeRoot.GetChild("Bindings");
                        for (var i = idx + 1; i < item.Value.Action.bindings.Count; i++)
                        {
                            var mBinding = item.Value.Action.bindings[i];
                            if (mBinding.isComposite || !mBinding.isPartOfComposite)
                                break;
                            Instantiate(Assets[PrefabKeys.BindingGroup], bindingsContainer.transform).AddComponent<BindingGroup>().Initialize(item.Value.Action, i);
                            idx = i;
                        }
                    }
                    else
                    {
                        Instantiate(Assets[PrefabKeys.BindingGroup], actionBindings.transform).AddComponent<BindingGroup>().Initialize(item.Value.Action, idx);
                    }
                }
            }
            _uiPage1.GetChild("ProcessorCancelButton").AddComponent<ProcCancelBehaviour>();
            _uiPage2.GetChild("ProcessorCancelButton").AddComponent<ProcCancelBehaviour>();
            _uiPage3.GetChild("ProcessorCancelButton").AddComponent<ProcCancelBehaviour>();
            _uiPage1.GetChild("ProcessorAddButton").AddComponent<ProcNextBehaviour>();
            _uiPage3.GetChild("ProcessorSaveButton").AddComponent<ProcNextBehaviour>();
            _uiPage1.GetChild("Processors").AddComponent<Page1ListPopulator>();
            _uiPage2.GetChild("Processors").AddComponent<Page2ListPopulator>();
            _uiPage3.GetChild("Values").AddComponent<Page3ValuesManager>();
            ChangeStatus(Status.Default);
        }

        public void ChangeStatus(Status status)
        {
            _uiMain.SetActive(false);
            _uiOverlayRebind.SetActive(false);
            _uiOverlaySaveAs.SetActive(false);
            _uiOverlayLoad.SetActive(false);
            _uiPage1.SetActive(false);
            _uiPage2.SetActive(false);
            _uiPage3.SetActive(false);
            _btnLoad.interactable = false;
            _btnSaveDrp.interactable = false;
            switch (status)
            {
                case Status.Default:
                    _uiMain.SetActive(true);
                    _btnSaveDrp.interactable = true;
                    _btnLoad.interactable = true;
                    break;
                case Status.SaveDialog:
                    _uiMain.SetActive(true);
                    _uiOverlaySaveAs.SetActive(true);
                    break;
                case Status.LoadDialog:
                    _uiMain.SetActive(true);
                    _uiOverlayLoad.SetActive(true);
                    break;
                case Status.Rebinding:
                    if (!Inputbinder.Instance.ActionManager.IsCurrentlyRebinding)
                    {
                        ChangeStatus(Status.Default);
                        return;
                    }
                    _uiOverlayRebind.transform.Find("Dialog").Find("Content").Find("Subtitle").GetComponent<TextMeshProUGUI>().text = Inputbinder.Instance.ActionManager.RebindInfo.ToString();
                    _uiMain.SetActive(true);
                    _uiOverlayRebind.SetActive(true);
                    break;
                case Status.ProcessorList:
                    _uiPage1.SetActive(true);
                    break;
                case Status.ProcessorAdd:
                    _uiPage2.SetActive(true);
                    break;
                case Status.ProcessorConfirm:
                    _uiPage3.SetActive(true);
                    break;
                case Status.ProcessorEdit:
                    _uiPage3.SetActive(true);
                    break;
            }
            CurrentStatus = status;
        }


        private void OnEnable()
        {
            gameObject.SetActive(true);
            _grpBtns.SetActive(true);
            VisibilityChanged?.Invoke(true);
        }

        private void OnDisable()
        {
            gameObject.SetActive(false);
            _grpBtns.SetActive(false);
            if (!IsInitialized)
                return;
            ChangeStatus(Status.Default);
            _actionManager.CancelBinding();
            _actionManager.CompleteChangeProcessors();
            VisibilityChanged?.Invoke(false);
        }

        public static class PrefabKeys
        {
            public static readonly string WindowBindingContent =            "Codenade.Inputbinder/BindingWindowContent";
            public static readonly string WindowProcessorsContent =         "Codenade.Inputbinder/ProcessorWindowContent";
            public static readonly string BindingGroup =                    "Codenade.Inputbinder/BindingGroup";
            public static readonly string BindingGroupComposite =           "Codenade.Inputbinder/CompositeBindingGroup";
            public static readonly string ActionGroup =                     "Codenade.Inputbinder/ActionGroup";
            public static readonly string ProcessorGroup =                  "Codenade.Inputbinder/ProcessorGroup";
            public static readonly string ProcessorAddGroup =               "Codenade.Inputbinder/ProcessorAddGroup";
            public static readonly string ProcessorValueGroup =             "Codenade.Inputbinder/ProcessorValueGroup";
            public static readonly string ProcessorValueGroupBool =         "Codenade.Inputbinder/ProcessorValueGroupBool";
            public static readonly string ProcessorSaveButton =             "Codenade.Inputbinder/ProcessorSaveButton";
            public static readonly string LoadBindingsButton =              "Codenade.Inputbinder/LoadBindingsButton";
            public static readonly string RemoveGamepadBindingsButton =     "Codenade.Inputbinder/RemoveGamepadBindingsButton";
            public static readonly string CurrentlyBindingOverlay =         "Codenade.Inputbinder/CurrentlyBindingOverlay";
            public static readonly string ResetAllBindingsButton =          "Codenade.Inputbinder/ResetAllBindingsButton";
            public static readonly string ConfirmResetAllBindingsOverlay =  "Codenade.Inputbinder/ConfirmResetAllDialogOverlay";
            public static readonly string SaveButtonDropdown =              "Codenade.Inputbinder/SaveButtonDropdown";
            public static readonly string SaveAsDialogOverlay =             "Codenade.Inputbinder/SaveAsDialogOverlay";
            public static readonly string ProfileElement =                  "Codenade.Inputbinder/ProfileElement";
            public static readonly string LoadDialogOverlay =               "Codenade.Inputbinder/LoadDialogOverlay";

            public static string[] AllKeys 
            { 
                get
                {
                    var fields = typeof(PrefabKeys).GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                    var arr = new string[fields.Length];
                    for (var i = 0; i < fields.Length; i++)
                        arr[i] = fields[i].GetValue(null) as string;
                    return arr;
                }
            }
        }

        public enum Status
        {
            Default,
            SaveDialog,
            LoadDialog,
            Rebinding,
            ProcessorList,
            ProcessorAdd,
            ProcessorConfirm,
            ProcessorEdit
        }
    }
}
