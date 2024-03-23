using KSP.Game;
using System;
using UnityEngine.InputSystem;

namespace Codenade.Inputbinder
{
    public interface IWrappedInputAction
    {
        public string FriendlyName { get; set; }
    }

    public struct WrappedInputAction : IWrappedInputAction
    {
        public ActionSource Source { get; set; }
        public InputAction InputAction { get; set; }
        public string FriendlyName { get; set; }
        public Action<WrappedInputAction> Setup { get; set; }

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

    public struct Category : IWrappedInputAction
    {
        public string FriendlyName { get; set; }

        public Category(string name) => FriendlyName = name;
    }

    public struct CategoryEnd : IWrappedInputAction
    {
        public string FriendlyName { get; set; }
    }

    public enum ActionSource
    {
        Internal, Game
    }

    public static class DefaultInputActionDefinitions
    {
        public static Category CategoryBasicFlight = new Category("Basic Controls");
        public static Category CategoryActionGroups = new Category("Action Groups");
        public static Category CategoryAPMode = new Category("SAS Modes");
        public static Category CategoryFlightCamera = new Category("Flight Camera Controls");
        public static Category CategoryEVA = new Category("EVA");
        public static Category CategoryOAB = new Category("OAB");
        public static Category CategoryRD = new Category("R&D");
        public static Category CategoryMap = new Category("Map");
        public static Category CategoryGeneral = new Category("General");
        public static Category CategoryCustom = new Category("Custom");

        public static IWrappedInputAction[] WrappedInputActions => _wrappedInputActions;

