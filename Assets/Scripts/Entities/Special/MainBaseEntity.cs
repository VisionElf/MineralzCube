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
            GameObject obj = GameObject.Instantiate(workerType.gameObject);
            obj.transform.position = transform.position;
            WorkerEntity ent = obj.GetComponent<WorkerEntity>();
            ent.basicProperties.owner = basicProperties.owner;
            ent.mainBase = this;
            ent.RequestTask();
            workerList.Add(ent);
        }
    }

    public void AssignTask(Task task)
    {
        if (!taskQueue.Contains(task))
        {
            taskQueue.Add(task);
            foreach (WorkerEntity w in workerList)
                if (!w.IsWorking())
                    w.RequestTask();
        }
    }
    public void RemoveTask(Task task)
    {
        if (!task.Unassigned())
            task.UnassignAllWorkers();
        task.OnRemove();
        taskQueue.Remove(task);
    }
    public Task GetNextTask()
    {
        Task temp = taskQueue.GetNextTask();
        if (temp != null)
        {
            if (!Pathfinding.instance.PathExists(transform.position, temp.GetTarget().transform.position))
            {
                RemoveTask(temp);
                return GetNextTask();
            }
        }
        return temp;
    }

    public void CreateWorkers(int qty)
    {
        for (int i = 0; i < qty; i++)
            CreateWorker();
    }
}