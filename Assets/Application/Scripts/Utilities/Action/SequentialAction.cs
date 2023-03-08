using System;
using System.Collections.Generic;

public class SequentialAction
{
    private Action<int, Action> m_OnTick;
    private Action m_OnComplete;

    private int m_Current;
    private int m_Count;

    private SequentialAction(int count, Action<int, Action> onTick, Action onComplete)
    {
        m_OnComplete = onComplete;
        m_OnTick = onTick;
        m_Count = count;

        Tick();
    }

    private void Tick()
    {
        m_OnTick(m_Current, Increment);
    }

    private void Increment()
    {
        m_Current++;

        if (m_Current == m_Count)
        {
            m_OnComplete?.Invoke();
        }
        else
        {
            Tick();
        }
    }

    public static void Count(int count, Action<int, Action> onTick, Action onComplete)
    {
        if (count > 0)
        {
            new SequentialAction(count, onTick, onComplete);
        }
        else
        {
            onComplete?.Invoke();
        }
    }

    public static void List<T>(IList<T> list, Action<T, Action> onTick, Action onComplete = null)
    {
        if (list != null)
        {
            Count(list.Count, (i, tick) => onTick(list[i], tick), onComplete);
        }
        else
        {
            onComplete?.Invoke();
        }
    }

    public static void WithResult<T, U>(IList<T> list, Action<T, Action<U>> onTick, Action<List<U>> onComplete)
    {
        var resultList = new List<U>();
        List(list, (i, tick) =>
        {
            onTick(i, result =>
            {
                resultList.Add(result);
                tick();
            });
        }, () => onComplete(resultList));
    }
}
