using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainBaseEntity : Entity {

    //UNITY PROPERTIES
    public int maxWorkersCount;
    public int startingWorkersCount;
    public WorkerEntity workerType;

    //PROPERTIES
    List<WorkerEntity> workerList;

    TaskQueue taskQueue;

    void Start()
    {
        workerList = new List<WorkerEntity>();
        taskQueue = new TaskQueue();
        CreateWorkers(startingWorkersCount);
    }

    void CreateWorker()
    {
        if (workerList.Count < maxWorkersCount)
        {
            Entity entity = Map.instance.CreateEntityOnMap(workerType, transform.position);
            WorkerEntity worker = entity.workerProperties;
            worker.basicProperties.SetOwner(basicProperties.GetOwner());
            worker.mainBase = this;
            workerList.Add(worker);
            worker.RequestTask();
        }
    }

    public void AssignTask(Task task)
    {
        if (!taskQueue.Contains(task))
        {
            taskQueue.Add(task);
            NotifyWorkers();
        }
    }
    public void RemoveTask(Task task)
    {
        if (!task.Unassigned())
            task.UnassignAllWorkers();
        if (!task.Paused())
        {
            task.OnRemove();
            taskQueue.Remove(task);
        }
    }
    public Task GetNextTask(WorkerEntity worker)
    {
        Task temp = taskQueue.GetNextTask(worker);
        if (temp != null)
        {
            if (!worker.basicProperties.CanReach(temp.GetTarget()))
            {
                RemoveTask(temp);
                return GetNextTask(worker);
            }
        }
        return temp;
    }

    public void NotifyWorkers()
    {
        foreach (WorkerEntity worker in workerList)
            if (!worker.IsWorking())
                worker.RequestTask();
    }

    public int GetTaskCount()
    {
        return taskQueue.Count();
    }

    public void CreateWorkers(int qty)
    {
        for (int i = 0; i < qty; i++)
            CreateWorker();
    }
}