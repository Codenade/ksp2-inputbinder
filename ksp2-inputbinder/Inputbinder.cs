using KSP.Game;
using KSP.Logging;
using KSP.Messages;
using UnityEngine.InputSystem;
using UnityEngine;
using KSP.Modding;
using KSP.IO;
using System.Collections.Generic;
using KSP;
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

        private void Awake()
        {
            DontDestroyOnLoad(this);
            foreach (var mod in Game.KSP2ModManager.CurrentMods)
                if (mod.ModName == Constants.Name && mod.ModAuthor == Constants.Author)
                    _mod = mod;
            InputSystem.RegisterProcessor<Processors.MapProcessor>("Map");
            InputSystem.settings.defaultDeadzoneMin = 0f;
            StopKSPFromRemovingGamepads();
            RemoveKSPsGamepadBindings();
            _actionManager = InputActionManager.LoadFromJson(IOProvider.JoinPath(_mod.ModRootPath, "input.json"));
            var gameActionsToAdd = new List<InputAction>()
            {
                Game.Input.Flight.Pitch,
                Game.Input.Flight.Roll,
                Game.Input.Flight.Yaw,
                Game.Input.Flight.ToggleLandingGear,
                Game.Input.Flight.WheelSteer,
                Game.Input.Flight.WheelBrakes,
                Game.Input.Flight.WheelThrottle
            };
            foreach (var gameAction in gameActionsToAdd)
            {
                if (_actionManager.Actions.ContainsKey(gameAction.name))
                    continue;
                _actionManager.Add(gameAction, true);
            }
            _actionManager.Actions["inputbinder/throttle_axis"].Action.performed += ctx => _vessel?.ApplyFlightCtrlState(new KSP.Sim.State.FlightCtrlStateIncremental() { mainThrottle = ctx.ReadValue<float>() });
            _actionManager.Actions["inputbinder/throttle_axis"].Action.started += ctx => _vessel?.ApplyFlightCtrlState(new KSP.Sim.State.FlightCtrlStateIncremental() { mainThrottle = ctx.ReadValue<float>() });
            _actionManager.Actions["inputbinder/throttle_axis"].Action.canceled += ctx => _vessel?.ApplyFlightCtrlState(new KSP.Sim.State.FlightCtrlStateIncremental() { mainThrottle = ctx.ReadValue<float>() });
            _actionManager.Actions["inputbinder/throttle_axis"].Action.Enable();
            _actionManager.Actions["inputbinder/pitch_trim"].Action.Enable();
            _bindingUI = gameObject.AddComponent<BindingUI>();
            _bindingUI.enabled = false;
        }

        private void Update() => _vessel?.ApplyFlightCtrlState(new KSP.Sim.State.FlightCtrlStateIncremental() { pitchTrim = _vessel.flightCtrlState.pitchTrim + _actionManager.Actions["inputbinder/pitch_trim"].Action.ReadValue<float>() * Time.deltaTime });

        private void StopKSPFromRemovingGamepads()
        {
            GlobalLog.Log(LogFilter.UserMod, $"[{Constants.Name}] Stopping KSP from automatically removing Gamepads...");
            var eventInfo = typeof(InputSystem).GetEvent(nameof(InputSystem.onDeviceChange), BindingFlags.Static | BindingFlags.Public);
            var method = typeof(InputManager).GetMethod("RemoveGamepadCallback", BindingFlags.NonPublic | BindingFlags.Instance);
            var handler = Delegate.CreateDelegate(eventInfo.EventHandlerType, Game.InputManager, method);
            eventInfo.RemoveEventHandler(null, handler);
        }

        private void RemoveKSPsGamepadBindings()
        {
            GlobalLog.Log(LogFilter.UserMod, $"[{Constants.Name}] Removing KSP's Gamepad bindings...");
            var gamepads = Gamepad.all.ToArray();
            foreach (var action in Game.Input)
            {
                foreach (var bdg in action.bindings)
                {
                    if (bdg.effectivePath.Contains("<Gamepad>"))
                    {
                        var a = Game.Input.FindAction(action.name);
                        a.ApplyBindingOverride("", path: bdg.path);
                    }
                }
            }
            foreach (var gamepad in gamepads)
            {
                foreach (var control in gamepad.allControls)
                {
                    foreach (var action in Game.Input)
                    {
                        foreach (var ac in action.controls)
                        {
                            if (control == ac)
                            {
                                var b = action.GetBindingForControl(control);
                                if (!b.HasValue)
                                    continue;
                                action.ApplyBindingOverride("", b.Value.path);
                            }
                        }
                    }
                }
            }
        }

        private void OnEnable()
        {
            GameManager.Instance.Game.Messages.Subscribe<VesselChangingMessage>(VehicleStateChanged);
            GameManager.Instance.Game.Messages.Subscribe<VesselChangedMessage>(VehicleStateChanged);       
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
                if (_button is null)
                {
                    _button = new AppBarButton($"BTN-{Constants.ID}", Constants.Name, OnAppBarButtonClicked);
                }
            }
            else
            {
                _button.Dispose();
                _button = null;
                _bindingUI.Hide();
            }
        }

        private void OnAppBarButtonClicked(bool state)
        {
            // TODO: Sync app button state with window enabled state
            if (!state)
                _bindingUI.Hide();
            else
                _bindingUI.Show();
        }
    }
}
