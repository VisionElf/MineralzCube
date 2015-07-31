﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Task {

    int maxAssign;
    List<Entity> workersList;

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

    public virtual void OnUpdateAssign() { }
    public virtual void OnAdd() { }
    public virtual void OnRemove() { }
    public virtual Entity GetTarget() { return null; }
    public virtual bool DoTask(Entity worker) { return false; }
    
}
