using KSP.Game;
using System;
using UnityEngine.InputSystem;

namespace Codenade.Inputbinder
{
    public struct WrappedInputAction
    {
        public ActionSource Source;
        public InputAction InputAction;
        public string FriendlyName;
        public Action<WrappedInputAction> Setup;

        public WrappedInputAction(ActionSource source, InputAction inputAction, string friendlyName, Action<WrappedInputAction> setup)
        {
            if (inputAction is null)
                throw new ArgumentNullException("inputAction");
            Source = source;
            InputAction = inputAction;
            FriendlyName = friendlyName;
            Setup = setup;
        }
    }
    
    public enum ActionSource
    {
        Internal, Game
    }

    public static class DefaultInputActionDefinitions
    {
        public static WrappedInputAction[] WrappedInputActions
        {
            get
            {
                return new WrappedInputAction[]
                {
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Game,
                        InputAction = GameManager.Instance.Game.Input.Flight.Pitch,
                        FriendlyName = "Pitch",
                        Setup = DoAxis2C1NSetup
                    },
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Game,
                        InputAction = GameManager.Instance.Game.Input.Flight.Roll,
                        FriendlyName = "Roll",
                        Setup = DoAxis2C1NSetup
                    },
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Game,
                        InputAction = GameManager.Instance.Game.Input.Flight.Yaw,
                        FriendlyName = "Yaw",
                        Setup = DoAxis2C1NSetup
                    },
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Internal,
                        InputAction = new InputAction(Constants.ActionPitchTrimID),
                        FriendlyName = "Pitch Trim",
                        Setup = DoAxis2C1NSetup
                    },
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Internal,
                        InputAction = new InputAction(Constants.ActionRollTrimID),
                        FriendlyName = "Roll Trim",
                        Setup = DoAxis2C1NSetup
                    },
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Internal,
                        InputAction = new InputAction(Constants.ActionYawTrimID),
                        FriendlyName = "Yaw Trim",
                        Setup = DoAxis2C1NSetup
                    },
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Internal,
                        InputAction = new InputAction(Constants.ActionTrimResetID),
                        FriendlyName = "Reset Trim",
                        Setup = DoButtonSetup
                    },
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Game,
                        InputAction = GameManager.Instance.Game.Input.Flight.TogglePrecisionMode,
                        FriendlyName = "Toggle Precision Mode",
                        Setup = DoButtonSetup
                    },
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Internal,
                        InputAction = new InputAction(Constants.ActionThrottleID),
                        FriendlyName = "Throttle",
                        Setup = DoAxisSetup
                    },
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Game,
                        InputAction = GameManager.Instance.Game.Input.Flight.ThrottleDelta,
                        FriendlyName = "Throttle Delta",
                        Setup = DoAxis2C1NSetup
                    },
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Game,
                        InputAction = GameManager.Instance.Game.Input.Flight.ThrottleCutoff,
                        FriendlyName = "Throttle Cutoff",
                        Setup = DoButtonSetup
                    },
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Game,
                        InputAction = GameManager.Instance.Game.Input.Flight.ThrottleMax,
                        FriendlyName = "Throttle Max",
                        Setup = DoButtonSetup
                    },
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Game,
                        InputAction = GameManager.Instance.Game.Input.Flight.WheelSteer,
                        FriendlyName = "Wheel Steer",
                        Setup = DoAxis2C1NSetup
                    },
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Game,
                        InputAction = GameManager.Instance.Game.Input.Flight.WheelThrottle,
                        FriendlyName = "Wheel Throttle",
                        Setup = DoAxis2C1NSetup
                    },
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Game,
                        InputAction = GameManager.Instance.Game.Input.Flight.WheelBrakes,
                        FriendlyName = "Wheel Brakes",
                        Setup = DoAxisSetup
                    },
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Game,
                        InputAction = GameManager.Instance.Game.Input.Flight.Stage,
                        FriendlyName = "Stage",
                        Setup = DoButtonSetup
                    },
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Game,
                        InputAction = GameManager.Instance.Game.Input.Flight.ToggleLandingGear,
                        FriendlyName = "Toggle Landing Gear",
                        Setup = DoButtonSetup
                    },
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Game,
                        InputAction = GameManager.Instance.Game.Input.Flight.ToggleLights,
                        FriendlyName = "Toggle Lights",
                        Setup = DoButtonSetup
                    },
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Game,
                        InputAction = GameManager.Instance.Game.Input.Flight.ToggleSAS,
                        FriendlyName = "Toggle SAS",
                        Setup = DoButtonSetup
                    },
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Game,
                        InputAction = GameManager.Instance.Game.Input.Flight.ToggleRCS,
                        FriendlyName = "Toggle RCS",
                        Setup = DoButtonSetup
                    },
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Game,
                        InputAction = GameManager.Instance.Game.Input.Flight.TranslateX,
                        FriendlyName = "Translate X",
                        Setup = DoAxis2C1NSetup
                    },
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Game,
                        InputAction = GameManager.Instance.Game.Input.Flight.TranslateY,
                        FriendlyName = "Translate Y",
                        Setup = DoAxis2C1NSetup
                    },
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Game,
                        InputAction = GameManager.Instance.Game.Input.Flight.TranslateZ,
                        FriendlyName = "Translate Z",
                        Setup = DoAxis2C1NSetup
                    },
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Game,
                        InputAction = GameManager.Instance.Game.Input.Flight.TriggerActionGroup1,
                        FriendlyName = "Trigger Action Group 1",
                        Setup = DoButtonSetup
                    },
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Game,
                        InputAction = GameManager.Instance.Game.Input.Flight.TriggerActionGroup2,
                        FriendlyName = "Trigger Action Group 2",
                        Setup = DoButtonSetup
                    },
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Game,
                        InputAction = GameManager.Instance.Game.Input.Flight.TriggerActionGroup3,
                        FriendlyName = "Trigger Action Group 3",
                        Setup = DoButtonSetup
                    },
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Game,
                        InputAction = GameManager.Instance.Game.Input.Flight.TriggerActionGroup4,
                        FriendlyName = "Trigger Action Group 4",
                        Setup = DoButtonSetup
                    },
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Game,
                        InputAction = GameManager.Instance.Game.Input.Flight.TriggerActionGroup5,
                        FriendlyName = "Trigger Action Group 5",
                        Setup = DoButtonSetup
                    },
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Game,
                        InputAction = GameManager.Instance.Game.Input.Flight.TriggerActionGroup6,
                        FriendlyName = "Trigger Action Group 6",
                        Setup = DoButtonSetup
                    },
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Game,
                        InputAction = GameManager.Instance.Game.Input.Flight.TriggerActionGroup7,
                        FriendlyName = "Trigger Action Group 7",
                        Setup = DoButtonSetup
                    },
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Game,
                        InputAction = GameManager.Instance.Game.Input.Flight.TriggerActionGroup8,
                        FriendlyName = "Trigger Action Group 8",
                        Setup = DoButtonSetup
                    },
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Game,
                        InputAction = GameManager.Instance.Game.Input.Flight.TriggerActionGroup9,
                        FriendlyName = "Trigger Action Group 9",
                        Setup = DoButtonSetup
                    },
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Game,
                        InputAction = GameManager.Instance.Game.Input.Flight.TriggerActionGroup10,
                        FriendlyName = "Trigger Action Group 10",
                        Setup = DoButtonSetup
                    },
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Game,
                        InputAction = GameManager.Instance.Game.Input.Flight.CameraPitchGamepad,
                        FriendlyName = "Camera Pitch",
                        Setup = DoAxis2C1NSetup
                    },
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Game,
                        InputAction = GameManager.Instance.Game.Input.Flight.CameraYawGamepad,
                        FriendlyName = "Camera Yaw",
                        Setup = DoAxis2C1NSetup
                    },
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Game,
                        InputAction = GameManager.Instance.Game.Input.Flight.CameraZoom,
                        FriendlyName = "Camera Zoom",
                        Setup = DoCameraZoomSetup
                    },
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Game,
                        InputAction = GameManager.Instance.Game.Input.Flight.ShowMap,
                        FriendlyName = "Show Map",
                        Setup = DoButtonSetup
                    },
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Game,
                        InputAction = GameManager.Instance.Game.Input.MapView.HideMap,
                        FriendlyName = "Hide Map",
                        Setup = DoButtonSetup
                    },
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Game,
                        InputAction = GameManager.Instance.Game.Input.Global.QuickSave,
                        FriendlyName = "Quick Save",
                        Setup = DoButtonSetup
                    },
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Game,
                        InputAction = GameManager.Instance.Game.Input.Global.QuickLoad,
                        FriendlyName = "Quick Load",
                        Setup = DoButtonSetup
                    },
					new WrappedInputAction()
					{
						Source = ActionSource.Game,
						InputAction = GameManager.Instance.Game.Input.Global.QuickLoadHold,
						FriendlyName = "Quick Load Hold",
						Setup = DoButtonSetup
					},
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Game,
                        InputAction = GameManager.Instance.Game.Input.Global.TimeWarpDecrease,
                        FriendlyName = "Time Warp Decrease",
                        Setup = DoButtonSetup
                    },
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Game,
                        InputAction = GameManager.Instance.Game.Input.Global.TimeWarpIncrease,
                        FriendlyName = "Time Warp Increase",
                        Setup = DoButtonSetup
                    },
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Game,
                        InputAction = GameManager.Instance.Game.Input.Global.TimeWarpStop,
                        FriendlyName = "Time Warp Stop",
                        Setup = DoButtonSetup
                    },
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Game,
                        InputAction = GameManager.Instance.Game.Input.Global.TogglePauseMenu,
                        FriendlyName = "Toggle Pause Menu",
                        Setup = DoButtonSetup
                    },
                    new WrappedInputAction()
                    {
                        Source = ActionSource.Game,
                        InputAction = GameManager.Instance.Game.Input.Global.ToggleUIVisibility,
                        FriendlyName = "Toggle UI Visibility",
                        Setup = DoButtonSetup
                    }
                };
            }
        }

        public static void DoButtonSetup(WrappedInputAction d)
        {
            if (d.InputAction is null)
            {
                QLog.ErrorLine("d.InputAction cannot be null");
            }
            bool wasEnabled = d.InputAction.enabled;
            d.InputAction.expectedControlType = "Button";
            if (d.Source == ActionSource.Internal)
            {
                for (var jb = d.InputAction.bindings.Count - 1; jb >= 0; jb--)
                    d.InputAction.ChangeBinding(jb).Erase();
                d.InputAction.AddBinding(path: null).WithName("Device 1");
                d.InputAction.AddBinding(path: null).WithName("Device 2");
            }
            else if (d.Source == ActionSource.Game)
            {
                if (d.InputAction.bindings.Count < 1 || d.InputAction.bindings[0].isComposite || d.InputAction.bindings[0].isPartOfComposite)
                {
                    QLog.WarnLine($"Cannot load default path for {d.InputAction.name} - {d.InputAction.id}");
                    d.Source = ActionSource.Internal;
                    DoButtonSetup(d);
                }
                else
                {
                    string defaultPath = d.InputAction.bindings[0].path;
                    for (var jb = d.InputAction.bindings.Count - 1; jb >= 0; jb--)
                        d.InputAction.ChangeBinding(jb).Erase();
                    d.InputAction.AddBinding(path: defaultPath).WithName("Device 1");
                    d.InputAction.AddBinding(path: null).WithName("Device 2");
                }
            }
            if (wasEnabled)
                d.InputAction.Enable();
        }

        public static void DoAxisSetup(WrappedInputAction d)
        {
            if (d.InputAction is null)
            {
                QLog.ErrorLine("d.InputAction cannot be null");
            }
            bool wasEnabled = d.InputAction.enabled;
            d.InputAction.expectedControlType = "Axis";
            if (d.Source == ActionSource.Internal)
            {
                for (var jb = d.InputAction.bindings.Count - 1; jb >= 0; jb--)
                    d.InputAction.ChangeBinding(jb).Erase();
                d.InputAction.AddBinding(path: null).WithName("Device 1");
                d.InputAction.AddBinding(path: null).WithName("Device 2");
            }
            else if (d.Source == ActionSource.Game)
            {
                if (d.InputAction.bindings.Count < 1 || d.InputAction.bindings[0].isComposite || d.InputAction.bindings[0].isPartOfComposite)
                {
                    QLog.WarnLine($"Cannot load default path for {d.InputAction.name} - {d.InputAction.id}");
                    QLog.PrintEnumerable(d.InputAction.bindings, QLog.Category.Debug);
                    d.Source = ActionSource.Internal;
                    DoAxisSetup(d);
                }
                else
                {
                    string defaultPath = d.InputAction.bindings[0].path;
                    for (var jb = d.InputAction.bindings.Count - 1; jb >= 0; jb--)
                        d.InputAction.ChangeBinding(jb).Erase();
                    d.InputAction.AddBinding(path: defaultPath).WithName("Device 1");
                    d.InputAction.AddBinding(path: null).WithName("Device 2");
                }
            }
            if (wasEnabled)
                d.InputAction.Enable();
        }

        public static void DoAxis1C1NSetup(WrappedInputAction d)
        {
            if (d.InputAction is null)
            {
                QLog.ErrorLine("d.InputAction cannot be null");
            }
            bool wasEnabled = d.InputAction.enabled;
            d.InputAction.expectedControlType = "Axis";
            if (d.Source == ActionSource.Internal)
            {
                for (var jb = d.InputAction.bindings.Count - 1; jb >= 0; jb--)
                    d.InputAction.ChangeBinding(jb).Erase();
                d.InputAction.AddCompositeBinding("1DAxis")
                        .With("negative", null)
                        .With("positive", null);
                d.InputAction.AddBinding(path: null).WithName("Axis");
            }
            else if (d.Source == ActionSource.Game)
            {
                sbyte statusCode = -1;
                string defaultPathA = null;
                string defaultPathB = null;
                if (d.InputAction.bindings.Count > 0)
                {
                    if (d.InputAction.bindings[0].isComposite)
                    {
                        if (d.InputAction.bindings.Count > 2 && d.InputAction.bindings[1].isPartOfComposite && d.InputAction.bindings[2].isPartOfComposite)
                        {
                            statusCode = 0;
                            if (d.InputAction.bindings[1].name == "negative")
                            {
                                defaultPathA = d.InputAction.bindings[1].path;
                                defaultPathB = d.InputAction.bindings[2].path;
                            }
                            else if (d.InputAction.bindings[1].name == "positive")
                            {
                                defaultPathA = d.InputAction.bindings[2].path;
                                defaultPathB = d.InputAction.bindings[1].path;
                            }
                        }
                    }
                    else if (!d.InputAction.bindings[0].isPartOfComposite)
                    {
                        statusCode = 1;
                        defaultPathA = d.InputAction.bindings[0].path;
                    }
                }
                if (statusCode == -1)
                {
                    QLog.WarnLine($"Cannot load default path for {d.InputAction.name} - {d.InputAction.id}");
                    QLog.PrintEnumerable(d.InputAction.bindings, QLog.Category.Debug);
                    d.Source = ActionSource.Internal;
                    DoAxis1C1NSetup(d);
                }
                else
                {
                    for (var jb = d.InputAction.bindings.Count - 1; jb >= 0; jb--)
                        d.InputAction.ChangeBinding(jb).Erase();
                    d.InputAction.AddCompositeBinding("1DAxis")
                        .With("negative", statusCode == 0 ? defaultPathA : null)
                        .With("positive", statusCode == 0 ? defaultPathB : null);
                    d.InputAction.AddBinding(path: statusCode == 1 ? defaultPathA : null).WithName("Axis");
                }
            }
            if (wasEnabled)
                d.InputAction.Enable();
        }

        public static void DoAxis2C1NSetup(WrappedInputAction d)
        {
            if (d.InputAction is null)
            {
                QLog.ErrorLine("d.InputAction cannot be null");
            }
            bool wasEnabled = d.InputAction.enabled;
            d.InputAction.expectedControlType = "Axis";
            if (d.Source == ActionSource.Internal)
            {
                for (var jb = d.InputAction.bindings.Count - 1; jb >= 0; jb--)
                    d.InputAction.ChangeBinding(jb).Erase();
                d.InputAction.AddCompositeBinding("1DAxis")
                        .With("negative", null)
                        .With("positive", null);
                d.InputAction.AddCompositeBinding("1DAxis")
                    .With("negative", null)
                    .With("positive", null);
                d.InputAction.AddBinding(path: null).WithName("Axis");
            }
            else if (d.Source == ActionSource.Game)
            {
                sbyte statusCode = -1;
                string defaultPathA = null;
                string defaultPathB = null;
                if (d.InputAction.bindings.Count > 0)
                {
                    if (d.InputAction.bindings[0].isComposite)
                    {
                        if (d.InputAction.bindings.Count > 2 && d.InputAction.bindings[1].isPartOfComposite && d.InputAction.bindings[2].isPartOfComposite)
                        {
                            statusCode = 0;
                            if (d.InputAction.bindings[1].name == "negative")
                            {
                                defaultPathA = d.InputAction.bindings[1].path;
                                defaultPathB = d.InputAction.bindings[2].path;
                            }
                            else if (d.InputAction.bindings[1].name == "positive")
                            {
                                defaultPathA = d.InputAction.bindings[2].path;
                                defaultPathB = d.InputAction.bindings[1].path;
                            }
                        }
                    }
                    else if (!d.InputAction.bindings[0].isPartOfComposite)
                    {
                        statusCode = 1;
                        defaultPathA = d.InputAction.bindings[0].path;
                    }
                }

                if (statusCode == -1)
                {
                    QLog.WarnLine($"Cannot load default path for {d.InputAction.name} - {d.InputAction.id}");
                    QLog.PrintEnumerable(d.InputAction.bindings, QLog.Category.Debug);
                    d.Source = ActionSource.Internal;
                    DoAxis2C1NSetup(d);
                }
                else
                {
                    for (var jb = d.InputAction.bindings.Count - 1; jb >= 0; jb--)
                        d.InputAction.ChangeBinding(jb).Erase();
                    d.InputAction.AddCompositeBinding("1DAxis")
                        .With("negative", statusCode == 0 ? defaultPathA : null)
                        .With("positive", statusCode == 0 ? defaultPathB : null);
                    d.InputAction.AddCompositeBinding("1DAxis")
                        .With("negative", null)
                        .With("positive", null);
                    d.InputAction.AddBinding(path: statusCode == 1 ? defaultPathA : null).WithName("Axis");
                }
            }
            if (wasEnabled)
                d.InputAction.Enable();
        }

        public static void DoVector2dSetup(WrappedInputAction d)
        {
            if (d.InputAction is null)
            {
                QLog.ErrorLine("d.InputAction cannot be null");
            }
            bool wasEnabled = d.InputAction.enabled;
            d.InputAction.expectedControlType = "Vector2";
            for (var jb = d.InputAction.bindings.Count - 1; jb >= 0; jb--)
                d.InputAction.ChangeBinding(jb).Erase();
            d.InputAction.AddCompositeBinding("2DVector")
                            .With("left", null)
                            .With("right", null)
                            .With("down", null)
                            .With("up", null);
            d.InputAction.AddCompositeBinding("2DAxis")
                .With("x", null)
                .With("y", null);
            if (wasEnabled)
                d.InputAction.Enable();
        }

        public static void DoCameraZoomSetup(WrappedInputAction d)
        {
            if (d.InputAction is null)
            {
                QLog.ErrorLine("d.InputAction cannot be null");
            }
            bool wasEnabled = d.InputAction.enabled;
            d.InputAction.expectedControlType = "Vector2";
            for (var jb = d.InputAction.bindings.Count - 1; jb >= 0; jb--)
                d.InputAction.ChangeBinding(jb).Erase();
            d.InputAction.AddCompositeBinding("2DVector")
                            .With("left", null)
                            .With("right", null)
                            .With("down", null)
                            .With("up", null);
            d.InputAction.AddCompositeBinding("2DAxis")
                .With("x", null)
                .With("y", "/Mouse/scroll/y");
            //var ovrd = d.InputAction.bindings[5];
            //ovrd.overrideProcessors = "ScaleVector2(x=0,y=0.0005)";
            //d.InputAction.ApplyBindingOverride(5, ovrd);
            d.InputAction.ChangeBinding(5).WithProcessors("ScaleVector2(x=0,y=0.0005)");
            if (wasEnabled)
                d.InputAction.Enable();
        }
    }
}
