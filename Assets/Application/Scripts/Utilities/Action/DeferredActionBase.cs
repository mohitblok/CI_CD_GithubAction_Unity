using System;
using System.Collections.Generic;

public abstract class DeferredActionBase
{
    private List<Action> m_PendingCallbacks;
    private bool m_IsInProgress;
    private bool m_IsDone;

    protected DeferredActionBase()
    {
        m_PendingCallbacks = new List<Action>();
        m_IsInProgress = false;
        m_IsDone = false;
    }

    protected bool AttemptTaskInvoke(Action callback)
    {
        if (m_IsDone == true)
        {
            callback?.Invoke();
        }
        else
        {
            m_PendingCallbacks.Add(callback);

            if (m_IsInProgress == false)
            {
                m_IsInProgress = true;
                return true;
            }
        }
        return false;
    }

    protected void OnTaskComplete()
    {
        m_IsDone = true;
        m_PendingCallbacks.ForEach(c => c?.Invoke());
        m_PendingCallbacks.Clear();
    }
}
