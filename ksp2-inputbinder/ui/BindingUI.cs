using KSP.Game;
using KSP.UI;
using KSP.UserInterface;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
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
        public bool ResetAllBindingsDialogVisible => _uiOverlayResetAll.activeSelf;
        public Vector2 WindowPosition
        {
            get => _uiWindow.gameObject.GetComponent<RectTransform>().anchoredPosition;
            set => _uiWindow.gameObject.GetComponent<RectTransform>().anchoredPosition = value;
        }

        private InputActionManager _actionManager;
        private KSP2UIWindow _uiWindow;
        private bool _allPrefabsLoaded;
        private bool _allPrefabsQueued;
        private int _operationsInProgress;
        private GameObject _uiMain;
        private GameObject _uiOverlayRebind;
        private GameObject _uiPage1;
        private GameObject _uiPage2;
        private GameObject _uiPage3;
        private GameObject _uiOverlayResetAll;
        private GameObject _uiOverlaySaveAs;
        private GameObject _uiOverlayLoad;
        private GameObject _uiActionsList;
        private GameObject _uiSearchList;
        private SaveButtonBehaviour _btnSaveDrp;
        private TMP_InputField _searchIF;
        private Button _btnLoad;
        private Button _btnResetAll;
        private float _scrollRectPosition;

        public void Initialize(Transform parent)
        {
            if (parent is null)
            {
                QLog.Error("BindingUI.Initialize(): Argument parent is null .. not initializing.");
                return;
            }
            IsInitializing = true;
            if (_uiWindow is object)
                Destroy(_uiWindow);
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
            Setup();
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
        }

        public void Show() => enabled = true;

        public void Hide() => enabled = false;

        private void LoadSettings() => Inputbinder.Reload();

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
            _uiActionsList = Instantiate(new GameObject("ActionsList", typeof(SetupVerticalLayout)), _uiMain.transform);
            _uiActionsList.SetActive(false);
            _uiSearchList = Instantiate(new GameObject("SearchList", typeof(SetupVerticalLayout)), _uiMain.transform);
            _uiSearchList.SetActive(false);
            _uiOverlayRebind = Instantiate(Assets[PrefabKeys.CurrentlyBindingOverlay], ContentRoot.transform.parent.parent);
            _uiOverlayRebind.SetActive(false);
            _uiPage1 = Instantiate(Assets[PrefabKeys.WindowProcessorsContent].GetChild("Viewport").GetChild("Content").GetChild("Page1"), ContentRoot.transform);
            _uiPage1.SetActive(false);
            _uiPage2 = Instantiate(Assets[PrefabKeys.WindowProcessorsContent].GetChild("Viewport").GetChild("Content").GetChild("Page2"), ContentRoot.transform);
            _uiPage2.SetActive(false);
            _uiPage3 = Instantiate(Assets[PrefabKeys.WindowProcessorsContent].GetChild("Viewport").GetChild("Content").GetChild("Page3"), ContentRoot.transform);
            _uiPage3.SetActive(false);
            _uiOverlayResetAll = Instantiate(Assets[PrefabKeys.ConfirmResetAllBindingsOverlay], ContentRoot.transform.parent.parent);
            _uiOverlayResetAll.SetActive(false);
            _uiOverlayResetAll.AddComponent<ResetAllBindingsDialogBehaviour>();
            _uiOverlaySaveAs = Instantiate(Assets[PrefabKeys.SaveAsDialogOverlay], ContentRoot.transform.parent.parent);
            _uiOverlaySaveAs.SetActive(false);
            _uiOverlaySaveAs.AddComponent<SaveAsDialogBehaviour>();
            _uiOverlayLoad = Instantiate(Assets[PrefabKeys.LoadDialogOverlay], ContentRoot.transform.parent.parent);
            _uiOverlayLoad.SetActive(false);
            _uiOverlayLoad.AddComponent<ProfileLoadDialogBehaviour>();
            var uiwindowcontent = _uiMain.transform.parent.gameObject;
            var scrollComponent = uiwindowcontent.transform.parent.parent.gameObject.GetComponent<ScrollRect>();
            scrollComponent.scrollSensitivity = 1f;
            scrollComponent.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
            scrollComponent.tag = "UI_SCROLL"; // Block the scrollwheel from zooming the camera view using the game's mechanic for it
            var uipanel = scrollComponent.transform.parent.parent.gameObject;
            var uipanelL = uipanel.GetComponent<VerticalLayoutGroup>();
            uipanelL.reverseArrangement = true;
            uipanelL.childForceExpandHeight = false;
            // reordered children of uigroup to make the save button draw on top in its extended state
            var header = uipanel.GetChild("GRP-Header");            //foot
            var body = uipanel.GetChild("GRP-Body");                //resz
            var footer = uipanel.GetChild("GRP-Footer");            //body
            var resizehandle = uipanel.GetChild("ResizeHandle");    //head
            var toolbar1 = Instantiate(new GameObject("GRP-Toolbar1", typeof(LayoutElement), typeof(HorizontalLayoutGroup)), uipanel.transform);
            var toolbar2 = Instantiate(new GameObject("GRP-Toolbar2", typeof(HorizontalLayoutGroup)), uipanel.transform);
            toolbar1.AddComponent<Image>().color = new Color(0.0314f, 0.0471f, 0.051f, 0.7843f);
            toolbar2.AddComponent<Image>().color = new Color(0.0314f, 0.0471f, 0.051f, 0.7843f);
            body.transform.SetSiblingIndex(3);
            footer.transform.SetSiblingIndex(1);
            header.transform.SetSiblingIndex(4);
            toolbar1.transform.SetSiblingIndex(4);
            toolbar2.transform.SetSiblingIndex(4);
            body.GetComponent<LayoutElement>().flexibleHeight = 999999;
            toolbar1.GetComponent<LayoutElement>().preferredHeight = 30;
            var tb1L = toolbar1.GetComponent<HorizontalLayoutGroup>();
            var tb1RT = toolbar1.GetComponent<RectTransform>();
            tb1L.childForceExpandHeight = false;
            var tb2L = toolbar2.GetComponent<HorizontalLayoutGroup>();
            var tb2RT = toolbar2.GetComponent<RectTransform>();
            tb2L.childForceExpandHeight = false;
            var saveBtnDrp = Instantiate(Assets[PrefabKeys.SaveButtonDropdown], toolbar1.transform);
            _btnSaveDrp = saveBtnDrp.AddComponent<SaveButtonBehaviour>();
            _btnSaveDrp.Init();
            _btnSaveDrp.PrimaryClick += () => Inputbinder.Instance.ActionManager.SaveOverrides();
            _btnSaveDrp.SecondaryClick += () => ChangeStatus(Status.SaveDialog);
            var saveBtnRT = saveBtnDrp.GetComponent<RectTransform>();
            saveBtnRT.sizeDelta = new Vector2(80, 30);
            var loadBtn = Instantiate(Assets[PrefabKeys.LoadBindingsButton], toolbar1.transform);
            _btnLoad = loadBtn.GetComponent<Button>();
            _btnLoad.onClick.AddListener(() => ChangeStatus(Status.LoadDialog));
            var loadBtnRT = loadBtn.GetComponent<RectTransform>();
            loadBtnRT.sizeDelta = new Vector2(55, 30);
            var rstAllBdgsBtn = Instantiate(Assets[PrefabKeys.ResetAllBindingsButton], toolbar1.transform);;
            _btnResetAll = rstAllBdgsBtn.GetComponent<Button>();
            _btnResetAll.onClick.AddListener(RemoveAllBindingsButtonClicked);
            var rstAllBdgsBtnRT = rstAllBdgsBtn.GetComponent<RectTransform>();
            rstAllBdgsBtnRT.sizeDelta = new Vector2(70, 30);
            var searchBox = Instantiate(Assets[PrefabKeys.SearchBox], toolbar2.transform);
            var searchBoxRT = searchBox.GetComponent<RectTransform>();
            var searchBoxTF = searchBox.GetComponent<TMP_InputField>();
            _searchIF = searchBoxTF;
            searchBoxTF.onSelect.AddListener((_) => InputSystem.DisableDevice(Keyboard.current.device));
            searchBoxTF.onDeselect.AddListener((_) => InputSystem.EnableDevice(Keyboard.current.device));
            searchBoxTF.onValueChanged.AddListener((x) => SearchFor(x));
            //var rmGpdBdgsBtn = Instantiate(Assets[PrefabKeys.RemoveGamepadBindingsButton], header.transform);
            //rmGpdBdgsBtn.transform.SetSiblingIndex(5);
            //rmGpdBdgsBtn.GetComponent<Button>().onClick.AddListener(Inputbinder.Instance.RemoveKSPsGamepadBindings);
            //var rmGpdBdgBtnRT = rmGpdBdgsBtn.GetComponent<RectTransform>();
            //rmGpdBdgBtnRT.anchoredPosition= new Vector2(320, 20);
            //rmGpdBdgBtnRT.sizeDelta = new Vector2(180, 30);
            var closeBtn = header.GetChild("KSP2ButtonText");
            closeBtn.GetComponent<ButtonExtended>().onClick.AddListener(Hide);
            var windowTitle = header.GetChild("TXT-Title");
            windowTitle.GetComponent<TextMeshProUGUI>().text = "Inputbinder";
            var uiwc_rect = uiwindowcontent.GetComponent<RectTransform>();
            uiwc_rect.offsetMin = new Vector2(0, uiwc_rect.offsetMin.y);
            uiwc_rect.offsetMax = new Vector2(0, uiwc_rect.offsetMax.y);
            var uiwc_csf = uiwindowcontent.GetComponent<ContentSizeFitter>();
            uiwc_csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            uiwc_csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            var uiwc_vlg = uiwindowcontent.GetComponent<VerticalLayoutGroup>();
            uiwc_vlg.childControlWidth = true;
            uiwc_vlg.childControlHeight = true;
            uiwc_vlg.childAlignment = TextAnchor.UpperCenter;
            GameObject catObj = null;
            foreach (var item in _actionManager.OrganizedInputActionData)
            {
                if (item is Category cat)
                {
                    catObj = Instantiate(Assets[PrefabKeys.CategoryGroup], _uiActionsList.transform);
                    catObj.name = "Category_" + cat.FriendlyName;
                    var actionHeader = catObj.GetChild("CategoryHeader");
                    var actionName = actionHeader.GetChild("CategoryName").GetComponent<TextMeshProUGUI>();
                    actionName.text = cat.FriendlyName;
                    var actionExpBtn = actionHeader.GetChild("ExpandableButton");
                    catObj = catObj.GetChild("Actions");
                    actionExpBtn.AddComponent<ExpandCollapse>().Initialize(false, catObj);
                    continue;
                }
                else if (item is CategoryEnd)
                {
                    catObj = null;
                    continue;
                }
                else if (item is WrappedInputAction action)
                {
                    CreateInputActionElement(action, catObj?.transform);
                    continue;
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

        private void CreateInputActionElement(WrappedInputAction action, Transform parent = null)
        {
            var actionObj = Instantiate(Assets[PrefabKeys.ActionGroup], parent ?? _uiActionsList.transform);
            actionObj.name = action.InputAction.name;
            var actionHeader = actionObj.GetChild("ActionHeader");
            var actionName = actionHeader.GetChild("ActionName").GetComponent<TextMeshProUGUI>();
            actionName.text = action.FriendlyName;
            var actionExpBtn = actionHeader.GetChild("ExpandableButton");
            actionExpBtn.AddComponent<ExpandCollapse>().Initialize(false);
            var actionBindings = actionObj.GetChild("Bindings");
            for (var idx = 0; idx < action.InputAction.bindings.Count; idx++)
            {
                var binding = action.InputAction.bindings[idx];
                if (binding.isComposite)
                {
                    var compositeRoot = Instantiate(Assets[PrefabKeys.BindingGroupComposite], actionBindings.transform);
                    compositeRoot.AddComponent<CompositeBindingGroup>().Initialize(action.InputAction, idx);
                    var bindingsContainer = compositeRoot.GetChild("Bindings");
                    for (var i = idx + 1; i < action.InputAction.bindings.Count; i++)
                    {
                        var mBinding = action.InputAction.bindings[i];
                        if (mBinding.isComposite || !mBinding.isPartOfComposite)
                            break;
                        Instantiate(Assets[PrefabKeys.BindingGroup], bindingsContainer.transform).AddComponent<BindingGroup>().Initialize(action.InputAction, i);
                        idx = i;
                    }
                }
                else
                {
                    Instantiate(Assets[PrefabKeys.BindingGroup], actionBindings.transform).AddComponent<BindingGroup>().Initialize(action.InputAction, idx);
                }
            }
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
            _uiOverlayResetAll.SetActive(false);
            _uiOverlaySaveAs.SetActive(false);
            _uiOverlayLoad.SetActive(false);
            _uiPage1.SetActive(false);
            _uiPage2.SetActive(false);
            _uiPage3.SetActive(false);
            _btnResetAll.interactable = false;
            _btnLoad.interactable = false;
            _btnSaveDrp.interactable = false;
            switch (status)
            {
                case Status.Default:
                    _uiMain.SetActive(true);
                    _btnSaveDrp.interactable = true;
                    _btnLoad.interactable = true;
                    _btnResetAll.interactable = true;
                    SearchFor(_searchIF.text);
                    break;
                case Status.ResetDialog:
                    _uiMain.SetActive(true);
                    _uiOverlayResetAll.SetActive(true);
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
            if (status == Status.Default && CurrentStatus != Status.Default)
            {
                StartCoroutine(SetScrollRectPosOnNextFrame());
            }
            CurrentStatus = status;
        }

        public void SearchFor(string name)
        {
            if (!name.IsNullOrEmpty())
            {
                name = name.ToLower();
                for (var j = _uiSearchList.transform.childCount - 1; j >= 0; j--)
                    DestroyImmediate(_uiSearchList.transform.GetChild(j).gameObject);
                _uiActionsList.SetActive(false);
                var actions = _actionManager.OrganizedInputActionData;
                foreach (var c in actions)
                {
                    if (!(c is WrappedInputAction))
                        continue;
                    var a = (WrappedInputAction)c;
                    if (a.FriendlyName.ToLower().Contains(name))
                    {
                        CreateInputActionElementMarked(a, name, _uiSearchList.transform);
                        continue;
                    }
                    foreach (var b in a.InputAction.bindings)
                    {
                        if (b.effectivePath is null)
                            continue;
                        if (b.effectivePath.ToLower().Contains(name))
                        {
                            CreateInputActionElementMarked(a, name, _uiSearchList.transform);
                            break;
                        }
                    }
                }
                _uiSearchList.SetActive(true);
            }
            else
            {
                _uiActionsList.SetActive(true);
                _uiSearchList.SetActive(false);
            }
        }

        private void CreateInputActionElementMarked(WrappedInputAction action, string toMark, Transform parent = null)
        {
            var actionObj = Instantiate(Assets[PrefabKeys.ActionGroup], parent ?? _uiSearchList.transform);
            actionObj.name = action.InputAction.name;
            var actionHeader = actionObj.GetChild("ActionHeader");
            var actionName = actionHeader.GetChild("ActionName").GetComponent<TextMeshProUGUI>();
            actionName.text = Utils.MarkText(action.FriendlyName, toMark);
            var actionExpBtn = actionHeader.GetChild("ExpandableButton");
            actionExpBtn.AddComponent<ExpandCollapse>().Initialize(false);
            var actionBindings = actionObj.GetChild("Bindings");
            for (var idx = 0; idx < action.InputAction.bindings.Count; idx++)
            {
                var binding = action.InputAction.bindings[idx];
                if (binding.isComposite)
                {
                    var compositeRoot = Instantiate(Assets[PrefabKeys.BindingGroupComposite], actionBindings.transform);
                    compositeRoot.AddComponent<CompositeBindingGroup>().Initialize(action.InputAction, idx);
                    var bindingsContainer = compositeRoot.GetChild("Bindings");
                    for (var i = idx + 1; i < action.InputAction.bindings.Count; i++)
                    {
                        var mBinding = action.InputAction.bindings[i];
                        if (mBinding.isComposite || !mBinding.isPartOfComposite)
                            break;
                        Instantiate(Assets[PrefabKeys.BindingGroup], bindingsContainer.transform).AddComponent<BindingGroup>().Initialize(action.InputAction, i, toMark);
                        idx = i;
                    }
                }
                else
                {
                    Instantiate(Assets[PrefabKeys.BindingGroup], actionBindings.transform).AddComponent<BindingGroup>().Initialize(action.InputAction, idx, toMark);
                }
            }
        }

        private IEnumerator SetScrollRectPosOnNextFrame()
        {
            yield return null;
            var scrollRect = _uiMain.transform.parent.parent.parent.gameObject.GetComponent<ScrollRect>();
            scrollRect.verticalNormalizedPosition = _scrollRectPosition;
        }

        private void RemoveAllBindingsButtonClicked()
        {
            if (Inputbinder.Instance.ActionManager.IsCurrentlyRebinding || Inputbinder.Instance.ActionManager.IsChangingProc)
                return;
            ChangeStatus(Status.ResetDialog);
        }

        private void OnEnable()
        {
            if (!(IsInitialized || IsInitializing))
                Initialize(GameManager.Instance.Game.UI.GetPopupCanvas()?.transform);
            if (_uiWindow is object)
                _uiWindow.gameObject.SetActive(true);
            VisibilityChanged?.Invoke(true);
        }

        private void OnDisable()
        {
            if (_uiWindow is object)
                _uiWindow.gameObject.SetActive(false);
            if (!IsInitialized)
                return;
            ChangeStatus(Status.Default);
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
            public static readonly string CategoryGroup =                   "Codenade.Inputbinder/CategoryGroup";
            public static readonly string SearchBox =                       "Codenade.Inputbinder/SearchBox";

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
            ResetDialog,
            Rebinding,
            ProcessorList,
            ProcessorAdd,
            ProcessorConfirm,
            ProcessorEdit
        }
    }
}
