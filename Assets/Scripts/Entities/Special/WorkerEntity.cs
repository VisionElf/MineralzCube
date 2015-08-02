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

    public float depositSpeed;
    public int depositQuantity;

    //PROPERTIES
    public MainBaseEntity mainBase { get; set; }
    DepotEntity currentNearestDepot;

    Task currentTask;

    //FUNCTIONS
    void Start()
    {
        resourceContainer.Initialize(this);
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

    public bool HarvestResource2(ResourceEntity resource)
    {
        int bestQty = Mathf.Min(harvestQuantity, resource.GetRemainingResources(), resourceContainer.GetEmptyPlace());

        resourceContainer.AddResource(resource.resourceType, bestQty);
        resource.Harvest(bestQty);
        return !resourceContainer.IsFull();
    }
    public bool BuildBuilding2(BuildingEntity building)
    {
        int bestQty = Mathf.Min(buildQuantity, building.GetRemainingResources(), resourceContainer.GetAllResourcesStock());

        resourceContainer.RemoveResource(EResourceType.Rock, bestQty);
        building.Build(bestQty);
        return !resourceContainer.IsEmpty();
    }
    public bool DepositResource(DepotEntity depot)
    {
        int bestQty = Mathf.Min(depositQuantity, resourceContainer.GetAllResourcesStock(), depot.resourceContainer.GetEmptyPlace());

        resourceContainer.RemoveResource(EResourceType.Rock, bestQty);
        depot.resourceContainer.AddResource(EResourceType.Rock, bestQty);
        return !resourceContainer.IsEmpty() && !depot.IsFull();
    }
    public bool WithdrawResource(DepotEntity depot)
    {
        int bestQty = Mathf.Min(depositQuantity, resourceContainer.GetEmptyPlace(), depot.resourceContainer.GetResourceStock(EResourceType.Rock).stock);

        depot.resourceContainer.RemoveResource(EResourceType.Rock, bestQty);
        resourceContainer.AddResource(EResourceType.Rock, bestQty);
        return !resourceContainer.IsFull() && !depot.IsEmpty();
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
        StopAllCoroutines();
        currentNearestDepot = null;
        currentTask = task;
        if (currentTask != null && basicProperties.CanReach(task.GetTarget()))
        {
            currentTask.AssignWorker(this);
            if (currentTask.GetType() == typeof(HarvestTask))
                StartCoroutine("Harvest");
            else if (currentTask.GetType() == typeof(BuildTask))
                StartCoroutine("Build");
            //StartCoroutine("Task");
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

    IEnumerator Harvest()
    {
        // HARVEST CONDITION
        while (!currentTask.Done() && !currentTask.PauseCondition(this))
        {
            //REACH
            while (!basicProperties.Reached(currentTask.GetTarget(), harvestRange))
                yield return null;
            //HARVEST RESOURCE
            while (!currentTask.Done() && currentTask.DoTask(this))
                yield return new WaitForSeconds(harvestSpeed);

            // EMPTY CONTAINER
            if (!resourceContainer.IsEmpty())
            {
                DepotEntity depot = basicProperties.owner.GetNearestDepotNotFull(transform.position);
                if (depot != null)
                {
                    //REACH
                    while (!basicProperties.Reached(depot))
                        yield return null;
                    //DEPOSIT
                    while (DepositResource(depot))
                        yield return new WaitForSeconds(depositSpeed);
                }
            }
        }

        OnTaskDone();

        //WHEN OVER RETURN TO BASE
        while (!basicProperties.Reached(mainBase, -mainBase.basicProperties.radius))
            yield return null;
    }
    IEnumerator Build()
    {
        // HARVEST CONDITION
        while (!currentTask.Done() && !currentTask.PauseCondition(this))
        {
            //FILL CONTAINER
            DepotEntity depot = basicProperties.owner.GetNearestDepotNotEmpty(transform.position);
            if (depot != null)
            {
                //REACH
                while (!basicProperties.Reached(depot))
                    yield return null;
                //DEPOSIT
                while (WithdrawResource(depot))
                    yield return new WaitForSeconds(depositSpeed);
            }

            if (!resourceContainer.IsEmpty())
            {
                //REACH
                while (!basicProperties.Reached(currentTask.GetTarget(), buildRange))
                    yield return null;
                //HARVEST RESOURCE
                while (!currentTask.Done() && currentTask.DoTask(this))
                    yield return new WaitForSeconds(buildSpeed);
            }
        }

        OnTaskDone();

        //IF THERE IS STILL RESOURCES IN CARGO
        if (!resourceContainer.IsEmpty())
        {
            // EMPTY CONTAINER
            DepotEntity depot = basicProperties.owner.GetNearestDepotNotFull(transform.position);

            //REACH
            while (!basicProperties.Reached(depot))
                yield return null;
            //DEPOSIT
            while (DepositResource(depot))
                yield return new WaitForSeconds(depositSpeed);
        }

        //WHEN OVER RETURN TO BASE
        while (!basicProperties.Reached(mainBase, -mainBase.basicProperties.radius))
            yield return null;
    }

    IEnumerator Task()
    {
        while (!currentTask.Done())
        {
            while (!basicProperties.Reached(currentTask.GetTarget()))
                yield return null;
            while (!currentTask.PauseCondition(this) && currentTask.DoTask(this))
                yield return new WaitForSeconds(harvestSpeed);
        }

        OnTaskDone();

        while (!resourceContainer.IsEmpty() && BringCargo())
            yield return null;
        while (BackToBase())
            yield return null;
    }
}
