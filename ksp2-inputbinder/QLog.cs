﻿using KSP.Logging;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Codenade.Inputbinder
{
    // Mod specific logging and debug functions
    internal static class QLog
    {
        public static void Info(object message)
        {
            GlobalLog.Log(LogFilter.UserMod, $"[{Constants.Name}] {message}");
        }

        public static void Debug(object message)
        {
#if DEBUG
            GlobalLog.Log(LogFilter.UserMod, $"[{Constants.Name}] [DEBUG] {message}");
#endif
        }

        public static void Warn(object message)
        {
            GlobalLog.Warn(LogFilter.UserMod, $"[{Constants.Name}] {message}");
        }

        public static void Error(object message)
        {
            GlobalLog.Error(LogFilter.UserMod, $"[{Constants.Name}] {message}");
        }

        public static void InfoLine(object message, [CallerLineNumber] int lineNumber = -1, [CallerMemberName] string caller = null)
        {
            Info($"{message} {caller} line {lineNumber}");
        }

        public static void DebugLine(object message, [CallerLineNumber] int lineNumber = -1, [CallerMemberName] string caller = null)
        {
            Debug($"{message} {caller} line {lineNumber}");
        }

        public static void WarnLine(object message, [CallerLineNumber] int lineNumber = -1, [CallerMemberName] string caller = null)
        {
            Warn($"{message} {caller} line {lineNumber}");
        }

        public static void ErrorLine(object message, [CallerLineNumber] int lineNumber = -1, [CallerMemberName] string caller = null)
        {
            Error($"{message} {caller} line {lineNumber}");
        }

        public static void Print(object message, Category c)
        {
            switch (c)
            {
                case Category.Info:
                    Info(message);
                    break;
                case Category.Debug:
                    Debug(message);
                    break;
                case Category.Warn:
                    Warn(message);
                    break;
                case Category.Error:
                    Error(message);
                    break;
            }
        }

        public static void PrintEnumerable(IEnumerable message, Category c)
        {
            Print("<" + message.GetType().FullName + ">", c);
            foreach (var item in message)
                Print(item.ToString(), c);
            Print("</" + message.GetType().FullName + ">", c);
        }

        public enum Category
        {
            Info, Debug, Warn, Error
        }
    }
}
