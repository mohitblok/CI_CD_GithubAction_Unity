using System;
using System.Collections;
using System.Collections.Generic;
using Logging;
using UnityEngine;

public class ExampleLogger : GameLogger
{
    private string _name;

    public ExampleLogger(string name, ILogHandler logHandler) : base(name, logHandler)
    {
        _name = name;
    }

    public void AddDebug(string msg)
    {
        Log("[ExampleLogger]" + msg);
    }

}

public class SamepleLogger : GameLogger
{
    private string _name;

    public SamepleLogger(string name, ILogHandler logHandler) : base(name, logHandler)
    {
        _name = name;
    }

    public void AddDebug(string msg)
    {
        Log("[SampleLogger]" + msg);
    }

}


