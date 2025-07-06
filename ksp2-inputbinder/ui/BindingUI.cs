using KSP.Game;
using System;
using System.Collections.Generic;
using UitkForKsp2.API;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UIElements;

namespace Codenade.Inputbinder.ui
{
    public partial class BindingUI : MonoBehaviour
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
        public Status CurrentStatus { get; private set; }
        public bool ResetAllBindingsDialogVisible => false;
        public Vector2 WindowPosition
        {
            get => _uitkWindow?.transform.position ?? Vector2.zero;
            set { if (_uitkWindow is object) _uitkWindow.transform.position = value; }
        }

        private InputActionManager _actionManager;
        private bool _allPrefabsLoaded;
        private bool _allPrefabsQueued;
        private int _operationsInProgress;

        private UIDocument _uitkWindow;
        private VisualElement _uitkActionsScreen;
        private VisualElement _uitkActionsContainer;
        private VisualElement _uitkStatusBar;
        private Label _uitkStatusBarLbl;
        private Dictionary<InputAction, ActionUpdater> _uitkActionsList = new Dictionary<InputAction, ActionUpdater>();
        private SearchOperation _so;

        private Button _btnBack;
        private Button _btnAdd;
        private Button _btnReset;
        private Button _btnSave;
        private Button _btnSaveAs;
        private Button _btnLoad;
        private Button _btnSettings;

        internal ProcessorEditMenu ProcessorEditMenu { get; private set; }
        internal IMenuPanel ActiveMenuPanel { get; set; }

        public void InitAssets()
        {
            // Begin loading assets
            foreach (var enfd in typeof(AssetDb).GetFields())
            {
                _operationsInProgress++;
                if (!(enfd.GetValue(null) is IAssetEntry entry)) continue;
                object ophandle = typeof(KSP.Assets.AssetProvider)
                    .GetMethod(nameof(KSP.Assets.AssetProvider.LoadAssetAsync))
                    .MakeGenericMethod(entry.Type)
                    .Invoke(GameManager.Instance.Assets, new object[] { entry.Key });
                typeof(AsyncOperationHandle<>).MakeGenericType(entry.Type)
                    .GetEvent(nameof(AsyncOperationHandle<object>.CompletedTypeless))
                    .AddEventHandler(ophandle, (Action<AsyncOperationHandle>)(operation => SinglePrefabLoadFinished(entry, operation)));
            }
            _allPrefabsQueued = true;
        }

        private void SinglePrefabLoadFinished(IAssetEntry entry, AsyncOperationHandle operation)
        {
            if (operation.Status != AsyncOperationStatus.Succeeded)
            {
                QLog.Error($"Could not load asset {entry.Key} of type {entry.Type}");
                return;
            }
            entry.EntryAsObject = operation.Result;
            _operationsInProgress--;
            if (_allPrefabsQueued && _operationsInProgress <= 0)
            {
                _allPrefabsLoaded = true;
                InitUi();
            }
        }

        private void InitUi()
        {
            if (!_allPrefabsLoaded) return;

            _uitkWindow = Window.Create(new WindowOptions()
            {
                IsHidingEnabled = true,
                MoveOptions = new MoveOptions()
                {
                    CheckScreenBounds = true,
                    IsMovingEnabled = true
                },
                DisableGameInputForTextFields = true
            }, AssetDb.MainWindowUITK);
            _uitkWindow.rootVisualElement.style.height = 400;
            _uitkWindow.rootVisualElement.style.width = 400;

            WindowPosition = new Vector2(400, 0);

            // Setup title bar
            _uitkWindow.rootVisualElement.Q<Button>("CloseButton").clicked += Hide;

            // Setup nav bar
            var navBar = _uitkWindow.rootVisualElement.Q<VisualElement>("NavBar");
            _btnBack = navBar.Q<Button>("BackBtn");
            _btnAdd = navBar.Q<Button>("AddBtn");
            _btnReset = navBar.Q<Button>("ResetAllBtn");
            _btnSave = navBar.Q<Button>("SaveBtn");
            _btnSaveAs = navBar.Q<Button>("SaveAsBtn");
            _btnLoad = navBar.Q<Button>("LoadBtn");
            _btnSettings = navBar.Q<Button>("SettingsBtn");

            _btnBack.clicked += () =>
            {
                bool result = ActiveMenuPanel?.Back() ?? true;
                if (result)
                {
                    ActiveMenuPanel = null;
                    ChangeStatus(Status.Default);
                }
            };

            // Setup status bar
            _uitkStatusBar = _uitkWindow.rootVisualElement.Q<VisualElement>("RebindWarning");
            _uitkStatusBarLbl = _uitkStatusBar.Q<Label>("RebindWarningText");

            // Setup processors screen
            ProcessorEditMenu = new ProcessorEditMenu(_uitkWindow.rootVisualElement.Q<VisualElement>("ProcessorEditScreen"), _btnAdd);

            // Setup load profile screen


            // Setup save profile screen


            // Setup settings screen


            // Setup actions screen
            _uitkActionsScreen = _uitkWindow.rootVisualElement.Q<VisualElement>("ActionsScreen");
            _uitkActionsContainer = _uitkActionsScreen.Q<VisualElement>("unity-content-container");

            VisualElement catContainer = null;
            foreach (var item in _actionManager.OrganizedInputActionData)
            {
                if (item is Category cat)
                {
                    catContainer = AssetDb.ActionCategoryUITK.Entry.Instantiate();
                    _uitkActionsContainer.Add(catContainer);
                    catContainer.name = "Category_" + cat.FriendlyName;
                    var actionHeader = catContainer.Q<Foldout>();
                    actionHeader.text = cat.FriendlyName;
                    catContainer = actionHeader.Q<VisualElement>("unity-content");
                    continue;
                }
                else if (item is CategoryEnd)
                {
                    catContainer = null;
                    continue;
                }
                else if (item is WrappedInputAction action)
                {
                    CreateInputActionElement(action, catContainer);
                    continue;
                }
            }

            // Set status once
            ChangeStatus(Status.Default);

            // After this step we are done initializing
            IsInitializing = false;
            IsInitialized = true;
            InitializationFinished?.Invoke();

            Show();
        }

