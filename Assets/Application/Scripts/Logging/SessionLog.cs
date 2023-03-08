using System;
using UnityEngine;

namespace Logging
{
    public class SessionLog
    {
        private DateTime _sessionStartTime;
        private DateTime _sessionEndTime;

        public static GameLogger Log { get; } = new("SessionLog", Debug.unityLogger.logHandler);

        public void StartSession()
        {
            _sessionStartTime = DateTime.Now;
            Log.Log(LogType.Log, $"Session Start Time: {_sessionStartTime} ");
        }

        public void EndSession()
        {
            _sessionEndTime = DateTime.Now;
            var timeDifference = _sessionEndTime.Subtract(_sessionStartTime);

            Log.Log($"Session End Time: {_sessionEndTime}");
            Log.Log($"Session Time Elapsed: {timeDifference}");
        }
    }
}