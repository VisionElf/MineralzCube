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

    List<HarvestableEntity> harvestableEntities;

    void Start()
    {
        workerList = new List<Entity>();
        harvestableEntities = new List<HarvestableEntity>();
        CreateWorkers(startingWorkersCount);
    }

    void CreateWorker()
    {
        GameObject obj = GameObject.Instantiate(workerType);
        obj.transform.position = transform.position;
        Entity ent = obj.GetComponent<Entity>();
        ent.basicProperties.owner = basicProperties.owner;
        if (ent.IsHarvester())
            ent.harvesterProperties.mainBase = this;
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
        if (!harvestableEntities.Contains(entity))
        {
            foreach (Entity worker in workerList)
            {
                if (worker.IsHarvester() && !worker.harvesterProperties.IsHarvesting())
                {
                    worker.harvesterProperties.Harvest(entity);
                    harvestableEntities.Add(entity);
                    break;
                }
            }
        }
    }

    void OnGUI()
    {
        foreach (HarvestableEntity h in harvestableEntities)
        {
            if (h != null)
            {
                Vector3 pos = basicProperties.owner.playerCamera.WorldToScreenPoint(h.transform.position);
                pos.y = Screen.height - pos.y;

                int size = 20;
                GUI.color = new Color(1f, 0f, 0f, 0.5f);
                GUI.DrawTexture(new Rect(pos.x - size / 2, pos.y - size / 2, size, size), Static.basic_texture);
            }
        }
    }
}