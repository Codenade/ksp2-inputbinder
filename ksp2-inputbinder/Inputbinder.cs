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
using UnityEngine.InputSystem.DualShock;
using KSP.Input;
using System.Reflection;

namespace Codenade.Inputbinder
{
    public class Inputbinder : KerbalMonoBehaviour
    {
        private KSP2Mod _mod;
        private bool _isInFlightView = false;
        private IVehicle _vehicle;
        private InputActionManager _actionManager;
        private AppBarButton _button = null;
        private bool _showWindow = false;
        private Rect _windowRect = new Rect(400, 300, 100, 400);
        private Rect _windowProcRect = new Rect(0, 0, 300, 100);
        private float _maxWidth = 0f;
        private Vector2 _scrollPos = Vector2.zero;
        private bool _inputsLoaded = false;
        private int _procFlowPos = 0;
        private string _procTemp = "";
        private Dictionary<string, object> _procValStore = null;
        private Dictionary<InputDestination, object> _events = null;

        public Inputbinder()
        {
            _procValStore = new Dictionary<string, object>();
            _events = new Dictionary<InputDestination, object>();
        }

        private void Start()
        {
            GlobalLog.Log(LogFilter.UserMod, $"[{Constants.Name}] Loaded and active.");
            foreach (var mod in Game.KSP2ModManager.CurrentMods)
            {
                if (mod.ModName == Constants.Name && mod.ModAuthor == Constants.Author)
                    _mod = mod;
            }
            InputSystem.RegisterProcessor<Processors.MapProcessor>("Map");
            InputSystem.settings.defaultDeadzoneMin = 0f;
            //InputSystem.onDeviceChange += OnDeviceChange;
            StopKSPFromRemovingGamepads();
            RemoveKSPsGamepadBindings();
            
            if (!_inputsLoaded)
            {
                _actionManager = InputActionManager.LoadFromJson(IOProvider.JoinPath(_mod.ModRootPath, "input.json"));
                _inputsLoaded = true;
            }

            _actionManager.Actions["throttle_axis"].Action.performed += ctx =>      _events[InputDestination.Throttle] = ctx.ReadValue<float>();
            _actionManager.Actions["throttle_axis"].Action.started += ctx =>        _events[InputDestination.Throttle] = ctx.ReadValue<float>();
            _actionManager.Actions["throttle_axis"].Action.canceled += ctx =>       _events[InputDestination.Throttle] = ctx.ReadValue<float>();
            _actionManager.Actions["pitch_trim_up"].Action.performed += ctx =>      _events[InputDestination.PitchTrim] = ctx.ReadValue<float>();
            _actionManager.Actions["pitch_trim_up"].Action.started += ctx =>        _events[InputDestination.PitchTrim] = ctx.ReadValue<float>();
            _actionManager.Actions["pitch_trim_up"].Action.canceled += ctx =>       _events[InputDestination.PitchTrim] = ctx.ReadValue<float>();
            _actionManager.Actions["pitch_trim_down"].Action.performed += ctx =>    _events[InputDestination.PitchTrim] = -ctx.ReadValue<float>();
            _actionManager.Actions["pitch_trim_down"].Action.started += ctx =>      _events[InputDestination.PitchTrim] = -ctx.ReadValue<float>();
            _actionManager.Actions["pitch_trim_down"].Action.canceled += ctx =>     _events[InputDestination.PitchTrim] = -ctx.ReadValue<float>();
            _actionManager.Actions["pitch_axis"].Action.performed += ctx =>         _events[InputDestination.Pitch] = -ctx.ReadValue<float>();
            _actionManager.Actions["pitch_axis"].Action.started += ctx =>           _events[InputDestination.Pitch] = -ctx.ReadValue<float>();
            _actionManager.Actions["pitch_axis"].Action.canceled += ctx =>          _events[InputDestination.Pitch] = -ctx.ReadValue<float>();
            _actionManager.Actions["roll_axis"].Action.performed += ctx =>          _events[InputDestination.Roll] = -ctx.ReadValue<float>();
            _actionManager.Actions["roll_axis"].Action.started += ctx =>            _events[InputDestination.Roll] = -ctx.ReadValue<float>();
            _actionManager.Actions["roll_axis"].Action.canceled += ctx =>           _events[InputDestination.Roll] = -ctx.ReadValue<float>();
            _actionManager.Actions["yaw_axis"].Action.performed += ctx =>           _events[InputDestination.Yaw] = -ctx.ReadValue<float>();
            _actionManager.Actions["yaw_axis"].Action.started += ctx =>             _events[InputDestination.Yaw] = -ctx.ReadValue<float>();
            _actionManager.Actions["yaw_axis"].Action.canceled += ctx =>            _events[InputDestination.Yaw] = -ctx.ReadValue<float>();

            foreach (var item in InputSystem.devices)
            {
                Debug.Log(item);
            }
        }

