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

    List<HarvestableEntity> currentHarvestableEntities;
    List<HarvestableEntity> nextHarvestableEntities;

    void Start()
    {
        workerList = new List<Entity>();
        currentHarvestableEntities = new List<HarvestableEntity>();
        nextHarvestableEntities = new List<HarvestableEntity>();
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

    public void CleanCurrentHarvestableEntities()
    {
        while (currentHarvestableEntities.Count > 0 && currentHarvestableEntities.Contains(null))
            currentHarvestableEntities.Remove(null);
    }
    public HarvestableEntity GetHarvestableEntity()
    {
        if (nextHarvestableEntities.Count > 0)
        {
            HarvestableEntity entity = nextHarvestableEntities[0];
            nextHarvestableEntities.Remove(entity);
            currentHarvestableEntities.Add(entity);
            CleanCurrentHarvestableEntities();
            return entity;
        }
        return null;
    }

    public void OrderHarvest(HarvestableEntity entity)
    {
        if (!currentHarvestableEntities.Contains(entity) && !nextHarvestableEntities.Contains(entity))
        {
            if (Pathfinding.instance.FindPath(entity.transform.position, transform.position).Count == 0)
                return;

            foreach (Entity worker in workerList)
            {
                if (worker.IsHarvester() && !worker.harvesterProperties.IsHarvesting())
                {
                    worker.harvesterProperties.Harvest(entity);
                    currentHarvestableEntities.Add(entity);
                    return;
                }
            }
            
            nextHarvestableEntities.Add(entity);
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
        foreach (HarvestableEntity h in currentHarvestableEntities)
        {
            if (h != null)
            {
                Vector3 pos = basicProperties.owner.playerCamera.WorldToScreenPoint(h.transform.position);
                pos.y = Screen.height - pos.y;

                Rect rect = new Rect(pos.x - 10, pos.y - 10, 40, 40);
                GUI.color = Color.green;
                GUI.Label(rect, "C" + i);
            }
            i++;
        }
    }

}