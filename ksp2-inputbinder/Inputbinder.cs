using KSP.Game;
using KSP.Messages;
using UnityEngine.InputSystem;
using UnityEngine;
using KSP.IO;
using System;
using KSP.Input;
using System.Reflection;
using KSP.Sim.impl;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections;
using System.IO;

namespace Codenade.Inputbinder
{
    public sealed class Inputbinder : KerbalMonoBehaviour
    {
        public static event Action Initialized;
        public InputActionManager ActionManager => _actionManager;
        public BindingUI BindingUI => _bindingUI;
        public static Inputbinder Instance => _instance;
        public string ModRootPath => _modRootPath;
        public bool IsInitialized => _isInitialized;

        private static Inputbinder _instance;
        private static bool _notFirstLoad;
        private VesselComponent _vessel;
        private InputActionManager _actionManager;
        private AppBarButton _button;
        private BindingUI _bindingUI;
        private string _modRootPath;
        private bool _isInitialized;

        public Inputbinder()
        {
            if (_instance is null)
                _instance = this;
            else
                Destroy(this);
        }

        private void OnGameStateEntered(MessageCenterMessage message)
        {
            GameStateEnteredMessage gameStateEnteredMessage = message as GameStateEnteredMessage;
            if (gameStateEnteredMessage.StateBeingEntered == GameState.MainMenu)
            {
                Initialize();
                Game.Messages.Unsubscribe<GameStateEnteredMessage>(OnGameStateEntered);
            }
        }

        private void Initialize()
        {
            RemoveKSPsGamepadBindings();
            _actionManager = InputActionManager.LoadFromJson(IOProvider.JoinPath(_modRootPath, "input.json"));
            foreach (var id in new string[] { Constants.ActionThrottleID, Constants.ActionTrimResetID })
            {
                if (!_actionManager.ContainsAction(id))
                {
                    var n_action = new InputAction(id);
                    n_action.AddBinding(path: null).WithName(id == Constants.ActionThrottleID ? "Axis" : "Button");
                    n_action.expectedControlType = id == Constants.ActionThrottleID ? "Axis" : "Button";
                    _actionManager.AddAction(n_action, id == Constants.ActionThrottleID ? "Throttle Axis" : "Reset Trim");
                }
            }
            foreach (var id in new string[] { Constants.ActionPitchTrimID, Constants.ActionRollTrimID, Constants.ActionYawTrimID })
            {
                if (_actionManager.TryGetAction(id, out var action))
                {
                    if (action.Action.bindings.Count < 4)
                        action.Action.AddBinding(path: null).WithName("Axis");
                    action.Action.expectedControlType = "Axis";
                }
                else
                {
                    var n_action = new InputAction(id);
                    n_action.AddCompositeBinding("1DAxis")
                        .With("negative", "")
                        .With("positive", "");
                    n_action.AddBinding(path: null).WithName("Axis");
                    n_action.expectedControlType = "Axis";
                    _actionManager.AddAction(n_action, id == Constants.ActionPitchTrimID ? "Pitch Trim" : (id == Constants.ActionYawTrimID ? "Yaw Trim" : "Roll Trim"));
                }
            }   
            foreach (var gameAction in GameInputUtils.Load(IOProvider.JoinPath(_modRootPath, "game_actions_to_add.txt")))
            {
                var inputAction = gameAction.Item1;
                if (gameAction.Item2)
                {
                    if (inputAction.expectedControlType == "Vector2")
                    {
                        for (var j = inputAction.bindings.Count - 1; j >= 0; j--)
                            inputAction.ChangeBinding(j).Erase();
                        inputAction.AddCompositeBinding("2DVector")
                            .With("left", "")
                            .With("right", "")
                            .With("down", "")
                            .With("up", "");
                        if (inputAction == GameManager.Instance.Game.Input.Flight.CameraZoom)
                        {
                            inputAction.AddCompositeBinding("2DAxis")
                                .With("x", "")
                                .With("y", "/Mouse/scroll/y");
                            try
                            {
                                var ovrd = inputAction.bindings[5];
                                ovrd.overrideProcessors = "ScaleVector2(x=0,y=0.0005)";
                                inputAction.ApplyBindingOverride(5, ovrd);
                            }
                            catch (Exception e)
                            {
                                QLog.Error(e);
                            }
                        }
                        else
                            inputAction.AddCompositeBinding("2DAxis")
                                .With("x", "")
                                .With("y", "");
                    }
                    else
                    {
                        bool contains_axis_binding = false;
                        foreach (var binding in inputAction.bindings)
                            if (binding.name == "Axis")
                                contains_axis_binding = true;
                        if (!contains_axis_binding)
                            inputAction.AddBinding(path: null).WithName("Axis");
                        _actionManager.ModifiedGameActionsAxis1D.Add(inputAction);
                    }
                }
                if (_actionManager.Actions.ContainsKey(inputAction.name))
                    continue;
                _actionManager.AddAction(inputAction, true);
            }
            _actionManager.Actions[Constants.ActionThrottleID].Action.performed += ctx => SetThrottle(ctx.ReadValue<float>());
            _actionManager.Actions[Constants.ActionThrottleID].Action.started += ctx => SetThrottle(ctx.ReadValue<float>());
            _actionManager.Actions[Constants.ActionThrottleID].Action.canceled += ctx => SetThrottle(ctx.ReadValue<float>());
            _actionManager.Actions[Constants.ActionTrimResetID].Action.performed += ctx => ResetTrim();
            _actionManager.Actions[Constants.ActionThrottleID].Action.Enable();
            _actionManager.Actions[Constants.ActionPitchTrimID].Action.Enable();
            _actionManager.Actions[Constants.ActionRollTrimID].Action.Enable();
            _actionManager.Actions[Constants.ActionYawTrimID].Action.Enable();
            _actionManager.Actions[Constants.ActionTrimResetID].Action.Enable();
            _bindingUI = gameObject.AddComponent<BindingUI>();
            _bindingUI.Hide();
            _bindingUI.VisibilityChanged += OnUiVisibilityChange;
            _isInitialized = true;
            Initialized?.Invoke();
        }

