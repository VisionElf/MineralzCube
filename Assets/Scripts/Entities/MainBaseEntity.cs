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

    Queue<HarvestableEntity> nextHarvestableEntities;

    void Start()
    {
        workerList = new List<Entity>();
        nextHarvestableEntities = new Queue<HarvestableEntity>();
        CreateWorkers(startingWorkersCount);
    }

    void CreateWorker()
    {
        GameObject obj = GameObject.Instantiate(workerType);
        obj.transform.position = transform.position;
        Entity ent = obj.GetComponent<Entity>();
        ent.basicProperties.owner = basicProperties.owner;
        if (ent.IsHarvester())
        {
            ent.harvesterProperties.mainBase = this;
            ent.harvesterProperties.SeekHarvest();
        }
        if (ent != null)
            workerList.Add(ent);
    }

    public void CreateWorkers(int qty)
    {
        for (int i = 0; i < qty; i++)
            CreateWorker();
    }

    public HarvestableEntity GetHarvestableEntity()
    {
        if (nextHarvestableEntities.Count > 0)
        {
            HarvestableEntity entity = nextHarvestableEntities.Dequeue();
            bool reachable = Pathfinding.instance.FindPath(entity.transform.position, transform.position).Count > 0;
            if (reachable)
                return entity;
            else
                return GetHarvestableEntity();
        }
        return null;
    }

    public void OrderHarvest(HarvestableEntity entity)
    {
        if (!entity.IsHarvested() && !nextHarvestableEntities.Contains(entity))
        {
            foreach (Entity worker in workerList)
            {
                if (worker.IsHarvester() && !worker.harvesterProperties.IsHarvesting())
                {
                    if (Pathfinding.instance.FindPath(entity.transform.position, transform.position).Count == 0)
                        break;
                    else
                    {
                        worker.harvesterProperties.Harvest(entity);
                        return;
                    }
                }
            }

            nextHarvestableEntities.Enqueue(entity);
        }
    }

    void OnGUI()
    {
        int i = 0;
        foreach (HarvestableEntity h in nextHarvestableEntities)
        {
            if (h != null)
            {
                Vector3 pos = basicProperties.owner.playerCamera.WorldToScreenPoint(h.transform.position);
                pos.y = Screen.height - pos.y;

                Rect rect = new Rect(pos.x - 10, pos.y - 10, 40, 40);
                GUI.color = Color.red;
                GUI.Label(rect, "N" + i);
            }
            i++;
        }

        i = 0;
    }

}