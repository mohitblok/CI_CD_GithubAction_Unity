using UnityEngine;

namespace Logging
{
    public class GameLogger : Logger
    {
        public GameLogger(string name, ILogHandler logHandler) : base(logHandler)
        {
            Name = name;
            GameLog.AddLogger(this);
            Enable();
        }

        public string Name { get; }

        public bool Enabled => logEnabled;

        public void Enable() => GameLog.EnableLogger(Name);

        public void Disable() => GameLog.DisableLogger(Name);

        public override string ToString() => Name;
    }
}