        private void Awake()
        {
            DontDestroyOnLoad(this);
            gameObject.tag = "Game Manager";
            _modRootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (_modRootPath == null || _modRootPath == string.Empty)
                QLog.Error($"ModRootPath empty!");
            StopKSPFromRemovingGamepads();
            RemoveKSPsGamepadBindings();
            if (!_notFirstLoad)
            {
                InputSystem.RegisterProcessor<Processors.MapProcessor>("Map");
                InputSystem.RegisterBindingComposite<Composites.Vector2AxisComposite>("2DAxis");
                InputSystem.settings.defaultDeadzoneMin = 0f;
                Game.Messages.Subscribe<GameStateEnteredMessage>(OnGameStateEntered);
                StartCoroutine(LoadCatalog());
                _notFirstLoad = true;
            }
            else
            {
                Initialize();
                VehicleStateChanged(null);
            }
        }

        private IEnumerator LoadCatalog()
        {
            AsyncOperationHandle<IResourceLocator> operation = Addressables.LoadContentCatalogAsync(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + IOProvider.DirectorySeparatorCharacter.ToString() + "addressables/catalog.json");
            yield return operation;
            if (operation.Status == AsyncOperationStatus.Failed)
                QLog.Error($"Failed to load addressables catalog!");
            else
                GameManager.Instance.Assets.RegisterResourceLocator(operation.Result);
            yield break;
        }

        private void OnUiVisibilityChange(bool visible)
        {
            if (_button is object)
                _button.State = visible;
        }

        public void SetThrottle(float value)
        {
            _vessel?.ApplyFlightCtrlState(new KSP.Sim.State.FlightCtrlStateIncremental() { mainThrottle = Mathf.Clamp01(value) });
        }

        public void ResetTrim()
        {
            _vessel?.ApplyFlightCtrlState(new KSP.Sim.State.FlightCtrlStateIncremental()
            {
                pitchTrim = 0,
                rollTrim = 0,
                yawTrim = 0
            });
        }

