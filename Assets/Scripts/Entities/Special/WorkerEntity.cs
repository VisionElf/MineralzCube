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
    DepotEntity currentNearestDepot;

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
        if (!resourceContainer.IsEmpty() && (building != null && !building.isBuilt))
        {
            if (basicProperties.Reached(building, harvestRange))
            {
                int bestQty = Mathf.Min(buildQuantity, building.GetRemainingResources(), resourceContainer.GetAllResourcesStock());

                resourceContainer.RemoveResource(EResourceType.Rock, bestQty);
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
    
    public bool BringCargo()
    {
        if (currentNearestDepot == null)
            currentNearestDepot = basicProperties.owner.GetNearestDepotNotFull(transform.position);

        if (basicProperties.Reached(currentNearestDepot))
        {
            int bestQty = Mathf.Min(resourceContainer.GetAllResourcesStock(), currentNearestDepot.resourceContainer.GetEmptyPlace());

            resourceContainer.RemoveResource(EResourceType.Rock, bestQty);
            currentNearestDepot.resourceContainer.AddResource(EResourceType.Rock, bestQty);
            currentNearestDepot = null;
            return false;
        }
        return true;
    }
    public bool GatherCargo(BuildingEntity building)
    {
        if (currentNearestDepot == null)
            currentNearestDepot = basicProperties.owner.GetNearestDepotNotEmpty(transform.position);

        if (basicProperties.Reached(currentNearestDepot))
        {
            int bestQty = Mathf.Min(building.GetRemainingResources(), resourceContainer.GetEmptyPlace(), currentNearestDepot.resourceContainer.GetResourceStock(EResourceType.Rock).stock);

            currentNearestDepot.resourceContainer.RemoveResource(EResourceType.Rock, bestQty);
            resourceContainer.AddResource(EResourceType.Rock, bestQty);
            currentNearestDepot = null;
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
        Task nextTask = mainBase.GetNextTask(this);
        if (nextTask != null)
            AssignTask(nextTask);
    }

    public void AssignTask(Task task)
    {
        StopCoroutine("Task");
        currentNearestDepot = null;
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
        if (currentTask != null)
            mainBase.RemoveTask(currentTask);
        RequestTask();
    }

    IEnumerator Task()
    {
        while (currentTask != null && !currentTask.PauseCondition(this) && currentTask.DoTask(this))
            yield return null;

        OnTaskDone();

        while (!resourceContainer.IsEmpty() && BringCargo())
            yield return null;
        while (BackToBase())
            yield return null;
    }
}
