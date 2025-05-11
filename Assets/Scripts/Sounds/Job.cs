using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using System;

public class Job
{
    UnityEvent m_endOfJobEvent;
    public void AddListenerToEndOfJobEvent(UnityAction p_function) { m_endOfJobEvent.AddListener(p_function); }
    public void RemoveListenerToEndOfJobEvent(UnityAction p_function) { m_endOfJobEvent.RemoveListener(p_function); }

    // A task represents a coroutine
    Task m_currentTask;

    Queue<Task> m_tasksToDo;
    List<Task> m_tasksCompleted;

    bool m_isAutomated;

    private int m_ID;

    public Job(bool p_isAutomated)
    {
        m_tasksToDo = new Queue<Task>();
        m_tasksCompleted = new List<Task>();

        m_isAutomated = p_isAutomated;
        m_endOfJobEvent = new UnityEvent();

        m_ID = CoroutineManager.Instance.GetJobID(this);
    }
    ~Job()
    {
        m_currentTask.Stop();
        m_endOfJobEvent.RemoveAllListeners();
        CoroutineManager.Instance.RemoveJob(this);
    }

    public void AddTask(Func<IEnumerator> p_action)
    {
        Task newTask = new Task(p_action);

        if (m_isAutomated)
        {
            newTask.AddListenerToEndOfTaskEvent(NextTask);
        }

        m_tasksToDo.Enqueue(newTask);
    }

    public void AddTask(Task p_task)
    {
        if (m_isAutomated)
        {
            p_task.AddListenerToEndOfTaskEvent(NextTask);
        }

        m_tasksToDo.Enqueue(p_task);
    }

    public bool Start()
    {
        if (m_tasksToDo.Count == 0) { return false; }

        m_currentTask = m_tasksToDo.Dequeue();
        m_currentTask.Start();

        return true;
    }

    public void NextTask()
    {
        m_tasksCompleted.Add(m_currentTask);
        if (m_tasksToDo.Count == 0)
        {
            m_endOfJobEvent.Invoke();
            return;
        }
        m_currentTask = m_tasksToDo.Dequeue();
        m_currentTask.Start();
    }

    public bool Pause()
    {
        if (m_currentTask == null) { return false; }
        return m_currentTask.Pause();
    }

    public bool Unpause()
    {
        if (m_currentTask == null) { return false; }
        return m_currentTask.Unpause();
    }

    public void Stop()
    {
        Queue<Task> newQueue = new Queue<Task>(m_tasksCompleted);
        newQueue.Enqueue(m_currentTask);
        while (m_tasksToDo.Count > 0)
        {
            newQueue.Enqueue(m_tasksToDo.Dequeue());
        }
        m_tasksToDo = newQueue;
        m_tasksCompleted = new List<Task>();

        if (m_currentTask != null)
        {
            m_currentTask.Stop();
            m_currentTask = null;
        }
    }

    public void Restart()
    {
        Stop();
        Start();
    }

    public bool IsRunning
    {
        get
        {
            if (m_currentTask == null) { return false; }
            else { return m_currentTask.IsRunning; }
        }
    }

    public int ID { get { return m_ID; } }

}
