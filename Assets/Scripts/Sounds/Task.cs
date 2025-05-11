using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Task
{
    UnityEvent m_endOfTaskEvent;
    public void AddListenerToEndOfTaskEvent(UnityAction p_function) { m_endOfTaskEvent.AddListener(p_function); }
    public void RemoveListenerToEndOfTaskEvent(UnityAction p_function) { m_endOfTaskEvent.RemoveListener(p_function); }

    private bool m_isRunning = false;
    private bool m_isPaused = false;
    private bool m_isStopped = false;
    private bool m_isFinished = false;

    private bool m_isKilledWhenDone;

    private int m_ID;

    private Coroutine m_wrapper;

    private Func<IEnumerator> m_getCoroutine;

    public Task(Func<IEnumerator> p_action, bool p_isKilledWhenDone = false)
    {
        m_endOfTaskEvent = new UnityEvent();
        m_getCoroutine = p_action;
        m_ID = CoroutineManager.Instance.GetTaskID(this);
        m_isKilledWhenDone = p_isKilledWhenDone;

        if (!p_isKilledWhenDone) { return; }

        m_endOfTaskEvent.AddListener(KillTask);

    }
    ~Task()
    {
        Stop();
        m_endOfTaskEvent.RemoveAllListeners();
        CoroutineManager.Instance.RemoveTask(this);
        if (m_isKilledWhenDone) { m_endOfTaskEvent.RemoveListener(KillTask); }
    }

    IEnumerator Wrapper()
    {
        yield return null;

        IEnumerator e = m_getCoroutine();

        while (m_isRunning)
        {
            if (m_isPaused)
                yield return null;
            else
            {
                if (e != null && e.MoveNext())
                {
                    yield return e.Current;
                }
                else
                {
                    m_isRunning = false;
                    m_isFinished = true;
                    m_endOfTaskEvent.Invoke();
                }
            }
        }

    }

    private void KillTask() { CoroutineManager.Instance.RemoveTask(this); }
    public bool Pause()
    {
        if (!m_isRunning) { return false; }
        return m_isPaused = true;
    }
    public bool Unpause()
    {
        if (!m_isPaused) { return false; }
        m_isPaused = false;
        return true;
    }
    public bool Stop()
    {
        if (!HasStarted) { return false; }
        m_isStopped = true;
        m_isRunning = false;
        m_isPaused = false;
        CoroutineManager.Instance.StopCoroutine(Wrapper());
        return true;
    }
    public bool Restart()
    {
        if (!HasStarted) { return false; }
        m_wrapper = CoroutineManager.Instance.StartCoroutine(Wrapper());
        m_isRunning = true;
        m_isPaused = false;
        m_isFinished = false;
        return true;
    }
    public void Start()
    {
        m_isRunning = true;
        m_wrapper = CoroutineManager.Instance.StartCoroutine(Wrapper());
    }

    #region Accessors
    public bool HasStarted { get { return m_isRunning || !m_isRunning && m_isPaused || m_isStopped || !m_isFinished; } }
    public bool HasBeenStartedOnce { get { return m_isRunning || m_isPaused || m_isStopped; } }
    public bool IsRunning { get { return m_isRunning; } }
    public bool IsPaused { get { return m_isPaused; } }
    public bool IsStopped { get { return m_isStopped; } }
    public bool IsFinished { get { return m_isFinished; } }
    public int ID { get { return m_ID; } }
    #endregion
}
