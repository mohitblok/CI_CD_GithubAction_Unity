using System;
using System.Collections.Generic;
using System.Linq;

namespace Logging
{
    public static class GameLog
    {
        public static event Action<string> OnLoggerAdded;
        public static event Action<string, bool> OnLoggerActiveChanged;
        public static event Action<bool> OnDebugModeActiveChanged;
        public static event Action OnLoggerDisabled;

        public static bool DebugModeActive { get; set; }

        public static IList<KeyValuePair<string, GameLogger>> AllLoggers { get; private set; } = new List<KeyValuePair<string, GameLogger>>();
        public static IList<KeyValuePair<string, GameLogger>> ActiveLoggers { get; private set; } = new List<KeyValuePair<string, GameLogger>>();
        public static IList<KeyValuePair<string, GameLogger>> InActiveLoggers { get; private set; } = new List<KeyValuePair<string, GameLogger>>();

        private static IList<KeyValuePair<string, GameLogger>> CachedActive { get; set; } = new List<KeyValuePair<string, GameLogger>>();

        public static bool CheckActive(string loggerName)
        {
            var loggers = FindLoggers(loggerName);

            return loggers != null && loggers.First().Value.Enabled;
        }

        public static void ToggleLogger(string loggerName)
        {
            var logger = AllLoggers.FirstOrDefault(logger => logger.Key == loggerName).Value;

            if (logger.Enabled)
            {
                logger.Disable();

                return;
            }

            logger.Enable();
        }

        public static void EnableLogger(string loggerName)
        {
            InitLists();

            foreach (var logger in FindLoggers(loggerName))
            {
                AddActiveLogger(logger);
            }

            OnLoggerActiveChanged?.Invoke(loggerName, true);
        }

        public static void DisableLogger(string loggerName)
        {
            InitLists();

            foreach (var logger in FindLoggers(loggerName))
            {
                AddInactiveLogger(logger);
            }

            OnLoggerActiveChanged?.Invoke(loggerName, false);
            OnLoggerDisabled?.Invoke();
        }

        public static void EnterDebugMode()
        {
            InitLists();
            DebugModeActive = true;
            OnDebugModeActiveChanged?.Invoke(DebugModeActive);

            CachedActive = ActiveLoggers;

            foreach (var logger in AllLoggers)
            {
                logger.Value.Enable();
            }
        }

        public static void ExitDebugMode()
        {
            InitLists();
            DebugModeActive = false;
            OnDebugModeActiveChanged?.Invoke(DebugModeActive);

            foreach (var logger in AllLoggers)
            {
                logger.Value.Disable();
            }

            foreach (var logger in CachedActive)
            {
                logger.Value.Enable();
            }
        }

        internal static void AddLogger(GameLogger logger)
        {
            AllLoggers ??= new List<KeyValuePair<string, GameLogger>>();

            AllLoggers.Add(new KeyValuePair<string, GameLogger>(logger.ToString(), logger));
            OnLoggerAdded?.Invoke(logger.Name);
        }

        private static IEnumerable<KeyValuePair<string, GameLogger>> FindLoggers(string name)
        {
            AllLoggers ??= new List<KeyValuePair<string, GameLogger>>();

            return AllLoggers.Where(item => item.Key == name);
        }

        private static void InitLists()
        {
            AllLoggers ??= new List<KeyValuePair<string, GameLogger>>();
            ActiveLoggers ??= new List<KeyValuePair<string, GameLogger>>();
            InActiveLoggers ??= new List<KeyValuePair<string, GameLogger>>();
            CachedActive ??= new List<KeyValuePair<string, GameLogger>>();
        }

        private static void AddActiveLogger(KeyValuePair<string, GameLogger> logger)
        {
            logger.Value.logEnabled = true;

            ActiveLoggers.Remove(logger);
            InActiveLoggers.Remove(logger);
            ActiveLoggers.Add(logger);
        }

        private static void AddInactiveLogger(KeyValuePair<string, GameLogger> logger)
        {
            logger.Value.logEnabled = false;

            ActiveLoggers.Remove(logger);
            InActiveLoggers.Remove(logger);
            InActiveLoggers.Add(logger);
        }
    }
}