using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Codenade.Inputbinder
{
    internal static class GlobalConfiguration
    {
        public static float SliderMin { get; set; } = -2;
        public static float SliderMax { get; set; } = 2;
        public static string DefaultProfile { get; set; } = "input";

        public static void Load(string path)
        {
            if (!File.Exists(path))
                return;
            using (TextReader cr = File.OpenText(path))
            {
                string cl;
                while ((cl = cr.ReadLine()) is object)
                {
                    if (Regex.IsMatch(cl, "^\\s*#+") || !Regex.IsMatch(cl, ".+=.+"))
                        continue;
                    var nvp = cl.IndexOf('=');
                    if (nvp == -1)
                        continue;
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

        public static void Save(string path)
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
}
