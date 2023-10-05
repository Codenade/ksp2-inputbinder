using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Codenade.Inputbinder
{
    internal static class GlobalConfiguration
    {
        internal static readonly string path = Path.Combine(BepInEx.Paths.ConfigPath, "inputbinder/inputbinder.cfg");
        internal static readonly string profilePath = Path.Combine(BepInEx.Paths.ConfigPath, "inputbinder/defaultprofile.txt");
        public static List<AAPBinding> aapBindings = new List<AAPBinding>();

        [ConfigProperty("main")]
        public static float SliderMin { get; set; } = -2;
        [ConfigProperty("main")]
        public static float SliderMax { get; set; } = 2;
        public static string DefaultProfile { get; set; } = "input";

        public static void Load()
        {
            if (!File.Exists(path))
            {
                Create();
                return;
            }
            if (File.Exists(profilePath))
            {
                string dp = File.ReadAllText(profilePath);
                dp.Trim();
                if (Utils.IsValidFileName(dp))
                    DefaultProfile = dp;
            }
            else
                SaveDefaultProfile();
            aapBindings.Clear();
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
                        var name = cl.Substring(0, nvp).Trim();
                        var sVal = cl.Substring(nvp + 1).Trim();
                        foreach (var ts in typeof(GlobalConfiguration).GetProperties(BindingFlags.Public | BindingFlags.Static))
                        {
                            var attr = ts.GetCustomAttribute(typeof(ConfigPropertyAttribute));
                            if (attr is null || ((ConfigPropertyAttribute)attr).Section != "main")
                                continue;
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

        public static void SaveDefaultProfile()
        {
            using (var ct = File.CreateText(profilePath))
            {
                ct.Write(DefaultProfile);
            }
        }

        public static void Create()
        {
            if (File.Exists(path))
                return;
            try
            {
                using (var cw = File.CreateText(path))
                {
                    cw.WriteLine("# Documentation available at:");
                    cw.WriteLine("# https://github.com/Codenade/ksp2-inputbinder/wiki/Configuration");
                    cw.WriteLine("[main]");
                    foreach (var ts in typeof(GlobalConfiguration).GetProperties(BindingFlags.Public | BindingFlags.Static))
                    {
                        var attr = ts.GetCustomAttribute(typeof(ConfigPropertyAttribute));
                        if (attr is null || ((ConfigPropertyAttribute)attr).Section != "main")
                            continue;
                        if (ts.PropertyType != typeof(string))
                            cw.WriteLine(ts.Name + "=" + ts.GetValue(null).ToString());
                        else
                            cw.WriteLine(ts.Name + "=\"" + (string)ts.GetValue(null) + '\"');
                    }
                    cw.WriteLine();
                    cw.WriteLine("# Uncomment the follwing section to automatically add ");
                    cw.WriteLine("# a Scale processor with factor = 15 to every binding ");
                    cw.WriteLine("# where you bind a Button an Axis binding.");
                    cw.WriteLine("# [auto-add-processors]");
                    cw.WriteLine("# WhenButtonMappedToAxis=\"Scale(factor=15)\"");
                }
            }
            catch (Exception e)
            {
                QLog.ErrorLine(e);
            }
            SaveDefaultProfile();
        }

        protected class ConfigPropertyAttribute : Attribute
        {
            public string Section { get; set; }

            public ConfigPropertyAttribute(string section)
            {
                Section = section;
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
