using KSP.Game;
using KSP.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine.InputSystem;

namespace Codenade.Inputbinder
{
    internal static class GameInputUtils
    {
        // Handling for game_actions_to_add.txt
        public static List<WrappedInputAction> Load(string path)
        {
            var outList = new List<WrappedInputAction>();
            if (!File.Exists(path))
                return outList;
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

        public static int GetPreviousCompositeBinding(this InputAction action, int startIndex)
        {
            if (action.bindings.Count - 1 < startIndex || startIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            for (var j = startIndex; j >= 0; j--)
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
    }
}