        private void Update()
        {
            _vessel?.ApplyFlightCtrlState(new KSP.Sim.State.FlightCtrlStateIncremental()
            {
                pitchTrim = _vessel.flightCtrlState.pitchTrim + _actionManager.Actions[Constants.ActionPitchTrimID].Action.ReadValue<float>() * Time.deltaTime,
                rollTrim = _vessel.flightCtrlState.rollTrim + _actionManager.Actions[Constants.ActionRollTrimID].Action.ReadValue<float>() * Time.deltaTime,
                yawTrim = _vessel.flightCtrlState.yawTrim + _actionManager.Actions[Constants.ActionYawTrimID].Action.ReadValue<float>() * Time.deltaTime
            });
        }

        private void StopKSPFromRemovingGamepads()
        {
            QLog.Info($"Stopping KSP from automatically removing Gamepads...");
            var eventInfo = typeof(InputSystem).GetEvent(nameof(InputSystem.onDeviceChange), BindingFlags.Static | BindingFlags.Public);
            var method = typeof(InputManager).GetMethod("RemoveGamepadCallback", BindingFlags.NonPublic | BindingFlags.Instance);
            var handler = Delegate.CreateDelegate(eventInfo.EventHandlerType, Game.InputManager, method);
            eventInfo.RemoveEventHandler(null, handler);
        }

        public void RemoveKSPsGamepadBindings()
        {
            QLog.Info($"Removing KSP's Gamepad bindings...");
            foreach (var action in Game.Input)
            {
                for (var i = 0; i < action.bindings.Count; i++)
                {
                    var bdg = action.bindings[i];
                    if (bdg.path is object && bdg.path.Contains("Gamepad") || bdg.overridePath is object && bdg.overridePath.Contains("XInputController"))
                    {
                        action.ChangeBinding(i).WithPath(null);
                        var ovrd = action.bindings[i];
                        ovrd.overridePath = null;
                        action.ApplyBindingOverride(i, ovrd);
                    }
                }
            }
        }

        private void OnEnable()
        {
            GameManager.Instance.Game.Messages.PersistentSubscribe<VesselChangingMessage>(VehicleStateChanged);
            GameManager.Instance.Game.Messages.PersistentSubscribe<VesselChangedMessage>(VehicleStateChanged);
        }

        private void OnDestroy()
        {
            _button?.Destroy();
        }

        private void OnDisable()
        {
            GameManager.Instance.Game.Messages.Unsubscribe<VesselChangingMessage>(VehicleStateChanged);
            GameManager.Instance.Game.Messages.Unsubscribe<VesselChangedMessage>(VehicleStateChanged);
        }

        private void VehicleStateChanged(MessageCenterMessage msg)
        {
            _vessel = Game.ViewController.GetActiveSimVessel();
            if (_vessel is object)
            {
                if (!_bindingUI.IsInitialized && !_bindingUI.IsInitializing)
                    _bindingUI.Initialize(Game.UI.GetPopupCanvas().transform);
                if (_button is null)
                {
                    _button = AppBarButton.CreateButton($"BTN-{Constants.ID}", Constants.Name, OnAppBarButtonClicked);
                    _button.Destroying += () => _button = null;
                    Assets.LoadRaw<Sprite>(Constants.AppBarIconAssetKey).Completed += op =>
                    {
                        if (_button is object)
                            _button.Icon = op.Result;
                    };
                    _button.State = _bindingUI.enabled;
                }
            }
            else
            {
                _button?.Destroy();
                _bindingUI.Hide();
            }
        }

        private void OnAppBarButtonClicked(bool state)
        {
            _bindingUI.enabled = state;
        }

        public static void Load()
        {
            if (_instance is object) Reload();
            else new GameObject("Codenade.Inputbinder", typeof(Inputbinder));
        }

        public static void Unload()
        {
            if (_instance is object)
            {
                _instance.gameObject.name += ".Old";
                Destroy(_instance.gameObject);
                _instance = null;
            }
        }

        public static void Reload()
        {
            if (_instance is null)
            {
                Load();
                return;
            }
            var oldInstance = _instance;
            _instance = null;
            oldInstance.gameObject.name += ".Old";
            new GameObject("Codenade.Inputbinder", typeof(Inputbinder));
            var wasVisible = oldInstance.BindingUI.IsVisible;
            _instance.BindingUI.InitializationFinished += () => _instance.BindingUI.IsVisible = wasVisible;
            Destroy(oldInstance.gameObject);
        }
    }
}
