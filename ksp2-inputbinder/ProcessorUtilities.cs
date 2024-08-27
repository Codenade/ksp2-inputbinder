using System;
using System.Reflection;
using UnityEngine.InputSystem;

namespace Codenade.Inputbinder
{
    public static class ProcessorUtilities
    {
        public static string ExtractSingleProcessor(string input, string processorName)
        {
            if (!input.Contains(processorName) || input.IndexOf('(') < 0 || input.IndexOf(')') < 0)
                return processorName + "()";
            var start = input.IndexOf(processorName);
            var end = input.IndexOf(')', start);
            return input.Substring(start, end - start + 1);
        }

        public static bool ExtractSingleProcessor(string input, string processorName, out string output)
        {
            if (!input.Contains(processorName) || input.IndexOf('(') < 0 || input.IndexOf(')') < 0)
            {
                output = processorName + "()";
                return false;
            }
                
            var start = input.IndexOf(processorName);
            var end = input.IndexOf(')', start);
            output = input.Substring(start, end - start + 1);
            return true; 
        }

        public static object Process(string processorString, object value, InputControl control)
        {
            var processorName = processorString.Substring(0, processorString.IndexOf('('));
            Type processorType = InputSystem.TryGetProcessor(processorName);
            if (processorType is null)
                return value;
            InputProcessor processorObj = (InputProcessor)Activator.CreateInstance(processorType);
            foreach (var parameterStr in processorString.Substring(processorName.Length + 1, processorString.IndexOf(')', processorName.Length) - (processorName.Length + 1)).Split(','))
            {
                string[] splits = parameterStr.Split('=');
                if (splits.Length == 2)
                {
                    var fldInf = processorType.GetField(splits[0], BindingFlags.Instance | BindingFlags.Public);
                    if (fldInf is null)
                        continue;
                    object procFldValue = null;
                    var fldType = fldInf.FieldType;
                    if (fldType.IsValueType)
                        procFldValue = Activator.CreateInstance(fldType);
                    if (fldType == typeof(string))
                        procFldValue = splits[1];
                    else if (fldType.GetMethod("TryParse", new Type[] { typeof(string), fldType.MakeByRefType() }) is MethodInfo tryParseMethod)
                    {
                        object[] mParams = new object[] { splits[1], procFldValue };
                        if (!(bool)tryParseMethod.Invoke(null, mParams))
                            continue;
                        procFldValue = mParams[1];
                    }
                    else if (fldType.GetMethod("Parse", new Type[] { typeof(string) }) is MethodInfo parseMethod)
                    {
                        try
                        {
                            procFldValue = parseMethod.Invoke(null, new object[] { splits[1] });
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        continue;
                    }

                    fldInf.SetValue(processorObj, procFldValue);
                }
            }
            return processorObj.ProcessAsObject(value, control);
        }
    }
}
