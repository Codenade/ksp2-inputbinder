using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Codenade.Inputbinder
{
    internal static class GlobalConfiguration
    {
        public static float SliderMin { get; private set; } = -2;
        public static float SliderMax { get; private set; } = 2;

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
                    var nvp = cl.Split('=');
                    var name = nvp[0];
                    var sVal = nvp[1];
                    foreach (var ts in typeof(GlobalConfiguration).GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static))
                    {
                        if (ts.Name == name)
                        {
                            if (ts.PropertyType == typeof(float) && float.TryParse(sVal, out var result))
                            {
                                ts.SetValue(null, result);
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
                        cw.WriteLine(ts.Name + "=" + ts.GetValue(null).ToString());
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
