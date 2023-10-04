using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Codenade.Inputbinder
{
    internal static class GlobalConfiguration
    {
        internal static readonly string path = Path.Combine(BepInEx.Paths.ConfigPath, "inputbinder/inputbinder.cfg");
        public static List<AAPBinding> aapBindings = new List<AAPBinding>();

        public static float SliderMin { get; set; } = -2;
        public static float SliderMax { get; set; } = 2;
        public static string DefaultProfile { get; set; } = "input";

        public static void Load()
        {
            if (!File.Exists(path))
                return;
            using (TextReader cr = File.OpenText(path))
            {
                string cl;
                bool sectMain = true;
                bool sectAAP = false;
                int lineNumber = 0;
                while ((cl = cr.ReadLine()) is object)
                {
                    lineNumber++;
                    if (Regex.IsMatch(cl, "^\\s*#+"))
                        continue;
                    if (Regex.IsMatch(cl, "^\\s*\\[main\\]\\s*$"))
                        sectMain = true;
                    else if (Regex.IsMatch(cl, "^\\s*\\[auto-add-processors\\]\\s*$"))
                        sectAAP = true;
                    var nvp = cl.IndexOf('=');
                    if (nvp == -1)
                        continue;
                    if (sectAAP)
                    {
                        var match = Regex.Match(cl, "^\\s*When(?<control_a>.+)MappedTo(?<control_b>.+)\\s*=\\s*\"(?<processors>.*)\"\\s*$");
                        if (match.Success)
                        {
                            var g_control_a = match.Groups["control_a"];
                            var g_control_b = match.Groups["control_b"];
                            var g_processors = match.Groups["processors"];
                            if (!g_control_a.Success || !g_control_b.Success || !g_processors.Success)
                            {
                                QLog.Warn("[Config] Syntax error detected at line " + lineNumber + ": " + cl);
                                continue;
                            }
                            aapBindings.Add(new AAPBinding(g_control_a.Value, g_control_b.Value, g_processors.Value));
                        }
                        else
                        {
                            QLog.Warn("[Config] Syntax error detected at line " + lineNumber + ": " + cl);
                            continue;
                        }
                    }
                    else if (sectMain)
                    {
                        var name = cl.Substring(0, nvp).Trim(' ');
                        var sVal = cl.Substring(nvp + 1).Trim(' ');
                        foreach (var ts in typeof(GlobalConfiguration).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
                        {
                            if (ts.Name == name)
                            {
                                if (ts.PropertyType == typeof(float) && float.TryParse(sVal, out var result))
                                {
                                    ts.SetValue(null, result);
                                }
                                else if (ts.PropertyType == typeof(string))
                                {
                                    var a = sVal.IndexOf('\"');
                                    var b = sVal.LastIndexOf('\"');
                                    if (a != -1 && b != -1)
                                        ts.SetValue(null, sVal.Substring(a + 1, b - a - 1));
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }

        public static void Save()
        {
            try
            {
                using (var cw = File.CreateText(path))
                {
                    foreach (var ts in typeof(GlobalConfiguration).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
                    {
                        if (ts.PropertyType != typeof(string))
                            cw.WriteLine(ts.Name + "=" + ts.GetValue(null).ToString());
                        else
                            cw.WriteLine(ts.Name + "=\"" + (string)ts.GetValue(null) + '\"');
                    }
                }
            }
            catch (Exception e)
            {
                QLog.ErrorLine(e);
            }
        }
    }

    internal struct AAPBinding
    {
        internal string A { get; set; }
        internal string B { get; set; }
        internal string ProcessorsToAdd { get; set; }

        internal AAPBinding(string a, string b, string processorsToAdd)
        {
            A = a;
            B = b;
            ProcessorsToAdd = processorsToAdd;
        }

        public override string ToString()
        {
            return A + " -> " + B + " => " + ProcessorsToAdd;
        }
    }
}
