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
        private AppBarButton _button = null;
        private BindingUI _bindingUI = null;

        public Inputbinder()
        {
            if (_instance is null)
                _instance = this;
        }

        private void Awake()
        {
            foreach (var mod in Game.KSP2ModManager.CurrentMods)
                if (mod.ModName == Constants.Name && mod.ModAuthor == Constants.Author)
                    _mod = mod;
            InputSystem.RegisterProcessor<Processors.MapProcessor>("Map");
            InputSystem.settings.defaultDeadzoneMin = 0f;
            StopKSPFromRemovingGamepads();
            RemoveKSPsGamepadBindings();
            // TODO: Trim reset action?
            _actionManager = InputActionManager.LoadFromJson(IOProvider.JoinPath(_mod.ModRootPath, "input.json"));
            _actionManager.Add(Game.Input.Flight.Pitch, true);
            _actionManager.Add(Game.Input.Flight.Roll, true);
            _actionManager.Add(Game.Input.Flight.Yaw, true);
            _actionManager.Add(Game.Input.Flight.ToggleLandingGear, true);
            _actionManager.Add(Game.Input.Flight.WheelSteer, true);
            _actionManager.Add(Game.Input.Flight.WheelBrakes, true);
            _actionManager.Add(Game.Input.Flight.WheelThrottle, true);
            _actionManager.Actions["inputbinder/throttle_axis"].Action.performed += ctx => _vessel?.ApplyFlightCtrlState(new KSP.Sim.State.FlightCtrlStateIncremental() { mainThrottle = ctx.ReadValue<float>() });
            _actionManager.Actions["inputbinder/throttle_axis"].Action.started += ctx => _vessel?.ApplyFlightCtrlState(new KSP.Sim.State.FlightCtrlStateIncremental() { mainThrottle = ctx.ReadValue<float>() });
            _actionManager.Actions["inputbinder/throttle_axis"].Action.canceled += ctx => _vessel?.ApplyFlightCtrlState(new KSP.Sim.State.FlightCtrlStateIncremental() { mainThrottle = ctx.ReadValue<float>() });
            //_actionManager.Actions["inputbinder/pitch_trim"].Action.performed += ctx => SetControls(pitchTrim: Mathf.Clamp(_vehicle.FlightControlInput.pitchTrim + ctx.ReadValue<float>(), -1, 1));
            //_actionManager.Actions["inputbinder/pitch_trim"].Action.started += ctx => SetControls(pitchTrim: Mathf.Clamp(_vehicle.FlightControlInput.pitchTrim + ctx.ReadValue<float>(), -1, 1));
            //_actionManager.Actions["inputbinder/pitch_trim"].Action.canceled += ctx => SetControls(pitchTrim: Mathf.Clamp(_vehicle.FlightControlInput.pitchTrim + ctx.ReadValue<float>(), -1, 1));
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
            _actionManager?.EnableAll();         
        }

        private void OnDisable()
        {
            GameManager.Instance.Game.Messages.Unsubscribe<VesselChangingMessage>(VehicleStateChanged);
            GameManager.Instance.Game.Messages.Unsubscribe<VesselChangedMessage>(VehicleStateChanged);
            _actionManager.DisableAll();
        }

        private void VehicleStateChanged(MessageCenterMessage msg)
        {
            _vessel = Game.ViewController.GetActiveSimVessel();
            if (_vessel is object)
            {
                if (_button is null)
                    _button = new AppBarButton($"BTN-{Constants.ID}", Constants.Name, OnAppBarButtonClicked);
            }
            else
            {
                _button.Dispose();
                _button = null;
                _actionManager.CancelBinding();
                _actionManager.CompleteChangeProcessors();
                _bindingUI.enabled = false;
            }
        }

        private void OnAppBarButtonClicked(bool state)
        {
            _bindingUI.enabled = state;
            if (!state)
            {
                _actionManager.CancelBinding();
                _actionManager.CompleteChangeProcessors();
            }
        }
    }
}
