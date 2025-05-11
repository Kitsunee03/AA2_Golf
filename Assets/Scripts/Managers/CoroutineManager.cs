using System.Collections.Generic;

public class CoroutineManager : Singleton<CoroutineManager>
{
    private int m_taskID;
    private int m_jobID;

    private Dictionary<int, Job> m_jobs;
    private Dictionary<int, Task> m_tasks;

    private void OnDestroy()
    {
        // Tasks and jobs should be cleared when each script is destroyed, this is just in case something goes wrong.
        foreach (KeyValuePair<int, Task> entry in m_tasks)
        {
            if (entry.Value != null) { entry.Value.Stop(); }
        }
        m_tasks.Clear();

        foreach (KeyValuePair<int, Job> entry in m_jobs)
        {
            if (entry.Value != null) { entry.Value.Stop(); }
        }
        m_jobs.Clear();
    }

    protected override void Awake()
    {
        base.Awake();
        m_tasks = new();
        m_jobs = new();
    }

    private void OnDisable()
    {
        StopAllCoroutinesInManager();
        m_jobs.Clear();
        m_tasks.Clear();
    }

    public int GetTaskID(Task p_task)
    {
        if (p_task == null) { return -1; }
        int nextID = m_taskID++;
        m_tasks.Add(nextID, p_task);
        return nextID;
    }
    public int GetJobID(Job p_job)
    {
        if (p_job == null) { return -1; }
        int nextID = m_taskID++;
        m_jobs.Add(nextID, p_job);
        return nextID;
    }

    public void StopAllCoroutinesInManager()
    {
        foreach (KeyValuePair<int, Task> task in m_tasks) { task.Value.Stop(); }
    }

    public bool RemoveTask(Task p_task) { return m_tasks.Remove(p_task.ID); }
    public bool RemoveJob(Job p_job) { return m_tasks.Remove(p_job.ID); }
}