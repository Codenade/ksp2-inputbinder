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
using KSP.Sim;

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
        private bool _isThrottleAxisActive = false;

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
            SetupConfigDir();
            GlobalConfiguration.Load();
            _actionManager = new InputActionManager();
            _actionManager.LoadOverrides();

            GameManager.Instance.Game.Input.Flight.TrimModifier.Disable();
            for (var j = GameManager.Instance.Game.Input.Flight.TrimModifier.bindings.Count - 1; j >= 0; j--)
                GameManager.Instance.Game.Input.Flight.TrimModifier.ChangeBinding(j).Erase();
            GameManager.Instance.Game.Input.Flight.ThrottleDelta.OnStateChange(_ => _isThrottleAxisActive = false);
            GameManager.Instance.Game.Input.Flight.ThrottleMax.canceled += _ => ResetThrottle();
            GameManager.Instance.Game.Input.Flight.ThrottleCutoff.canceled += _ => ResetThrottle();
            _actionManager.Actions[Constants.ActionThrottleID].Action.OnStateChange(ctx => SetThrottleFromAxis(ctx.ReadValue<float>()));
            _actionManager.Actions[Constants.ActionTrimResetID].Action.performed += ctx => ResetTrim();
            _actionManager.Actions[Constants.ActionAPStabilityID].Action.performed += ctx => SetAPMode(AutopilotMode.StabilityAssist);
            _actionManager.Actions[Constants.ActionAPProgradeID].Action.performed += ctx => SetAPMode(AutopilotMode.Prograde);
            _actionManager.Actions[Constants.ActionAPRetrogradeID].Action.performed += ctx => SetAPMode(AutopilotMode.Retrograde);
            _actionManager.Actions[Constants.ActionAPNormalID].Action.performed += ctx => SetAPMode(AutopilotMode.Normal);
            _actionManager.Actions[Constants.ActionAPAntinormalID].Action.performed += ctx => SetAPMode(AutopilotMode.Antinormal);
            _actionManager.Actions[Constants.ActionAPRadialInID].Action.performed += ctx => SetAPMode(AutopilotMode.RadialIn);
            _actionManager.Actions[Constants.ActionAPRadialOutID].Action.performed += ctx => SetAPMode(AutopilotMode.RadialOut);
            _actionManager.Actions[Constants.ActionAPTargetID].Action.performed += ctx => SetAPMode(AutopilotMode.Target);
            _actionManager.Actions[Constants.ActionAPAntiTargetID].Action.performed += ctx => SetAPMode(AutopilotMode.AntiTarget);
            _actionManager.Actions[Constants.ActionAPManeuverID].Action.performed += ctx => SetAPMode(AutopilotMode.Maneuver);
            _actionManager.Actions[Constants.ActionAPNavigationID].Action.performed += ctx => SetAPMode(AutopilotMode.Navigation);
            _actionManager.Actions[Constants.ActionAPAutopilotID].Action.performed += ctx => SetAPMode(AutopilotMode.Autopilot);
            _actionManager.Actions[Constants.ActionThrottleID].Action.Enable();
            _actionManager.Actions[Constants.ActionPitchTrimID].Action.Enable();
            _actionManager.Actions[Constants.ActionRollTrimID].Action.Enable();
            _actionManager.Actions[Constants.ActionYawTrimID].Action.Enable();
            _actionManager.Actions[Constants.ActionTrimResetID].Action.Enable();
            _actionManager.Actions[Constants.ActionAPStabilityID].Action.Enable();
            _actionManager.Actions[Constants.ActionAPProgradeID].Action.Enable();
            _actionManager.Actions[Constants.ActionAPRetrogradeID].Action.Enable();
            _actionManager.Actions[Constants.ActionAPNormalID].Action.Enable();
            _actionManager.Actions[Constants.ActionAPAntinormalID].Action.Enable();
            _actionManager.Actions[Constants.ActionAPRadialInID].Action.Enable();
            _actionManager.Actions[Constants.ActionAPRadialOutID].Action.Enable();
            _actionManager.Actions[Constants.ActionAPTargetID].Action.Enable();
            _actionManager.Actions[Constants.ActionAPAntiTargetID].Action.Enable();
            _actionManager.Actions[Constants.ActionAPManeuverID].Action.Enable();
            _actionManager.Actions[Constants.ActionAPNavigationID].Action.Enable();
            _actionManager.Actions[Constants.ActionAPAutopilotID].Action.Enable();
            _bindingUI = gameObject.AddComponent<BindingUI>();
            _bindingUI.Hide();
            _bindingUI.VisibilityChanged += OnUiVisibilityChange;
            _isInitialized = true;
            Initialized?.Invoke();
        }

        private void SetupConfigDir()
        {
            Directory.CreateDirectory(Path.Combine(BepInEx.Paths.ConfigPath, "inputbinder"));
            Directory.CreateDirectory(Path.Combine(BepInEx.Paths.ConfigPath, "inputbinder/profiles"));
            if (File.Exists(Path.Combine(_modRootPath, "input.json")))
                File.Move(Path.Combine(_modRootPath, "input.json"), Path.Combine(BepInEx.Paths.ConfigPath, "inputbinder", "profiles", "input.json"));
        }

        private void Awake()
        {
            DontDestroyOnLoad(this);
            _modRootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (_modRootPath == null || _modRootPath == string.Empty)
                QLog.Error($"ModRootPath empty!");
            StopKSPFromRemovingGamepads();
            RemoveKSPsGamepadBindings();
            if (_notFirstLoad)
            {
                Initialize();
                VehicleStateChanged(null);
            }
            else
            {
                InputSystem.RegisterProcessor<Processors.MapProcessor>("Map");
                InputSystem.RegisterBindingComposite<Composites.Vector2AxisComposite>("2DAxis");
                InputSystem.settings.defaultDeadzoneMin = 0f;
                Game.Messages.Subscribe<GameStateEnteredMessage>(OnGameStateEntered);
                StartCoroutine(LoadCatalog());
                _notFirstLoad = true;
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

        /// <summary>
        /// If the throttle axis is in use, reset the throttle to its value.
        /// </summary>
        public void ResetThrottle()
        {
            if (_isThrottleAxisActive)
            {
                SetThrottle(_actionManager.Actions[Constants.ActionThrottleID].Action.ReadValue<float>(), true);
            }
        }

        /// <summary>
        /// Sets the throttle axis as in use and sets the throttle to value.
        /// </summary>
        /// <param name="value">The throttle value, 0-1</param>
        public void SetThrottleFromAxis(float value)
        {
            _isThrottleAxisActive = true;
            SetThrottle(value, false);
        }

        /// <summary>
        /// Sets the throttle level of the active vessel.
        ///
        /// By default, this does nothing if the throttle cutoff or max buttons are pressed.
        /// Set `force` to `true` to override this behavior.
        /// </summary>
        /// <param name="value">The throttle level, clamped between 0 and 1.</param>
        /// <param name="force">`true` proceeds setting the throttle level even when the throttle cutoff and max input buttons are pressed.</param>
        public void SetThrottle(float value, bool force = false)
        {
            // Prioritize throttle cutoff and max buttons over a throttle axis input.
            var flight = GameManager.Instance.Game.Input.Flight;
            if (!force && (flight.ThrottleCutoff.IsPressed() || flight.ThrottleMax.IsPressed()))
                return;
            _vessel?.ApplyFlightCtrlState(new KSP.Sim.State.FlightCtrlStateIncremental() { mainThrottle = Mathf.Clamp01(value) });
        }

        public void SetAPMode(AutopilotMode mode)
        {
            _vessel?.SetAutopilotMode(mode);
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
                pitchTrim = _vessel.flightCtrlState.pitchTrim + _actionManager.Actions[Constants.ActionPitchTrimID].Action.ReadValue<float>() * Time.unscaledDeltaTime,
                rollTrim = _vessel.flightCtrlState.rollTrim + _actionManager.Actions[Constants.ActionRollTrimID].Action.ReadValue<float>() * Time.unscaledDeltaTime,
                yawTrim = _vessel.flightCtrlState.yawTrim + _actionManager.Actions[Constants.ActionYawTrimID].Action.ReadValue<float>() * Time.unscaledDeltaTime
            });
        }

        private static void StopKSPFromRemovingGamepads()
        {
            QLog.Info($"Stopping KSP from automatically removing Gamepads...");
            var eventInfo = typeof(InputSystem).GetEvent(nameof(InputSystem.onDeviceChange), BindingFlags.Static | BindingFlags.Public);
            var method = typeof(InputManager).GetMethod("RemoveGamepadCallback", BindingFlags.NonPublic | BindingFlags.Instance);
            var handler = Delegate.CreateDelegate(eventInfo.EventHandlerType, GameManager.Instance.Game.InputManager, method);
            eventInfo.RemoveEventHandler(null, handler);
        }

        public void RemoveKSPsGamepadBindings()
        {
            QLog.Info($"Removing KSP's Gamepad bindings...");
            foreach (var action in GameManager.Instance.Game.Input)
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
                if (!(_bindingUI.IsInitialized || _bindingUI.IsInitializing))
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
