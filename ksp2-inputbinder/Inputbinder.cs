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
        private bool _isInFlightView = false;
        private IVehicle _vehicle;
        private InputActionManager _actionManager;
        private AppBarButton _button = null;
        private BindingUI _bindingUI = null;

        public Inputbinder()
        {
            if (_instance is null)
                _instance = this;

            _bindingUI = new BindingUI();
            _bindingUI.enabled = false;
        }

        private void Start()
        {
            foreach (var mod in Game.KSP2ModManager.CurrentMods)
                if (mod.ModName == Constants.Name && mod.ModAuthor == Constants.Author)
                    _mod = mod;
            InputSystem.RegisterProcessor<Processors.MapProcessor>("Map");
            InputSystem.settings.defaultDeadzoneMin = 0f;
            StopKSPFromRemovingGamepads();
            RemoveKSPsGamepadBindings();
            
            _actionManager = InputActionManager.LoadFromJson(IOProvider.JoinPath(_mod.ModRootPath, "input.json"));
            _actionManager.Add(Game.Input.Flight.Pitch, true);
            _actionManager.Add(Game.Input.Flight.Roll, true);
            _actionManager.Add(Game.Input.Flight.Yaw, true);
            _actionManager.Actions["inputbinder/throttle_axis"].Action.performed += ctx =>      SetControls(throttle: ctx.ReadValue<float>());
            _actionManager.Actions["inputbinder/throttle_axis"].Action.started += ctx =>        SetControls(throttle: ctx.ReadValue<float>());
            _actionManager.Actions["inputbinder/throttle_axis"].Action.canceled += ctx =>       SetControls(throttle: ctx.ReadValue<float>());
            _actionManager.Actions["inputbinder/pitch_trim"].Action.performed += ctx =>         SetControls(pitchTrim: ctx.ReadValue<float>());
            _actionManager.Actions["inputbinder/pitch_trim"].Action.started += ctx =>           SetControls(pitchTrim: ctx.ReadValue<float>());
            _actionManager.Actions["inputbinder/pitch_trim"].Action.canceled += ctx =>          SetControls(pitchTrim: ctx.ReadValue<float>());
        }

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
            GameManager.Instance.Game.Messages.Subscribe<FlightViewLeftMessage>(OnFlightViewChanged);
            GameManager.Instance.Game.Messages.Subscribe<FlightViewEnteredMessage>(OnFlightViewChanged);
            GameManager.Instance.Game.Messages.Subscribe<MessageCenterMessage>(DebugVoid);
            _actionManager?.EnableAll();         
        }

        private void OnDisable()
        {
            _actionManager.DisableAll();
        }

        private void DebugVoid(MessageCenterMessage msg)
        {
            GlobalLog.Log(LogFilter.UserMod, $"[{Constants.Name}] <b><color=cyan>msg: {msg.GetType().Name}</color></b>");
        }

        private void OnFlightViewChanged(MessageCenterMessage msg)
        {
            // TODO: Find a better way to do this
            if (msg.GetType() == typeof(FlightViewEnteredMessage))
            {
                _isInFlightView = true;
                _vehicle = Game.ViewController.GetActiveVehicle();
                _button = new AppBarButton($"BTN-{Constants.ID}", Constants.Name, OnAppBarButtonClicked);
            }
            else
            {
                _isInFlightView = false;
                _vehicle = null;
                _button.Dispose();
                _button = null;
                _actionManager.CancelBinding();
                _actionManager.CompleteChangeProcessors();
                _bindingUI.enabled = true;
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

        public void SetControls(float? throttle = null, 
            float? roll = null, float? yaw = null, float? pitch = null, float? rollTrim = null, float? yawTrim = null, float? pitchTrim = null,
            float? wheelSteer = null, float? wheelSteerTrim = null, float? wheelThrottle = null, float? wheelThrottleTrim = null,
            bool? killRot = null, bool? gearUp = null, bool? gearDown = null, bool? headlight = null)
        {
            if (_isInFlightView && _vehicle is object)
            {
                _vehicle.FlightControllableInput.AtomicSet(throttle, 
                    roll, yaw, pitch, rollTrim, yawTrim, pitchTrim,
                    wheelSteer, wheelSteerTrim, wheelThrottle, wheelThrottleTrim,
                    killRot, gearUp, gearDown, headlight);
            }
        }
    }

    public enum InputDestination
    {
        Throttle, Roll, Yaw, Pitch, RollTrim, YawTrim, PitchTrim,
        WheelSteer, WheelSteerTrim, WheelThrottle, WheelThrottleTrim,
        KillRot, GearUp, GearDown, Headlight
    }
}