        private static readonly IWrappedInputAction[] _wrappedInputActions =
            new IWrappedInputAction[]
            {
                CategoryBasicFlight,
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
                    InputAction = GameManager.Instance.Game.Input.Flight.ToggleRotLinControls,
                    FriendlyName = "Toggle Docking Controls",
                    Setup = DoButtonSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Game,
                    InputAction = GameManager.Instance.Game.Input.Flight.TogglePrecisionMode,
                    FriendlyName = "Toggle Precision Controls",
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
                new CategoryEnd(),
                CategoryAPMode,
                new WrappedInputAction()
                {
                    Source = ActionSource.Internal,
                    InputAction = new InputAction(Constants.ActionAPStabilityID),
                    FriendlyName = "Set AP Mode Stability Assist",
                    Setup = DoButtonSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Internal,
                    InputAction = new InputAction(Constants.ActionAPProgradeID),
                    FriendlyName = "Set AP Mode Prograde",
                    Setup = DoButtonSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Internal,
                    InputAction = new InputAction(Constants.ActionAPRetrogradeID),
                    FriendlyName = "Set AP Mode Retrograde",
                    Setup = DoButtonSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Internal,
                    InputAction = new InputAction(Constants.ActionAPNormalID),
                    FriendlyName = "Set AP Mode Normal",
                    Setup = DoButtonSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Internal,
                    InputAction = new InputAction(Constants.ActionAPAntinormalID),
                    FriendlyName = "Set AP Mode Antinormal",
                    Setup = DoButtonSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Internal,
                    InputAction = new InputAction(Constants.ActionAPRadialInID),
                    FriendlyName = "Set AP Mode Radial In",
                    Setup = DoButtonSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Internal,
                    InputAction = new InputAction(Constants.ActionAPRadialOutID),
                    FriendlyName = "Set AP Mode Radial Out",
                    Setup = DoButtonSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Internal,
                    InputAction = new InputAction(Constants.ActionAPTargetID),
                    FriendlyName = "Set AP Mode Target",
                    Setup = DoButtonSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Internal,
                    InputAction = new InputAction(Constants.ActionAPAntiTargetID),
                    FriendlyName = "Set AP Mode Anti Target",
                    Setup = DoButtonSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Internal,
                    InputAction = new InputAction(Constants.ActionAPManeuverID),
                    FriendlyName = "Set AP Mode Maneuver",
                    Setup = DoButtonSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Internal,
                    InputAction = new InputAction(Constants.ActionAPNavigationID),
                    FriendlyName = "Set AP Mode Navigation",
                    Setup = DoButtonSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Internal,
                    InputAction = new InputAction(Constants.ActionAPAutopilotID),
                    FriendlyName = "Set AP Mode Autopilot",
                    Setup = DoButtonSetup
                },
                new CategoryEnd(),
                CategoryActionGroups,
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
                new CategoryEnd(),
                CategoryFlightCamera,
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
                    InputAction = GameManager.Instance.Game.Input.Flight.FocusNext,
                    FriendlyName = "Focus Next Vessel",
                    Setup = DoButtonSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Game,
                    InputAction = GameManager.Instance.Game.Input.Flight.FocusPrev,
                    FriendlyName = "Focus Previous Vessel",
                    Setup = DoButtonSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Game,
                    InputAction = GameManager.Instance.Game.Input.Flight.CycleCameraMode,
                    FriendlyName = "Cycle Camera Mode",
                    Setup = DoButtonSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Game,
                    InputAction = GameManager.Instance.Game.Input.Flight.CameraFineMovement,
                    FriendlyName = "Camera Fine Movement",
                    Setup = DoButtonSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Game,
                    InputAction = GameManager.Instance.Game.Input.Flight.ToggleMouselook,
                    FriendlyName = "Toggle Mouse Look",
                    Setup = DoButtonSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Game,
                    InputAction = GameManager.Instance.Game.Input.Flight.ToggleVesselLabels,
                    FriendlyName = "Toggle Vessel Labels",
                    Setup = DoButtonSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Game,
                    InputAction = GameManager.Instance.Game.Input.Flight.ToggleFreeCamera,
                    FriendlyName = "Toggle Free Camera",
                    Setup = DoButtonSetup
                },
                new CategoryEnd(),
                CategoryEVA,
                new WrappedInputAction()
                {
                    Source = ActionSource.Game,
                    InputAction = GameManager.Instance.Game.Input.Flight.Interact,
                    FriendlyName = "Interact",
                    Setup = DoButtonSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Game,
                    InputAction = GameManager.Instance.Game.Input.Flight.InteractAlt,
                    FriendlyName = "Interact Alt",
                    Setup = DoButtonSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Game,
                    InputAction = GameManager.Instance.Game.Input.Flight.InteractAlt2,
                    FriendlyName = "Interact Alt 2",
                    Setup = DoButtonSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Game,
                    InputAction = GameManager.Instance.Game.Input.EVA.Jump,
                    FriendlyName = "EVA Jump",
                    Setup = DoButtonSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Game,
                    InputAction = GameManager.Instance.Game.Input.EVA.Run,
                    FriendlyName = "EVA Run",
                    Setup = DoButtonSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Game,
                    InputAction = GameManager.Instance.Game.Input.EVA.MoveFrontBack,
                    FriendlyName = "EVA Move Forward / Backward",
                    Setup = DoAxis2C1NSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Game,
                    InputAction = GameManager.Instance.Game.Input.EVA.MoveLeftRight,
                    FriendlyName = "EVA Move Left / Right",
                    Setup = DoAxis2C1NSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Game,
                    InputAction = GameManager.Instance.Game.Input.EVA.MoveStrafeLeftRight,
                    FriendlyName = "EVA Strafe Left / Right",
                    Setup = DoAxis2C1NSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Game,
                    InputAction = GameManager.Instance.Game.Input.EVA.MoveUpDown,
                    FriendlyName = "EVA Move Up / Down",
                    Setup = DoAxis2C1NSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Game,
                    InputAction = GameManager.Instance.Game.Input.EVA.RotatePitch,
                    FriendlyName = "EVA Rotate Pitch",
                    Setup = DoAxis2C1NSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Game,
                    InputAction = GameManager.Instance.Game.Input.EVA.RotateYaw,
                    FriendlyName = "EVA Rotate Yaw",
                    Setup = DoAxis2C1NSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Game,
                    InputAction = GameManager.Instance.Game.Input.EVA.RotateRoll,
                    FriendlyName = "EVA Rotate Roll",
                    Setup = DoAxis2C1NSetup
                },
                new CategoryEnd(),
                CategoryMap,
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
                    InputAction = GameManager.Instance.Game.Input.MapView.resetCamera,
                    FriendlyName = "Map Reset Camera",
                    Setup = DoButtonSetup
                },
                new CategoryEnd(),
                CategoryOAB,
                new WrappedInputAction()
                {
                    Source = ActionSource.Game,
                    InputAction = GameManager.Instance.Game.Input.VAB.delete,
                    FriendlyName = "VAB Delete",
                    Setup = DoButtonSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Game,
                    InputAction = GameManager.Instance.Game.Input.VAB.nextSymmetryMode,
                    FriendlyName = "Cycle Symmetry Mode Forward",
                    Setup = DoButtonSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Game,
                    InputAction = GameManager.Instance.Game.Input.VAB.previousSymmetryMode,
                    FriendlyName = "Cycle Symmetry Mode Back",
                    Setup = DoButton1MSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Game,
                    InputAction = GameManager.Instance.Game.Input.VAB.toggleAngleSnap,
                    FriendlyName = "Toggle Angle Snap",
                    Setup = DoButtonSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Game,
                    InputAction = GameManager.Instance.Game.Input.VAB.disableStackAttachment,
                    FriendlyName = "Disable Stack Attachment",
                    Setup = DoButtonSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Game,
                    InputAction = GameManager.Instance.Game.Input.VAB.resetPartRotation,
                    FriendlyName = "Reset Part Orientation",
                    Setup = DoButtonSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Game,
                    InputAction = GameManager.Instance.Game.Input.VAB.resetCamera,
                    FriendlyName = "VAB Reset Camera",
                    Setup = DoButtonSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Game,
                    InputAction = GameManager.Instance.Game.Input.VAB.copy,
                    FriendlyName = "Copy Selected Assembly",
                    Setup = DoButton1MSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Game,
                    InputAction = GameManager.Instance.Game.Input.VAB.paste,
                    FriendlyName = "Paste Copied Assembly",
                    Setup = DoButton1MSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Game,
                    InputAction = GameManager.Instance.Game.Input.VAB.search,
                    FriendlyName = "Search Parts",
                    Setup = DoButton1MSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Game,
                    InputAction = GameManager.Instance.Game.Input.VAB.undo,
                    FriendlyName = "Undo",
                    Setup = DoButton1N1MSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Game,
                    InputAction = GameManager.Instance.Game.Input.VAB.redo,
                    FriendlyName = "Redo",
                    Setup = DoButton1N1MSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Game,
                    InputAction = GameManager.Instance.Game.Input.VAB.cameraFastModifier,
                    FriendlyName = "Fast Camera Mode",
                    Setup = DoButtonSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Game,
                    InputAction = GameManager.Instance.Game.Input.VAB.rotatePitch,
                    FriendlyName = "Pitch Part",
                    Setup = DoAxis2C1NSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Game,
                    InputAction = GameManager.Instance.Game.Input.VAB.rotateYaw,
                    FriendlyName = "Yaw Part",
                    Setup = DoAxis2C1NSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Game,
                    InputAction = GameManager.Instance.Game.Input.VAB.rotateRoll,
                    FriendlyName = "Roll Part",
                    Setup = DoAxis2C1NSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Game,
                    InputAction = GameManager.Instance.Game.Input.VAB.toggleFrameOfReference,
                    FriendlyName = "Toggle Frame of Reference",
                    Setup = DoButtonSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Game,
                    InputAction = GameManager.Instance.Game.Input.VAB.selectAllPrimaryAssembly,
                    FriendlyName = "Select Launch Assembly",
                    Setup = DoButton1MSetup
                },
                new CategoryEnd(),
                CategoryRD,
                new WrappedInputAction()
                {
                    Source = ActionSource.Game,
                    InputAction = GameManager.Instance.Game.Input.RD.expandPartInfoTooltip,
                    FriendlyName = "Expand Part Info Tooltip",
                    Setup = DoButtonSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Game,
                    InputAction = GameManager.Instance.Game.Input.RD.ScrollTechTree,
                    FriendlyName = "Tech Tree Scroll",
                    Setup = DoAxis2C1NSetup
                },
                new CategoryEnd(),
                CategoryGeneral,
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
                    FriendlyName = "Quick Load (Hold)",
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
                    InputAction = GameManager.Instance.Game.Input.Global.TogglePlayerCheatMenu,
                    FriendlyName = "Toggle Cheat Menu",
                    Setup = DoButton1MSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Game,
                    InputAction = GameManager.Instance.Game.Input.Global.ToggleUIVisibility,
                    FriendlyName = "Toggle UI Visibility",
                    Setup = DoButtonSetup
                },
                new WrappedInputAction()
                {
                    Source = ActionSource.Game,
                    InputAction = GameManager.Instance.Game.Input.Global.ConfirmDialogue,
                    FriendlyName = "Confirm Dialogue",
                    Setup = DoButtonSetup
                },
                new CategoryEnd()
        };

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
                    string defaultPath2 = d.InputAction.bindings.Count >= 2 ? d.InputAction.bindings[1].path : null;
                    for (var jb = d.InputAction.bindings.Count - 1; jb >= 0; jb--)
                        d.InputAction.ChangeBinding(jb).Erase();
                    d.InputAction.AddBinding(path: defaultPath).WithName("Device 1");
                    d.InputAction.AddBinding(path: defaultPath2).WithName("Device 2");
                }
            }
            if (wasEnabled)
                d.InputAction.Enable();
        }

        public static void DoButton1MSetup(WrappedInputAction d)
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
                d.InputAction.AddCompositeBinding("OneModifier")
                    .With("modifier", null)
                    .With("binding", null);
                d.InputAction.AddCompositeBinding("OneModifier")
                    .With("modifier", null)
                    .With("binding", null);
            }
            else if (d.Source == ActionSource.Game)
            {
                if (d.InputAction.bindings.Count < 3 || !d.InputAction.bindings[0].isComposite)
                {
                    QLog.WarnLine($"Cannot load default path for {d.InputAction.name} - {d.InputAction.id}");
                    d.Source = ActionSource.Internal;
                    DoButtonSetup(d);
                }
                else
                {
                    string defaultModifier = d.InputAction.bindings[d.InputAction.FindNamedCompositePart(0, "modifier")].path;
                    string defaultBinding = d.InputAction.bindings[d.InputAction.FindNamedCompositePart(0, "binding")].path;
                    for (var jb = d.InputAction.bindings.Count - 1; jb >= 0; jb--)
                        d.InputAction.ChangeBinding(jb).Erase();
                    d.InputAction.AddCompositeBinding("OneModifier")
                        .With("modifier", defaultModifier)
                        .With("binding", defaultBinding);
                    d.InputAction.AddCompositeBinding("OneModifier")
                        .With("modifier", null)
                        .With("binding", null);
                }
            }
            if (wasEnabled)
                d.InputAction.Enable();
        }

        public static void DoButton1N1MSetup(WrappedInputAction d)
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
                d.InputAction.AddBinding(path: null).WithName("Button 1");
                d.InputAction.AddCompositeBinding("OneModifier")
                    .With("modifier", null)
                    .With("binding", null);
                d.InputAction.AddBinding(path: null).WithName("Button 2");
                d.InputAction.AddCompositeBinding("OneModifier")
                    .With("modifier", null)
                    .With("binding", null);
            }
            else if (d.Source == ActionSource.Game)
            {
                if (d.InputAction.bindings.Count < 4 || !d.InputAction.bindings[1].isComposite)
                {
                    QLog.WarnLine($"Cannot load default path for {d.InputAction.name} - {d.InputAction.id}");
                    d.Source = ActionSource.Internal;
                    DoButtonSetup(d);
                }
                else
                {
                    string defaultB1 = d.InputAction.bindings[0].path;
                    string defaultModifier = d.InputAction.bindings[d.InputAction.FindNamedCompositePart(1, "modifier")].path;
                    string defaultBinding = d.InputAction.bindings[d.InputAction.FindNamedCompositePart(1, "binding")].path;
                    for (var jb = d.InputAction.bindings.Count - 1; jb >= 0; jb--)
                        d.InputAction.ChangeBinding(jb).Erase();
                    d.InputAction.AddBinding(defaultB1).WithName("Button 1");
                    d.InputAction.AddCompositeBinding("OneModifier")
                        .With("modifier", defaultModifier)
                        .With("binding", defaultBinding);
                    d.InputAction.AddBinding(path: null).WithName("Button 2");
                    d.InputAction.AddCompositeBinding("OneModifier")
                        .With("modifier", null)
                        .With("binding", null);
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
