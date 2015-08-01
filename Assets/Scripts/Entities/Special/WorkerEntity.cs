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
            {
                int bestQty = Mathf.Min(harvestQuantity, resource.GetRemainingResources(), resourceContainer.GetEmptyPlace());

                resourceContainer.AddResource(resource.resourceType, bestQty);
                resource.Harvest(bestQty);
            }
            return true;
        }
        return false;
    }
    public bool BuildBuilding(BuildingEntity building)
    {
        if (!resourceContainer.IsEmpty() && (building != null || !building.isBuilt))
        {
            if (basicProperties.Reached(building, harvestRange))
            {
                int bestQty = Mathf.Min(buildQuantity, building.GetRemainingResources(), resourceContainer.GetAllResourcesStock());

                resourceContainer.RemoveResource(EResourceType.Rock, buildQuantity);
                building.Build(bestQty);
            }
            return true;
        }
        return false;
    }
    public bool BackToBase()
    {
        return !basicProperties.Reached(mainBase, -mainBase.basicProperties.radius);
    }
    
    public bool BringCargo(ResourceEntity resource)
    {
        DepotEntity depot = basicProperties.owner.GetDepot();
        if (basicProperties.Reached(depot))
        {
            int bestQty = Mathf.Min(resourceContainer.GetAllResourcesStock(), depot.resourceContainer.GetEmptyPlace());

            resourceContainer.RemoveResource(EResourceType.Rock);
            depot.resourceContainer.AddResource(EResourceType.Rock, bestQty);
            return resource != null;
        }
        return true;
    }
    public bool GatherCargo(BuildingEntity building)
    {
        DepotEntity depot = basicProperties.owner.GetDepot();
        if (basicProperties.Reached(depot))
        {
            int bestQty = Mathf.Min(building.GetRemainingResources(), resourceContainer.GetEmptyPlace(), depot.resourceContainer.GetResourceStock(EResourceType.Rock).stock);
            
            depot.resourceContainer.RemoveResource(EResourceType.Rock, bestQty);
            resourceContainer.AddResource(EResourceType.Rock, bestQty);

            return !building.isBuilt;
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
