using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TaskQueue {

    //PROPERIES
    List<Task> tasks;

    public TaskQueue()
    {
        tasks = new List<Task>();
    }

    public void Add(Task task)
    {
        tasks.Add(task);
        task.OnAdd();
    }
    public void Remove(Task task)
    {
        tasks.Remove(task);
    }
    public bool Contains(Task task)
    {
        if (tasks.Contains(task))
            return true;
        else
        {
            foreach (Task t in tasks)
            {
                if (task.GetTarget() == t.GetTarget())
                    return true;
            }
        }
        return false;
    }

    public int Count()
    {
        return tasks.Count;
    }

    public Task GetNextTask()
    {
        foreach (Task t in tasks)
        {
            if (!t.Assigned())
                return t;
        }
        return null;
    }
    public Task GetNextTask(WorkerEntity worker)
    {
        foreach (Task t in tasks)
        {
            if (t.Paused() && !t.PauseCondition(worker) && !t.Assigned())
                return t;
        }
        foreach (Task t in tasks)
        {
            if (!t.Assigned() && !t.Paused())
                return t;
        }
        return null;
    }
}
