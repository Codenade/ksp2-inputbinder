using KSP.Game;
using KSP.Input;
using KSP.Logging;
using System.Collections.Generic;
using System.IO;
using UnityEngine.InputSystem;

namespace Codenade.Inputbinder
{
    internal static class GameInputUtils
    {
        public static List<InputAction> Load(string path)
        {
            var outList = new List<InputAction>()
            {
                GameManager.Instance.Game.Input.Flight.ThrottleDelta,
                GameManager.Instance.Game.Input.Flight.ThrottleCutoff,
                GameManager.Instance.Game.Input.Flight.ThrottleMax,
                GameManager.Instance.Game.Input.Flight.Pitch,
                GameManager.Instance.Game.Input.Flight.Roll,
                GameManager.Instance.Game.Input.Flight.Yaw,
                GameManager.Instance.Game.Input.Flight.TogglePrecisionMode,
                GameManager.Instance.Game.Input.Flight.WheelSteer,
                GameManager.Instance.Game.Input.Flight.WheelBrakes,
                GameManager.Instance.Game.Input.Flight.WheelThrottle,
                GameManager.Instance.Game.Input.Flight.Stage,
                GameManager.Instance.Game.Input.Flight.ToggleLandingGear,
                GameManager.Instance.Game.Input.Flight.ToggleLights,
                GameManager.Instance.Game.Input.Flight.ToggleSAS,
                GameManager.Instance.Game.Input.Flight.ToggleRCS,
                GameManager.Instance.Game.Input.Flight.TranslateX,
                GameManager.Instance.Game.Input.Flight.TranslateY,
                GameManager.Instance.Game.Input.Flight.TranslateZ,
                GameManager.Instance.Game.Input.Flight.TriggerActionGroup1,
                GameManager.Instance.Game.Input.Flight.TriggerActionGroup2,
                GameManager.Instance.Game.Input.Flight.TriggerActionGroup3,
                GameManager.Instance.Game.Input.Flight.TriggerActionGroup4,
                GameManager.Instance.Game.Input.Flight.TriggerActionGroup5,
                GameManager.Instance.Game.Input.Flight.TriggerActionGroup6,
                GameManager.Instance.Game.Input.Flight.TriggerActionGroup7,
                GameManager.Instance.Game.Input.Flight.TriggerActionGroup8,
                GameManager.Instance.Game.Input.Flight.TriggerActionGroup9,
                GameManager.Instance.Game.Input.Flight.TriggerActionGroup10,
                GameManager.Instance.Game.Input.Flight.CameraPitchGamepad,
                GameManager.Instance.Game.Input.Flight.CameraYawGamepad,
                GameManager.Instance.Game.Input.Flight.ShowMap,
                GameManager.Instance.Game.Input.Global.QuickSave,
                GameManager.Instance.Game.Input.Global.TimeWarpDecrease,
                GameManager.Instance.Game.Input.Global.TimeWarpIncrease,
                GameManager.Instance.Game.Input.Global.TimeWarpStop,
                GameManager.Instance.Game.Input.Global.TogglePauseMenu,
                GameManager.Instance.Game.Input.Global.ToggleUIVisibility
            };
            if (!File.Exists(path))
                return outList;
            using (var reader = new StreamReader(path))
            {
                string line = null;
                while ((line = reader.ReadLine()) is object)
                {
                    var action = ParseSingleAction(line);
                    if (action is null)
                    {
                        GlobalLog.Log(LogFilter.UserMod, $"[{Constants.Name}] GameInputList: could not find action {line}");
                        continue;
                    }
                    outList.TryAddUnique(action);
                }
            }
            return outList;
        }

        public static InputAction ParseSingleAction(string input)
        {
            var splitInput = input.Split('.');
            if (splitInput.Length < 2)
                return null;
            var categoryProperty = typeof(GameInput).GetProperty(splitInput[0]);
            if (categoryProperty is null)
                return null;
            var actionProperty = categoryProperty.PropertyType.GetProperty(splitInput[1]);
            if (actionProperty is null)
                return null;
            if (actionProperty.PropertyType == typeof(InputAction))
                return (InputAction)actionProperty.GetValue(categoryProperty.GetValue(GameManager.Instance.Game.Input));
            return null;
        }
    }
}