        private void OnDeviceChange(InputDevice device, InputDeviceChange deviceChange)
        {
            RemoveKSPsGamepadBindings();
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
                                action.ChangeBinding(action.GetBindingIndexForControl(control)).WithPath("");
                                action.ApplyBindingOverride(action.GetBindingIndexForControl(control), new InputBinding(""));
                            }
                        }
                    }
                }
            }
        }

        private void Update()
        {
            if (!_events.TryGetValue(InputDestination.Throttle, out var throttle)) throttle = null;
            if (!_events.TryGetValue(InputDestination.PitchTrim, out var pitch_trim)) pitch_trim = null;
            if (!_events.TryGetValue(InputDestination.Pitch, out var pitch)) pitch = null;
            if (!_events.TryGetValue(InputDestination.Yaw, out var yaw)) yaw = null;
            if (!_events.TryGetValue(InputDestination.Roll, out var roll)) roll = null;
            SetControls(
                throttle: (float?) throttle,
                pitchTrim: (float?) pitch_trim,
                pitch: (float?) pitch,
                yaw: (float?) yaw,
                roll: (float?) roll);
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
            GlobalLog.Log(LogFilter.UserMod, $"[{Constants.Name}] msg: {msg.GetType().Name}");
        }

        private void OnFlightViewChanged(MessageCenterMessage msg)
        {
            if (msg.GetType() == typeof(FlightViewEnteredMessage))
            {
                _isInFlightView = true;
                _vehicle = Game.ViewController.GetActiveVehicle();
                _button = new AppBarButton($"BTN-{Constants.ID}", Constants.Name, OnAppBarButtonClicked);
                Game.Input.Flight.Pitch.Disable();
                Game.Input.Flight.Yaw.Disable();
                Game.Input.Flight.Roll.Disable();
                Game.Input.Flight.ThrottleDelta.Disable();
            }
            else
            {
                _isInFlightView = false;
                _vehicle = null;
                _button.Dispose();
                _button = null;
            }
        }

        private void OnAppBarButtonClicked(bool state)
        {
            _showWindow = state;
            if (!state)
            {
                _actionManager.CancelBinding();
            }
        }

        private void OnGUI()
        {
            if (!_showWindow)
                return;
            var lblStyle = new GUIStyle(GUI.skin.label)
            {
                stretchHeight = true,
                stretchWidth = true,
                clipping = TextClipping.Clip,
                alignment = TextAnchor.MiddleLeft
            };
            var scrollStyle = new GUIStyle(GUI.skin.scrollView)
            {
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 0, 0)
            };
            _windowRect = GUILayout.Window(925380980, _windowRect, idMain =>
            {
                if (_actionManager.Actions is object)
                {
                    _scrollPos = GUILayout.BeginScrollView(_scrollPos, false, true, GUILayout.MinWidth(_maxWidth + 16));
                    foreach (var item in _actionManager.Actions)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Label(item.Value.FriendlyName, lblStyle, GUILayout.Width(300));
                        GUILayout.FlexibleSpace();
                        string pathStr = item.Value.Action.bindings[0].effectivePath;
                        if (_actionManager.IsCurrentlyRebinding)
                            pathStr = (_actionManager.RebindingAction == item.Value.Action) ? "Press ESC to cancel" : pathStr;
                        if (GUILayout.Button(new GUIContent(pathStr, "Click to change binding"), GUILayout.Width(150)))
                            _actionManager.Rebind(item.Value.Action);
                        if (GUILayout.Button(new GUIContent("Proc", "Change input modifiers"), GUILayout.Width(50)))
                            _actionManager.ChangeProcessors(item.Value.Action);
                        GUILayout.EndHorizontal();
                        var lastItemWidth = GUILayoutUtility.GetLastRect().width;
                        _maxWidth = lastItemWidth > _maxWidth ? lastItemWidth : _maxWidth;
                    }
                    GUILayout.FlexibleSpace();
                    GUILayout.EndScrollView();
                    if (GUILayout.Button("Save"))
                        _actionManager.SaveToJson(IOProvider.JoinPath(_mod.ModRootPath, "input.json"));
                }
                GUI.DragWindow();
            }, "Main Window");
            if (_actionManager.IsChangingProc)
            {
                var pwtxt = "";
                switch (_procFlowPos)
                {
                    case 0:
                        pwtxt = $"Change input modifiers of {_actionManager.ChgProcAction.name}";
                        break;
                    case 1:
                        pwtxt = $"Select a modifier to add to {_actionManager.ChgProcAction.name}";
                        break;
                    case 2:
                        pwtxt = $"Properties of processor {_procTemp} from {_actionManager.ChgProcAction.name}";
                        break;
                    default:
                        pwtxt = "Change input modifiers";
                        break;
                }
                _windowProcRect = GUILayout.Window(213124455,
                    new Rect(_windowRect.x + _windowRect.width, _windowRect.y, _windowProcRect.width, _windowProcRect.height),
                    idProc =>
                    {
                        GUILayout.BeginVertical();
                        if (_procFlowPos == 0)
                        {
                            if (_actionManager.ChgProcAction.bindings[0].overrideProcessors is object)
                            {
                                foreach (var proc in _actionManager.ChgProcAction.bindings[0].effectiveProcessors.Split(';'))
                                {
                                    if (proc == "")
                                        continue;
                                    GUILayout.BeginHorizontal();
                                    if (GUILayout.Button(proc))
                                    {
                                        _procFlowPos = 6;
                                        _procTemp = proc.Substring(0, proc.IndexOf('('));
                                        _procValStore.Clear();
                                        var st = proc.IndexOf('(') + 1;
                                        foreach (var prop in proc.Substring(st, proc.IndexOf(')') - st).Split(','))
                                        {
                                            string[] spl = prop.Split('=');
                                            string name = spl[0];
                                            string val = spl[1];
                                            if (float.TryParse(val, out float val_float))
                                            {
                                                _procValStore.Add(name, val_float);
                                                continue;
                                            }
                                            if (bool.TryParse(val, out bool val_bool))
                                                _procValStore.Add(name, val_bool);
                                        }
                                    }
                                    if (GUILayout.Button("-", GUILayout.Width(20)))
                                    {
                                        var binding = _actionManager.ChgProcAction.bindings[0];
                                        binding.overrideProcessors = binding.overrideProcessors.Replace(proc, "").Trim(';');
                                        _actionManager.ChgProcAction.ApplyBindingOverride(0, binding);
                                    }
                                    GUILayout.EndHorizontal();
                                }
                            }
                            if (GUILayout.Button("Add Processor")) _procFlowPos = 1;
                        }
                        else if (_procFlowPos == 1)
                        {
                            foreach (var avail in InputSystem.ListProcessors())
                            {
                                if (GUILayout.Button(avail))
                                {
                                    _procTemp = avail;
                                    _procValStore.Clear();
                                    _procFlowPos = 2;
                                    break;
                                }
                            }
                        }
                        else if (_procFlowPos == 2 || _procFlowPos == 6)
                        {
                            var type = InputSystem.TryGetProcessor(_procTemp);
                            if (type is object)
                            {
                                foreach (var field in type.GetFields())
                                {
                                    if (field.IsPublic)
                                    {
                                        if (field.FieldType.IsNumericType())
                                        {
                                            float val = 0;
                                            if (_procValStore.ContainsKey(field.Name))
                                                val = (float)_procValStore[field.Name];
                                            else
                                                _procValStore.Add(field.Name, val);
                                            GUILayout.Label(field.Name);
                                            if (float.TryParse(GUILayout.TextField(val.ToString()), out var tval))
                                                val = tval;
                                            val = GUILayout.HorizontalSlider(val, -3, 3);
                                            GUILayout.Space(4);
                                            _procValStore[field.Name] = val;
                                        }
                                        else if (Type.GetTypeCode(field.FieldType) == TypeCode.Boolean)
                                        {
                                            bool val = false;
                                            if (_procValStore.ContainsKey(field.Name))
                                            {
                                                val = (bool)_procValStore[field.Name];
                                            }
                                            else
                                            {
                                                _procValStore.Add(field.Name, val);
                                            }
                                            val = GUILayout.Toggle(val, field.Name);
                                            GUILayout.Space(4);
                                            _procValStore[field.Name] = val;
                                        }
                                    }
                                }
                            }
                        }
                        GUILayout.FlexibleSpace();
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button("< Back"))
                        {
                            if (_procFlowPos < 1) _actionManager.CompleteChangeProcessors();
                            else if (_procFlowPos == 6) _procFlowPos = 0;
                            else _procFlowPos--;
                        }
                        if (_procFlowPos == 2)
                        {
                            if (GUILayout.Button("Add"))
                            {
                                var binding = _actionManager.ChgProcAction.bindings[0];
                                if (binding.overrideProcessors is object)
                                {
                                    if (binding.overrideProcessors.Length > 0)
                                        _procTemp = InputBinding.Separator + _procTemp;
                                }
                                _procTemp += '(';
                                int item_no = 1;
                                foreach (var ent in _procValStore)
                                {
                                    _procTemp += $"{ent.Key}={ent.Value}{(item_no == _procValStore.Count ? "" : ",")}";
                                    item_no++;
                                }
                                _procTemp += ')';
                                if (binding.overrideProcessors is object) 
                                    binding.overrideProcessors += _procTemp;
                                else 
                                    binding.overrideProcessors = _procTemp;
                                _actionManager.ChgProcAction.ApplyBindingOverride(0, binding);
                                _procTemp = "";
                                _procValStore.Clear();
                                _procFlowPos = 0;
                            }
                        }
                        if (_procFlowPos == 6)
                        {
                            if (GUILayout.Button("Apply"))
                            {
                                var binding = _actionManager.ChgProcAction.bindings[0];
                                string original = $"{_procTemp}()";
                                string func_name = _procTemp;
                                if (binding.overrideProcessors is object)
                                {
                                    original = binding.overrideProcessors;
                                }
                                _procTemp += '(';
                                int item_no = 1;
                                foreach (var ent in _procValStore)
                                {
                                    _procTemp += $"{ent.Key}={ent.Value}{(item_no == _procValStore.Count ? "" : ",")}";
                                    item_no++;
                                }
                                _procTemp += ')';
                                var start_idx = original.IndexOf(func_name);
                                var end_idx = original.IndexOf(')', start_idx);
                                binding.overrideProcessors = original.Replace(original.Substring(start_idx, end_idx - start_idx + 1), _procTemp);
                                _actionManager.ChgProcAction.ApplyBindingOverride(0, binding);
                                _procTemp = "";
                                _procValStore.Clear();
                                _procFlowPos = 0;
                            }
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.EndVertical();
                    }, new GUIContent(pwtxt));
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
