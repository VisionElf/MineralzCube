using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Task {

    int maxAssign;
    List<Entity> workersList;
    protected bool paused;

    public Task(int _maxAssign)
    {
        maxAssign = _maxAssign;
        workersList = new List<Entity>();
    }

    public void AssignWorker(Entity worker)
    {
        workersList.Add(worker);
        OnUpdateAssign();
    }
    public void UnassignWorker(Entity worker)
    {
        workersList.Remove(worker);
        worker.workerProperties.UnassignTask();
        OnUpdateAssign();
    }
    public void UnassignAllWorkers()
    {
        foreach (Entity worker in workersList)
            worker.workerProperties.UnassignTask();
        workersList.Clear();
        OnUpdateAssign();
    }


    public bool Assigned()
    {
        return workersList.Count == maxAssign;
    }
    public bool Unassigned()
    {
        return workersList.Count == 0;
    }

    public void Pause() { paused = true; }
    public bool Paused() { return paused; }
    public virtual bool PauseCondition(WorkerEntity worker) { return false; }

    public virtual bool Done() { return false; }
    public virtual void OnUpdateAssign() { }
    public virtual void OnAdd() { }
    public virtual void OnRemove() { }
    public virtual Entity GetTarget() { return null; }
    public virtual bool DoTask(WorkerEntity worker) { return false; }
    
}
