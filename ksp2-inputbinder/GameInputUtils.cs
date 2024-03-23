using KSP.Game;
using KSP.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Codenade.Inputbinder
{
    internal static class GameInputUtils
    {
        // Handling for game_actions_to_add.txt
        public static List<IWrappedInputAction> Load(string path)
        {
            var outList = new List<IWrappedInputAction>();
            if (!File.Exists(path))
                return outList;
            outList.Add(DefaultInputActionDefinitions.CategoryCustom);
            using (var reader = new StreamReader(path))
            {
                string line = null;
                while ((line = reader.ReadLine()) is object)
                {
                    var action = ParseSingleAction(line);
                    if (action.InputAction is null)
                    {
                        QLog.Info($"GameInputList: could not find action {line}");
                        continue;
                    }
                    outList.TryAddUnique(action);
                }
            }
            outList.Add(new CategoryEnd());
            return outList;
        }

        public static WrappedInputAction ParseSingleAction(string input)
        {
            var flagChangeBindings = input.Contains("#");
            string sfDef;
            string actDef;
            if (flagChangeBindings)
            {
                actDef = input.Substring(0, input.IndexOf('#'));
                sfDef = input.Substring(input.IndexOf('#') + 1);
            }
            else
            {
                actDef = input;
                sfDef = null;
            }
            var splitInput = actDef.Split('.');
            if (splitInput.Length < 2)
                return default;
            var categoryProperty = typeof(GameInput).GetProperty(splitInput[0]);
            if (categoryProperty is null)
                return default;
            var actionProperty = categoryProperty.PropertyType.GetProperty(splitInput[1]);
            if (actionProperty is null)
                return default;
            if (actionProperty.PropertyType == typeof(InputAction))
            {
                var ia = (InputAction)actionProperty.GetValue(categoryProperty.GetValue(GameManager.Instance.Game.Input));
                Action<WrappedInputAction> sf = null;
                if (sfDef is object)
                {
                    foreach (var candidate in typeof(DefaultInputActionDefinitions).GetMethods(BindingFlags.Public | BindingFlags.Static))
                    {
                        var ps = candidate.GetParameters();
                        if (ps.Length != 1 || ps[0].ParameterType != typeof(WrappedInputAction) || candidate.ReturnType != typeof(void) || candidate.Name != sfDef)
                            continue;
                        sf = (Action<WrappedInputAction>)candidate.CreateDelegate(typeof(Action<WrappedInputAction>));
                    }
                }
                return new WrappedInputAction(ActionSource.Game, ia, ia.name, sf);
            }
            return default;
        }

        public static bool IsNullOrEmpty(this string s) => s is null || s == string.Empty;

        public static int GetPreviousCompositeBinding(this InputAction action, int fromIndex)
        {
            if (action.bindings.Count - 1 < fromIndex || fromIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(fromIndex));
            for (var j = fromIndex; j >= 0; j--)
            {
                if (action.bindings[j].isComposite)
                    return j;
            }
            return -1;
        }

        public static int FindNamedCompositePart(this InputAction action, int startIndex, string name)
        {
            if (action.bindings.Count - 1 < startIndex || startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            if (name is null)
                throw new ArgumentNullException(nameof(name));
            for (var i = startIndex + 1; i < action.bindings.Count; i++)
            {
                if (!action.bindings[i].isPartOfComposite)
                    return -1;
                if (action.bindings[i].name.ToLower() == name.ToLower())
                    return i;
            }
            return -1;
        }

        public static bool HasAnyOverrides(this InputAction action)
        {
            foreach (var bdg in action.bindings)
            {
                if (bdg.hasOverrides)
                    return true;
            }
            return false;
        }

        public static void OnStateChange(this InputAction action, Action<InputAction.CallbackContext> func)
        {
            action.started += func;
            action.performed += func;
            action.canceled += func;
        }

        public static Dictionary<string, Type> GetInputSystemLayouts()
        {
            var im = typeof(InputSystem).GetField("s_Manager", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            var col = im.GetType().GetField("m_Layouts", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(im);
            Dictionary<InternedString, Type> types = (Dictionary<InternedString, Type>)col.GetType().GetField("layoutTypes", BindingFlags.Public | BindingFlags.Instance).GetValue(col);
            Dictionary<string, Type> nTypes = new Dictionary<string, Type>();
            foreach (var kvp in types)
                nTypes.Add(kvp.Key.ToString(), kvp.Value);
            return nTypes;
        }

        public static string GetInputActionContainerName(InputAction action)
        {
            var gameInput = GameManager.Instance.Game.Input;
            List<object> searchTargets = new List<object>()
            {
                gameInput.Audio,
                gameInput.ConsoleToolbox,
                gameInput.Cursor,
                gameInput.EVA,
                gameInput.Flight,
                gameInput.Global,
                gameInput.KSC,
                gameInput.MapView,
                gameInput.Navigation_Move_DPad,
                gameInput.Navigation_Move_LeftStick,
                gameInput.Navigation_Quit,
                gameInput.Navigation_Scroll,
                gameInput.Navigation_Slider,
                gameInput.Navigation_Submit,
                gameInput.RD,
                gameInput.VAB
            };
            
            foreach (var target in searchTargets)
            {
                foreach (var prop in target.GetType().GetProperties().Where((x) => x.PropertyType == typeof(InputAction)))
                {
                    if (prop.GetValue(target) == action)
                        return typeof(GameInput).GetProperties().Where((x) => x.PropertyType == target.GetType()).First().Name;
                }
            }
            return null;
        }
    }
}
