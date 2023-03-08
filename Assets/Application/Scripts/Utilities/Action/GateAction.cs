using System;

public class GateAction
{
    private Action m_Handler = null;
    private bool m_IsDone = false;

    public static GateAction operator +(GateAction gate, Action action)
    {
        gate.AddHandler(action);
        return gate;
    }

    public static GateAction operator -(GateAction gate, Action action)
    {
        gate.RemoveHandler(action);
        return gate;
    }

    public void AddHandler(Action action)
    {
        if (m_IsDone == true)
        {
            action?.Invoke();
        }
        else
        {
            m_Handler += action;
        }
    }

    public void RemoveHandler(Action action)
    {
        m_Handler -= action;
    }

    public void Invoke()
    {
        m_Handler?.Invoke();
        m_Handler = null;
        m_IsDone = true;
    }

    public void Reset()
    {
        m_IsDone = false;
    }
}