using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MainBaseEntity : Entity {

    //UNITY PROPERTIES
    public int maxWorkersCount;
    public int startingWorkersCount;
    public GameObject workerType;

    //PROPERTIES
    List<Entity> workerList;

    void Start()
    {
        workerList = new List<Entity>();
        CreateWorkers(startingWorkersCount);
    }

    void CreateWorker()
    {
        GameObject obj = GameObject.Instantiate(workerType);
        obj.transform.position = transform.position;
        Entity ent = obj.GetComponent<Entity>();
        if (ent != null)
            workerList.Add(ent);
    }

    public void CreateWorkers(int qty)
    {
        for (int i = 0; i < qty; i++)
            CreateWorker();
    }

    public void OrderHarvest(HarvestableEntity entity)
    {
        foreach (Entity worker in workerList)
        {
            if (worker.IsHarvester() && !worker.harvesterProperties.IsHarvesting())
            {
                worker.harvesterProperties.Harvest(entity);
            }
        }
    }
}