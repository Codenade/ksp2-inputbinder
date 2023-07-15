using KSP.Game;
using KSP.Logging;
using KSP.Messages;
using UnityEngine.InputSystem;
using UnityEngine;
using KSP.Modding;
using KSP.IO;
using System;
using KSP.Input;
using System.Reflection;
using KSP.Sim.impl;

namespace Codenade.Inputbinder
{
    public sealed class Inputbinder : KerbalMonoBehaviour
    {
        public InputActionManager ActionManager => _actionManager;
        public KSP2Mod Mod => _mod;
        public BindingUI BindingUI => _bindingUI;
        public static Inputbinder Instance => _instance;

        private static Inputbinder _instance;
        private static bool _notFirstLoad;
        private KSP2Mod _mod;
        private VesselComponent _vessel;
        private InputActionManager _actionManager;
        private AppBarButton _button;
        private BindingUI _bindingUI;

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
            _actionManager = InputActionManager.LoadFromJson(IOProvider.JoinPath(_mod.ModRootPath, "input.json"));
            if (_actionManager.Actions.Count == 0)
            {
                var action = new InputAction(Constants.ActionThrottleID);
                action.AddBinding("")
                    .WithName("binding");
                action.expectedControlType = "Axis";
                _actionManager.Add(action, "Throttle Axis");
                action = new InputAction(Constants.ActionPitchTrimID);
                action.AddCompositeBinding("1DAxis")
                    .With("negative", "")
                    .With("positive", "");
                action.expectedControlType = "Axis";
                _actionManager.Add(action, "Pitch Trim");
                action = new InputAction(Constants.ActionRollTrimID);
                action.AddCompositeBinding("1DAxis")
                    .With("negative", "")
                    .With("positive", "");
                action.expectedControlType = "Axis";
                _actionManager.Add(action, "Roll Trim");
                action = new InputAction(Constants.ActionYawTrimID);
                action.AddCompositeBinding("1DAxis")
                    .With("negative", "")
                    .With("positive", "");
                action.expectedControlType = "Axis";
                _actionManager.Add(action, "Yaw Trim");
                action = new InputAction(Constants.ActionTrimResetID);
                action.AddBinding("")
                    .WithName("binding");
                action.expectedControlType = "Button";
                _actionManager.Add(action, "Reset Trim");
            }
            var gameActionsToAdd = GameInputUtils.Load(IOProvider.JoinPath(_mod.ModRootPath, "game_actions_to_add.txt"));
            foreach (var gameAction in gameActionsToAdd)
            {
                if (_actionManager.Actions.ContainsKey(gameAction.name))
                    continue;
                _actionManager.Add(gameAction, true);
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
        }

        private void Awake()
        {
            DontDestroyOnLoad(this);
            gameObject.tag = "Game Manager";
            foreach (var mod in Game.KSP2ModManager.CurrentMods)
                if (mod.ModName == Constants.Name && mod.ModAuthor == Constants.Author)
                    _mod = mod;
            StopKSPFromRemovingGamepads();
            RemoveKSPsGamepadBindings();
            if (!_notFirstLoad)
            {
                InputSystem.RegisterProcessor<Processors.MapProcessor>("Map");
                InputSystem.settings.defaultDeadzoneMin = 0f;
                Game.Messages.Subscribe<GameStateEnteredMessage>(OnGameStateEntered);
                _notFirstLoad = true;
            }
            else
            {
                Initialize();
                VehicleStateChanged(null);
            }
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
            GlobalLog.Log(LogFilter.UserMod, $"[{Constants.Name}] Stopping KSP from automatically removing Gamepads...");
            var eventInfo = typeof(InputSystem).GetEvent(nameof(InputSystem.onDeviceChange), BindingFlags.Static | BindingFlags.Public);
            var method = typeof(InputManager).GetMethod("RemoveGamepadCallback", BindingFlags.NonPublic | BindingFlags.Instance);
            var handler = Delegate.CreateDelegate(eventInfo.EventHandlerType, Game.InputManager, method);
            eventInfo.RemoveEventHandler(null, handler);
        }

        public void RemoveKSPsGamepadBindings()
        {
            GlobalLog.Log(LogFilter.UserMod, $"[{Constants.Name}] Removing KSP's Gamepad bindings...");
            foreach (var action in Game.Input)
            {
                for (var i = 0; i < action.bindings.Count; i++)
                {
                    var bdg = action.bindings[i];
                    if (bdg.effectivePath.Contains("Gamepad") || bdg.effectivePath.Contains("XInputController"))
                    {
                        action.ChangeBinding(i).WithPath("");
                        action.ApplyBindingOverride(i, "");
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
                    Game.Assets.LoadRaw<Sprite>(Constants.AppBarIconAssetKey).Completed += op =>
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