        private void Awake()
        {
            _operationsInProgress = 0;
            _allPrefabsQueued = false;
            _allPrefabsLoaded = false;
            _actionManager = Inputbinder.Instance.ActionManager;
        }

        public void Show() => enabled = true;

        public void Hide() => enabled = false;

        private void LoadSettings() => Inputbinder.Reload();

        private void CreateInputActionElement(WrappedInputAction action, VisualElement parent = null)
        {
            VisualElement actionObj = AssetDb.InputActionUITK.Entry.Instantiate();
            var actionUpdater = new ActionUpdater(action, actionObj);
            _uitkActionsList.Add(action.InputAction, actionUpdater);
            (parent ?? _uitkActionsContainer).Add(actionObj);
            actionObj.name = "Action_" + action.InputAction.name;
            actionObj = actionObj.Q<VisualElement>("unity-content");

            for (var idx = 0; idx < action.InputAction.bindings.Count; idx++)
            {
                var binding = action.InputAction.bindings[idx];
                if (binding.isComposite)
                {
                    var compositeRoot = AssetDb.CompositeBindingUITK.Entry.Instantiate();
                    actionObj.Add(compositeRoot);
                    actionUpdater.BindingUpdaters.Add(new CompositeUpdater(action, idx, compositeRoot));
                    var bindingsContainer = compositeRoot.Q<VisualElement>("SubBiContainer");
                    for (var i = idx + 1; i < action.InputAction.bindings.Count; i++)
                    {
                        var mBinding = action.InputAction.bindings[i];
                        if (mBinding.isComposite || !mBinding.isPartOfComposite)
                            break;
                        var bindingRoot = AssetDb.SingleBindingUITK.Entry.Instantiate();
                        bindingsContainer.Add(bindingRoot);
                        actionUpdater.BindingUpdaters.Add(new BindingUpdater(action, i, bindingRoot));
                        idx = i;
                    }
                }
                else
                {
                    var bindingRoot = AssetDb.SingleBindingUITK.Entry.Instantiate();
                    actionObj.Add(bindingRoot);
                    actionUpdater.BindingUpdaters.Add(new BindingUpdater(action, idx, bindingRoot));
                }
            }
        }

