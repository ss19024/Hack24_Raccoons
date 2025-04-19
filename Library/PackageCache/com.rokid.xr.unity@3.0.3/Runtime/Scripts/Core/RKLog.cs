namespace Rokid.UXR
{
    /// <summary>
    /// Log Utility
    /// </summary>
    public class RKLog
    {
        public enum LogLevel
        {
            Debug,
            Info,
            Warning,
            Error
        }

        private static LogLevel logLevel = LogLevel.Info;

        private static bool logEnable = true;

        public static void SetLogEnable(bool logEnable)
        {
            RKLog.logEnable = logEnable;
        }

        public static void SetLogLevel(LogLevel logLevel)
        {
            RKLog.logLevel = logLevel;
        }

        public static void Info(string msg)
        {
            if (logLevel <= LogLevel.Info && logEnable)
            {
                UnityEngine.Debug.Log($"[RKLogInfo]:{msg}");
            }
        }

        public static void Debug(string msg)
        {
            if (logLevel <= LogLevel.Debug && logEnable)
            {
                UnityEngine.Debug.Log($"[RKLogDebug]:{msg}");
            }
        }

        public static void Warning(string msg)
        {
            if (logLevel <= LogLevel.Warning && logEnable)
            {
                UnityEngine.Debug.LogWarning($"[RKLogWarning]:{msg}");
            }
        }

        public static void Error(string msg)
        {
            if (logEnable)
                UnityEngine.Debug.LogError($"[RKLogError]:{msg}");
        }

        public static void KeyInfo(string msg)
        {
            UnityEngine.Debug.Log($"[RKKeyInfo]:{msg}");
        }
    }
}
