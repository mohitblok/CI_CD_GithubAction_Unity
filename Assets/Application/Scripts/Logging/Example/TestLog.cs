using System;
using System.Collections;
using System.Collections.Generic;
using Logging;
using UnityEngine;

public class TestLog : MonoBehaviour
{
    ExampleLogger exampleLogger;
    SamepleLogger samepleLogger;

    MyLogHandler myLogHandler;

    void Start()
    {
        myLogHandler = new MyLogHandler();

        exampleLogger = new ExampleLogger("example", myLogHandler);
        samepleLogger = new SamepleLogger("sample", myLogHandler);
    }

    private void Update()
    {
        if (exampleLogger.Enabled)
        {
            LoggerOne();
        }
        if (samepleLogger.Enabled)
        {
            LoggerTwo();
        }
    }

    public void LoggerOne()
    {
        exampleLogger.AddDebug("This is a test log from example logger");
    }

    public void LoggerTwo()
    {
        samepleLogger.AddDebug("This is a test log from sample logger");
    }

    private void GetLoggerCount()
    {
        Debug.Log("Active loggers count:" + GameLog.ActiveLoggers.Count);
    }
}

public class MyLogHandler : ILogHandler
{
    public void LogFormat(LogType logType, UnityEngine.Object context, string format, params object[] args)
    {
        Debug.unityLogger.logHandler.LogFormat(logType, context, format, args);
    }

    public void LogException(Exception exception, UnityEngine.Object context)
    {
        Debug.unityLogger.LogException(exception, context);
    }
}