        public void ChangeStatus(Status status)
        {
            if (status != Status.Default && status != Status.Rebinding) _uitkActionsScreen.style.display = DisplayStyle.None;
            switch (status)
            {
                case Status.Default:
                    _uitkActionsScreen.style.display = DisplayStyle.Flex;
                    _btnAdd.style.display = DisplayStyle.None;
                    _btnBack.style.display = DisplayStyle.None;
                    _btnLoad.style.display = DisplayStyle.Flex;
                    _btnReset.style.display = DisplayStyle.Flex;
                    _btnSave.style.display = DisplayStyle.Flex;
                    _btnSaveAs.style.display = DisplayStyle.Flex;
                    _btnSettings.style.display = DisplayStyle.Flex;
                    break;
                case Status.ResetDialog:
                    _btnAdd.style.display = DisplayStyle.None;
                    _btnBack.style.display = DisplayStyle.Flex;
                    _btnLoad.style.display = DisplayStyle.None;
                    _btnReset.style.display = DisplayStyle.None;
                    _btnSave.style.display = DisplayStyle.None;
                    _btnSaveAs.style.display = DisplayStyle.None;
                    _btnSettings.style.display = DisplayStyle.None;
                    break;
                case Status.SaveDialog:
                    _btnAdd.style.display = DisplayStyle.None;
                    _btnBack.style.display = DisplayStyle.Flex;
                    _btnLoad.style.display = DisplayStyle.None;
                    _btnReset.style.display = DisplayStyle.None;
                    _btnSave.style.display = DisplayStyle.Flex;
                    _btnSaveAs.style.display = DisplayStyle.None;
                    _btnSettings.style.display = DisplayStyle.None;
                    break;
                case Status.LoadDialog:
                    _btnAdd.style.display = DisplayStyle.None;
                    _btnBack.style.display = DisplayStyle.Flex;
                    _btnLoad.style.display = DisplayStyle.None;
                    _btnReset.style.display = DisplayStyle.None;
                    _btnSave.style.display = DisplayStyle.None;
                    _btnSaveAs.style.display = DisplayStyle.None;
                    _btnSettings.style.display = DisplayStyle.None;
                    break;
                case Status.Rebinding:
                    if (!Inputbinder.Instance.ActionManager.IsCurrentlyRebinding)
                    {
                        ChangeStatus(Status.Default);
                        return;
                    }
                    _btnAdd.style.display = DisplayStyle.None;
                    _btnBack.style.display = DisplayStyle.Flex;
                    _btnLoad.style.display = DisplayStyle.None;
                    _btnReset.style.display = DisplayStyle.None;
                    _btnSave.style.display = DisplayStyle.None;
                    _btnSaveAs.style.display = DisplayStyle.None;
                    _btnSettings.style.display = DisplayStyle.None;
                    _uitkStatusBarLbl.text = $"Rebinding {Inputbinder.Instance.ActionManager.RebindInfo}";
                    _uitkStatusBar.Show();
                    
                    break;
                case Status.ProcessorAdd:
                    _btnAdd.style.display = DisplayStyle.None;
                    _btnBack.style.display = DisplayStyle.Flex;
                    _btnLoad.style.display = DisplayStyle.None;
                    _btnReset.style.display = DisplayStyle.None;
                    _btnSave.style.display = DisplayStyle.None;
                    _btnSaveAs.style.display = DisplayStyle.None;
                    _btnSettings.style.display = DisplayStyle.None;
                    break;
                case Status.ProcessorEdit:
                    _btnAdd.style.display = DisplayStyle.Flex;
                    _btnBack.style.display = DisplayStyle.Flex;
                    _btnLoad.style.display = DisplayStyle.None;
                    _btnReset.style.display = DisplayStyle.None;
                    _btnSave.style.display = DisplayStyle.None;
                    _btnSaveAs.style.display = DisplayStyle.None;
                    _btnSettings.style.display = DisplayStyle.None;
                    break;
            }
            CurrentStatus = status;
        }

        public void SearchFor(string name)
        {
            if (!name.IsNullOrEmpty())
            {
                if (_so is object) _so.Query = name;
                else _so = new SearchOperation(name);
                name = name.ToLower();
                var actions = _actionManager.OrganizedInputActionData;
                foreach (var c in actions)
                {
                    if (!(c is WrappedInputAction))
                        continue;
                    var a = (WrappedInputAction)c;
                    if (a.FriendlyName.ToLower().Contains(name))
                    {
                        _uitkActionsList[a.InputAction].Mark(_so);
                        continue;
                    }
                    else
                    {
                        if (_uitkActionsList.TryGetValue(a.InputAction, out var au)) au.Hide();
                    }
                    foreach (var b in a.InputAction.bindings)
                    {
                        if (b.effectivePath is null)
                            continue;
                        if (b.effectivePath.ToLower().Contains(name))
                        {
                            _uitkActionsList[a.InputAction].Mark(_so);
                            break;
                        }
                    }
                }
            }
            else
            {
                foreach (var ac in _uitkActionsList.Values) ac.Show();
            }
        }

        private void RemoveAllBindingsButtonClicked()
        {
            if (Inputbinder.Instance.ActionManager.IsCurrentlyRebinding)
                return;
            ChangeStatus(Status.ResetDialog);
        }

        private void OnEnable()
        {
            if (!(IsInitialized || IsInitializing))
            {
                IsInitializing = true;
                if (!_allPrefabsLoaded) InitAssets();
                else InitUi();
            }

            _uitkWindow?.Show();

            VisibilityChanged?.Invoke(true);
        }

        private void OnDisable()
        {
            if (!IsInitialized)
                return;
            ChangeStatus(Status.Default);
            _actionManager.CancelBinding();
            ProcessorEditMenu.Back();

            _uitkWindow?.Hide();

            VisibilityChanged?.Invoke(false);
        }

        private void OnDestroy()
        {
            if (_uitkWindow is object)
                Destroy(_uitkWindow);
        }

        public enum Status
        {
            Default,
            Rebinding,
            ProcessorAdd,
            ProcessorEdit,
            SaveDialog,
            LoadDialog,
            ResetDialog,
            Settings
        }
    }
}
