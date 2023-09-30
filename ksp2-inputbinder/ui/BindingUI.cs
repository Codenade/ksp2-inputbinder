using KSP.Game;
using KSP.IO;
using KSP.UI;
using KSP.UserInterface;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Codenade.Inputbinder
{
    public class BindingUI : MonoBehaviour
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
        public Dictionary<string, GameObject> Assets { get; private set; }
        public GameObject ContentRoot { get; private set; }
        public Status CurrentStatus { get; private set; }
        public Vector2 WindowPosition
        {
            get => _uiWindow.gameObject.GetComponent<RectTransform>().anchoredPosition;
            set => _uiWindow.gameObject.GetComponent<RectTransform>().anchoredPosition = value;
        }

        private InputActionManager _actionManager;
        private string _modRootPath;
        private KSP2UIWindow _uiWindow;
        private bool _allPrefabsLoaded;
        private bool _allPrefabsQueued;
        private int _operationsInProgress;
        private GameObject _uiMain;
        private GameObject _uiOverlayRebind;
        private GameObject _uiPage1;
        private GameObject _uiPage2;
        private GameObject _uiPage3;
        private float _scrollRectPosition;

        public void Initialize(Transform parent)
        {
            IsInitializing = true;
            GameManager.Instance.Assets.CreateAsync<GameObject>(
                (Attribute.GetCustomAttribute(typeof(KSP2UIWindow), typeof(PrefabNameAttribute)) as PrefabNameAttribute).Prefab,
                parent,
                (result) =>
                {
                    _uiWindow = result.GetComponent<KSP2UIWindow>();
                    Initialized();
                });
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
            if (_uiWindow is null || !_allPrefabsLoaded)
                return;
            if (!enabled)
                _uiWindow.gameObject.SetActive(false);
            WindowPosition = new Vector2(400, 0);
            ContentRoot = _uiWindow.gameObject.
                GetChild("Root").
                GetChild("UIPanel").
                GetChild("GRP-Body").
                GetChild("Scroll View").
                GetChild("Viewport").
                GetChild("Content");
            IsInitializing = false;
            IsInitialized = true;
            InitializationFinished?.Invoke();
        }

        private void Awake()
        {
            Assets = new Dictionary<string, GameObject>();
            _operationsInProgress = 0;
            _allPrefabsQueued = false;
            _allPrefabsLoaded = false;
            _actionManager = Inputbinder.Instance.ActionManager;
            _modRootPath = Inputbinder.Instance.ModRootPath;
        }

        public void Show() => enabled = true;

        public void Hide() => enabled = false;

        private void LoadSettings() => Inputbinder.Reload();

        private void SaveSettings()
        {
            _actionManager.SaveOverrides(IOProvider.JoinPath(_modRootPath, "input.json"));
        }

        private void Setup()
        {
            if (!_uiWindow || !_allPrefabsLoaded)
                return;
            for (var j = ContentRoot.transform.childCount - 1; j >= 0 ; j--)
                DestroyImmediate(ContentRoot.transform.GetChild(j).gameObject);
            _uiWindow.gameObject.name = "BindingUI";
            _uiWindow.MinSize = new Vector2(480, 300);
            _uiWindow.MaxSize = new Vector2(1200, 800);
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
            var uiwindowcontent = _uiMain.transform.parent.gameObject;
            var scrollComponent = uiwindowcontent.transform.parent.parent.gameObject.GetComponent<ScrollRect>();
            scrollComponent.scrollSensitivity = 1f;
            scrollComponent.tag = "UI_SCROLL"; // Block the scrollwheel from zooming the camera view using the game's mechanic for it
            var header = scrollComponent.transform.parent.parent.gameObject.GetChild("GRP-Header");
            var saveBtn = Instantiate(Assets[PrefabKeys.ProcessorSaveButton], header.transform);
            saveBtn.transform.SetSiblingIndex(3);
            saveBtn.GetComponent<Button>().onClick.AddListener(SaveSettings);
            var saveBtnRT = saveBtn.GetComponent<RectTransform>();
            saveBtnRT.anchoredPosition = new Vector2(140, 20);
            saveBtnRT.sizeDelta = new Vector2(55, 30);
            var loadBtn = Instantiate(Assets[PrefabKeys.LoadBindingsButton], header.transform);
            loadBtn.transform.SetSiblingIndex(4);
            loadBtn.GetComponent<Button>().onClick.AddListener(LoadSettings);
            var loadBtnRT = loadBtn.GetComponent<RectTransform>();
            loadBtnRT.anchoredPosition = new Vector2(199, 20);
            loadBtnRT.sizeDelta = new Vector2(55, 30);
            var rmGpdBdgsBtn = Instantiate(Assets[PrefabKeys.RemoveGamepadBindingsButton], header.transform);
            rmGpdBdgsBtn.transform.SetSiblingIndex(5);
            rmGpdBdgsBtn.GetComponent<Button>().onClick.AddListener(Inputbinder.Instance.RemoveKSPsGamepadBindings);
            var rmGpdBdgBtnRT = rmGpdBdgsBtn.GetComponent<RectTransform>();
            rmGpdBdgBtnRT.anchoredPosition= new Vector2(320, 20);
            rmGpdBdgBtnRT.sizeDelta = new Vector2(180, 30);
            var closeBtn = header.GetChild("KSP2ButtonText");
            closeBtn.GetComponent<ButtonExtended>().onClick.AddListener(Hide);
            var windowTitle = header.GetChild("TXT-Title");
            windowTitle.GetComponent<TextMeshProUGUI>().text = "Inputbinder";
            var uiwc_rect = uiwindowcontent.GetComponent<RectTransform>();
            uiwc_rect.offsetMin = new Vector2(0, uiwc_rect.offsetMin.y);
            uiwc_rect.offsetMax = new Vector2(-20, uiwc_rect.offsetMax.y);
            var uiwc_csf = uiwindowcontent.GetComponent<ContentSizeFitter>();
            uiwc_csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            uiwc_csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var uiwc_vlg = uiwindowcontent.GetComponent<VerticalLayoutGroup>();
            uiwc_vlg.childControlWidth = true;
            uiwc_vlg.childControlHeight = true;
            uiwc_vlg.childAlignment = TextAnchor.UpperCenter;
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
            if (status != Status.Default && CurrentStatus == Status.Default)
            {
                var scrollRect = _uiMain.transform.parent.parent.parent.gameObject.GetComponent<ScrollRect>();
                _scrollRectPosition = scrollRect.verticalNormalizedPosition;
            }
            _uiMain.SetActive(false);
            _uiOverlayRebind.SetActive(false);
            _uiPage1.SetActive(false);
            _uiPage2.SetActive(false);
            _uiPage3.SetActive(false);
            switch (status)
            {
                case Status.Default:
                    _uiMain.SetActive(true);
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
            if (status == Status.Default && CurrentStatus != Status.Default)
            {
                StartCoroutine(SetScrollRectPosOnNextFrame());
            }
            CurrentStatus = status;
        }

        private IEnumerator SetScrollRectPosOnNextFrame()
        {
            yield return null;
            var scrollRect = _uiMain.transform.parent.parent.parent.gameObject.GetComponent<ScrollRect>();
            scrollRect.verticalNormalizedPosition = _scrollRectPosition;
        }

        private void OnEnable()
        {
            if (_uiWindow is object)
                _uiWindow.gameObject.SetActive(true);
            Setup();
            VisibilityChanged?.Invoke(true);
        }

        private void OnDisable()
        {
            if (_uiWindow is object)
                _uiWindow.gameObject.SetActive(false);
            _actionManager.CancelBinding();
            _actionManager.CompleteChangeProcessors();
            VisibilityChanged?.Invoke(false);
        }

        private void OnDestroy()
        {
            if (_uiWindow is object)
                Destroy(_uiWindow.gameObject);
        }

        public static class PrefabKeys
        {
            public static readonly string WindowBindingContent =        "Codenade.Inputbinder/BindingWindowContent";
            public static readonly string WindowProcessorsContent =     "Codenade.Inputbinder/ProcessorWindowContent";
            public static readonly string BindingGroup =                "Codenade.Inputbinder/BindingGroup";
            public static readonly string BindingGroupComposite =       "Codenade.Inputbinder/CompositeBindingGroup";
            public static readonly string ActionGroup =                 "Codenade.Inputbinder/ActionGroup";
            public static readonly string ProcessorGroup =              "Codenade.Inputbinder/ProcessorGroup";
            public static readonly string ProcessorAddGroup =           "Codenade.Inputbinder/ProcessorAddGroup";
            public static readonly string ProcessorValueGroup =         "Codenade.Inputbinder/ProcessorValueGroup";
            public static readonly string ProcessorValueGroupBool =     "Codenade.Inputbinder/ProcessorValueGroupBool";
            public static readonly string ProcessorSaveButton =         "Codenade.Inputbinder/ProcessorSaveButton";
            public static readonly string LoadBindingsButton =          "Codenade.Inputbinder/LoadBindingsButton";
            public static readonly string RemoveGamepadBindingsButton = "Codenade.Inputbinder/RemoveGamepadBindingsButton";
            public static readonly string CurrentlyBindingOverlay =     "Codenade.Inputbinder/CurrentlyBindingOverlay";

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
            Rebinding,
            ProcessorList,
            ProcessorAdd,
            ProcessorConfirm,
            ProcessorEdit
        }
    }
}
