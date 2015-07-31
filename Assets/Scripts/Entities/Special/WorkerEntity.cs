using UnityEngine;
using System.Collections;

public class WorkerEntity : Entity {

    //UNITY PROPERTIES
    public ResourceContainer resourceContainer;

    //BUILD
    public float buildSpeed;
    public float buildRange;
    public int buildQuantity;

    //HARVEST
    public float harvestSpeed;
    public int harvestQuantity;
    public float harvestRange;

    //PROPERTIES
    public MainBaseEntity mainBase { get; set; }

    Task currentTask;

    //FUNCTIONS
    void Start()
    {
        resourceContainer.Initialize(gameObject);
    }

    public bool HarvestResource(ResourceEntity resource)
    {
        if (!resourceContainer.IsFull() && (resource != null || !resource.Empty()))
        {
            if (basicProperties.Reached(resource, harvestRange))
                resourceContainer.AddResource(resource.resourceType, resource.Harvest(harvestQuantity));
            return true;
        }
        return false;
    }
    public bool BuildBuilding(BuildingEntity building)
    {
        if (!resourceContainer.IsEmpty() && (building != null || !building.isBuilt))
        {
            if (basicProperties.Reached(building, harvestRange))
                building.Build(resourceContainer.RemoveResource(EResourceType.Rock, buildQuantity));
            return true;
        }
        return false;
    }
    public bool BackToBase()
    {
        return !basicProperties.Reached(mainBase, -mainBase.basicProperties.radius);
    }

    public bool BringCargo()
    {
        DepotEntity depot = basicProperties.owner.GetDepot();
        if (basicProperties.Reached(depot))
        {
            foreach (ResourceStock r in resourceContainer.resourcesStocks)
                depot.resourceContainer.AddResource(r.resourceType, resourceContainer.RemoveResource(r.resourceType, r.stock));
            return false;
        }
        return true;
    }
    public bool GatherCargo(BuildingEntity building)
    {
        DepotEntity depot = basicProperties.owner.GetDepot();
        if (basicProperties.Reached(depot))
        {
            foreach (ResourceStock r in resourceContainer.resourcesStocks)
                resourceContainer.AddResource(r.resourceType, depot.resourceContainer.RemoveResource(r.resourceType,
                    (int)Mathf.Min(r.stock, depot.resourceContainer.GetResourceStock(r.resourceType).stock, building.GetRemainingResources())));
            return false;
        }
        return true;
    }


    public bool IsWorking()
    {
        return currentTask != null;
    }

    public void RequestTask()
    {
        if (currentTask != null)
            mainBase.RemoveTask(currentTask);
        Task nextTask = mainBase.GetNextTask();
        if (nextTask != null)
            AssignTask(nextTask);
    }

    public void AssignTask(Task task)
    {
        StopCoroutine("Task");
        currentTask = task;
        if (currentTask != null && basicProperties.CanReach(task.GetTarget()))
        {
            currentTask.AssignWorker(this);
            StartCoroutine("Task");
        }
        else
            OnTaskDone();
    }
    public void UnassignTask()
    {
        currentTask = null;
    }
    public void OnTaskDone()
    {
        RequestTask();
    }

    IEnumerator Task()
    {
        while (currentTask.DoTask(this))
            yield return null;

        OnTaskDone();
        while (BackToBase())
            yield return null;
    }
}
