using System;

public class DeferredAction : DeferredActionBase
{
    private Action<Action> m_Task;

    public DeferredAction(Action<Action> task) : base()
    {
        m_Task = task;
    }

    public void Invoke(Action callback)
    {
        if (AttemptTaskInvoke(callback) == true)
        {
            m_Task?.Invoke(OnTaskComplete);
        }
    }
}

public class DeferredAction<T> : DeferredActionBase
{
    private Action<T, Action> m_Task;

    public DeferredAction(Action<T, Action> task) : base()
    {
        m_Task = task;
    }

    public void Invoke(T arg, Action callback)
    {
        if (AttemptTaskInvoke(callback) == true)
        {
            m_Task?.Invoke(arg, OnTaskComplete);
        }
    }
}