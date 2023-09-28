using KSP.Game;
using KSP.Input;
using KSP.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.InputSystem;

namespace Codenade.Inputbinder
{
    internal static class GameInputUtils
    {
        public static List<Tuple<InputAction, bool>> Load(string path)
        {
            var outList = new List<Tuple<InputAction, bool>>()
            {
                new Tuple<InputAction, bool>(GameManager.Instance.Game.Input.Flight.ThrottleDelta, true),
                new Tuple<InputAction, bool>(GameManager.Instance.Game.Input.Flight.ThrottleCutoff, false),
                new Tuple<InputAction, bool>(GameManager.Instance.Game.Input.Flight.ThrottleMax, false),
                new Tuple<InputAction, bool>(GameManager.Instance.Game.Input.Flight.Pitch, true),
                new Tuple<InputAction, bool>(GameManager.Instance.Game.Input.Flight.Roll, true),
                new Tuple<InputAction, bool>(GameManager.Instance.Game.Input.Flight.Yaw, true),
                new Tuple<InputAction, bool>(GameManager.Instance.Game.Input.Flight.TogglePrecisionMode, false),
                new Tuple<InputAction, bool>(GameManager.Instance.Game.Input.Flight.WheelSteer, true),
                new Tuple<InputAction, bool>(GameManager.Instance.Game.Input.Flight.WheelBrakes, true),
                new Tuple<InputAction, bool>(GameManager.Instance.Game.Input.Flight.WheelThrottle, true),
                new Tuple<InputAction, bool>(GameManager.Instance.Game.Input.Flight.Stage, false),
                new Tuple<InputAction, bool>(GameManager.Instance.Game.Input.Flight.ToggleLandingGear, false),
                new Tuple<InputAction, bool>(GameManager.Instance.Game.Input.Flight.ToggleLights, false),
                new Tuple<InputAction, bool>(GameManager.Instance.Game.Input.Flight.ToggleSAS, false),
                new Tuple<InputAction, bool>(GameManager.Instance.Game.Input.Flight.ToggleRCS, false),
                new Tuple<InputAction, bool>(GameManager.Instance.Game.Input.Flight.TranslateX, true),
                new Tuple<InputAction, bool>(GameManager.Instance.Game.Input.Flight.TranslateY, true),
                new Tuple<InputAction, bool>(GameManager.Instance.Game.Input.Flight.TranslateZ, true),
                new Tuple<InputAction, bool>(GameManager.Instance.Game.Input.Flight.TriggerActionGroup1, false),
                new Tuple<InputAction, bool>(GameManager.Instance.Game.Input.Flight.TriggerActionGroup2, false),
                new Tuple<InputAction, bool>(GameManager.Instance.Game.Input.Flight.TriggerActionGroup3, false),
                new Tuple<InputAction, bool>(GameManager.Instance.Game.Input.Flight.TriggerActionGroup4, false),
                new Tuple<InputAction, bool>(GameManager.Instance.Game.Input.Flight.TriggerActionGroup5, false),
                new Tuple<InputAction, bool>(GameManager.Instance.Game.Input.Flight.TriggerActionGroup6, false),
                new Tuple<InputAction, bool>(GameManager.Instance.Game.Input.Flight.TriggerActionGroup7, false),
                new Tuple<InputAction, bool>(GameManager.Instance.Game.Input.Flight.TriggerActionGroup8, false),
                new Tuple<InputAction, bool>(GameManager.Instance.Game.Input.Flight.TriggerActionGroup9, false),
                new Tuple<InputAction, bool>(GameManager.Instance.Game.Input.Flight.TriggerActionGroup10, false),
                new Tuple<InputAction, bool>(GameManager.Instance.Game.Input.Flight.CameraPitchGamepad, true),
                new Tuple<InputAction, bool>(GameManager.Instance.Game.Input.Flight.CameraYawGamepad, true),
                new Tuple<InputAction, bool>(GameManager.Instance.Game.Input.Flight.CameraZoom, true),
                new Tuple<InputAction, bool>(GameManager.Instance.Game.Input.Flight.ShowMap, false),
                new Tuple<InputAction, bool>(GameManager.Instance.Game.Input.Global.QuickSave, false),
                new Tuple<InputAction, bool>(GameManager.Instance.Game.Input.Global.TimeWarpDecrease, false),
                new Tuple<InputAction, bool>(GameManager.Instance.Game.Input.Global.TimeWarpIncrease, false),
                new Tuple<InputAction, bool>(GameManager.Instance.Game.Input.Global.TimeWarpStop, false),
                new Tuple<InputAction, bool>(GameManager.Instance.Game.Input.Global.TogglePauseMenu, false),
                new Tuple<InputAction, bool>(GameManager.Instance.Game.Input.Global.ToggleUIVisibility, false)
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

        public static Tuple<InputAction, bool> ParseSingleAction(string input)
        {
            var wantsAxis = input.StartsWith("#");
            if (wantsAxis)
                input = input.Substring(1);
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
                return new Tuple<InputAction, bool>((InputAction)actionProperty.GetValue(categoryProperty.GetValue(GameManager.Instance.Game.Input)), wantsAxis);
            return null;
        }
    }
}
