using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Codenade.Inputbinder
{
    internal class Utils
    {
        public static bool IsValidFileName(string name) => name.IndexOfAny(Path.GetInvalidFileNameChars().Concat(Path.GetInvalidPathChars()).ToArray()) < 0;

        public static bool IsValidFileNameCharacter(char c) => !(Path.GetInvalidFileNameChars().Contains(c) || Path.GetInvalidPathChars().Contains(c));

        public static string MarkText(string text, string toMark)
        {
            if (text is null)
                return null;
            if (toMark is null)
                return text;
            toMark = toMark.ToLower();
            if (text.ToLower().Contains(toMark))
            {
                var start = text.ToLower().IndexOf(toMark);
                var end = start + toMark.Length;
                return text[..start] + "<mark=#ffff00aa>" + text[start..end] + "</mark>" + text[end..];
            }
            else
                return text;
        }

        public static string GetFullDateTimeFormat(string localeName)
        {
            return GetTimeInfo(localeName, LOCALE_SSHORTDATE) + ' ' + GetTimeInfo(localeName, LOCALE_STIMEFORMAT);
        }

        public static string GetUserLocaleName()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new PlatformNotSupportedException();
            var localeName = new StringBuilder(LOCALE_NAME_MAX_LENGTH);
            GetUserDefaultLocaleName(localeName, localeName.Capacity);
            return localeName.ToString();
        }

        private static string GetTimeInfo(string localeName, uint LCType)
        {
            var tString = new StringBuilder(80);
            GetLocaleInfoEx(localeName, LCType, tString, tString.Capacity);
            return tString.ToString();
        }

        private static readonly uint LOCALE_SSHORTDATE = 0x0000001F;
        private static readonly uint LOCALE_STIMEFORMAT = 0x00001003;
        private static readonly int LOCALE_NAME_MAX_LENGTH = 85;

        [DllImport("kernel32.dll")]
        private static extern int GetUserDefaultLocaleName([MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpLocaleName, int cchLocaleName);

        [DllImport("kernel32.dll")]
        private static extern int GetLocaleInfoEx([MarshalAs(UnmanagedType.LPWStr)] string lpLocaleName, uint LCType, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpLCData, int cchData);
    }
}
