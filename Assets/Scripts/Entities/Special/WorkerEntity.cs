using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorkerEntity : Entity {

    //UNITY PROPERTIES
    public ResourceContainer resourceContainer;

    //BUILD
    public float buildSpeed;
    public float buildRange;
    public int buildQuantity;

    //HARVEST
    public List<HarvestProperties> harvestProperties;

    public float depositSpeed;
    public int depositQuantity;

    //PROPERTIES
    public MainBaseEntity mainBase { get; set; }

    Task currentTask;

    //FUNCTIONS
    public bool BackToBase()
    {
        return !basicProperties.Reached(mainBase, -mainBase.basicProperties.radius);
    }

    public bool HarvestResource(ResourceEntity resource)
    {
        EResourceType resourceType = resource.resourceType;
        int bestQty = Mathf.Min(GetResourceHarvestProp(resourceType).harvestQuantity, resource.GetRemainingResources(), resourceContainer.GetEmptyPlace(resourceType));

        resourceContainer.AddResource(resource.resourceType, bestQty);
        resource.Harvest(bestQty);
        return !resourceContainer.IsFull(resourceType);
    }
    public bool BuildBuilding(BuildingEntity building)
    {
        foreach (ResourceStock stock in resourceContainer.resourcesStocks)
        {
            if (!stock.IsEmpty())
            {
                EResourceType resourceType = stock.resourceType;
                int bestQty = Mathf.Min(buildQuantity, building.GetRemainingResources(resourceType), resourceContainer.GetCurrentResourceStock(resourceType));

                resourceContainer.RemoveResource(resourceType, bestQty);
                building.Build(bestQty, resourceType);
                return !resourceContainer.IsEmpty(resourceType) && building.GetRemainingResources(resourceType) > 0;
            }
        }
        return false;
    }
    public bool DepositResource(DepotEntity depot, EResourceType resourceType)
    {
        int bestQty = Mathf.Min(depositQuantity, resourceContainer.GetCurrentResourceStock(resourceType), depot.resourceContainer.GetEmptyPlace(resourceType));

        resourceContainer.RemoveResource(resourceType, bestQty);
        depot.resourceContainer.AddResource(resourceType, bestQty);
        return !resourceContainer.IsEmpty(resourceType) && !depot.IsFull(resourceType);
    }
    public bool WithdrawResource(DepotEntity depot, EResourceType resourceType)
    {
        int bestQty = Mathf.Min(depositQuantity, resourceContainer.GetEmptyPlace(resourceType), depot.resourceContainer.GetCurrentResourceStock(resourceType));

        depot.resourceContainer.RemoveResource(resourceType, bestQty);
        resourceContainer.AddResource(resourceType, bestQty);
        return !resourceContainer.IsFull(resourceType) && !depot.IsEmpty(resourceType);
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

    public HarvestProperties GetResourceHarvestProp(EResourceType resourceType)
    {
        foreach (HarvestProperties prop in harvestProperties)
            if (prop.resourceType == resourceType)
                return prop;
        return null;
    }

    IEnumerator Harvest()
    {
        EResourceType currentResourceHarvested = currentTask.GetTarget().resourceProperties.resourceType;
        // HARVEST CONDITION
        while (!currentTask.Done() && !currentTask.PauseCondition())
        {
            //REACH
            while (!basicProperties.Reached(currentTask.GetTarget(), GetResourceHarvestProp(currentResourceHarvested).harvestRange))
                yield return null;
            //HARVEST RESOURCE
            while (!currentTask.Done() && currentTask.DoTask(this))
                yield return new WaitForSeconds(GetResourceHarvestProp(currentResourceHarvested).harvestSpeed);

            // EMPTY CONTAINER
            if (!resourceContainer.IsEmpty())
            {
                DepotEntity depot = basicProperties.GetOwner().GetNearestDepotNotFull(this, currentResourceHarvested);
                if (depot != null)
                {
                    //REACH
                    while (!basicProperties.Reached(depot))
                        yield return null;
                    //DEPOSIT
                    while (DepositResource(depot, currentResourceHarvested))
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
        while (currentTask != null && !currentTask.Done() && !currentTask.PauseCondition())
        {
            EResourceType currentBuildResource = currentTask.GetTarget().buildingProperties.GetAvailableResourceCostType();
            if (currentBuildResource == EResourceType.None)
                print("[ERROR] ON A MERDER QUELQUE PART");

            //FILL CONTAINER
            DepotEntity depot = basicProperties.GetOwner().GetNearestDepotNotEmpty(this, currentBuildResource);
            if (depot != null)
            {
                //REACH
                while (!basicProperties.Reached(depot))
                    yield return null;
                //DEPOSIT
                while (WithdrawResource(depot, currentBuildResource))
                    yield return new WaitForSeconds(depositSpeed);
            }

            if (!resourceContainer.IsEmpty())
            {
                //REACH
                while (currentTask != null && !basicProperties.Reached(currentTask.GetTarget(), buildRange))
                    yield return null;
                //HARVEST RESOURCE
                while (currentTask != null && !currentTask.Done() && currentTask.DoTask(this))
                    yield return new WaitForSeconds(buildSpeed);
            }

            if (!resourceContainer.IsEmpty())
            {
                // EMPTY CONTAINER
                EResourceType depositResourceType = resourceContainer.GetCurrentResourceType();
                DepotEntity depot2 = basicProperties.GetOwner().GetNearestDepotNotFull(this, depositResourceType);

                if (depot2 != null)
                {
                    //REACH
                    while (!basicProperties.Reached(depot2))
                        yield return null;
                    //DEPOSIT
                    while (DepositResource(depot2, depositResourceType))
                        yield return new WaitForSeconds(depositSpeed);
                }
            }
        }

        OnTaskDone();

        //EMPTY RESOURCE CONTAINER
        if (!resourceContainer.IsEmpty())
        {
            // EMPTY CONTAINER
            EResourceType depositResourceType = resourceContainer.GetCurrentResourceType();
            DepotEntity depot2 = basicProperties.GetOwner().GetNearestDepotNotFull(this, depositResourceType);

            if (depot2 != null)
            {
                //REACH
                while (!basicProperties.Reached(depot2))
                    yield return null;
                //DEPOSIT
                while (DepositResource(depot2, depositResourceType))
                    yield return new WaitForSeconds(depositSpeed);
            }
        }

        //WHEN OVER RETURN TO BASE
        while (mainBase != null && !basicProperties.Reached(mainBase, -mainBase.basicProperties.radius))
            yield return null;
    }
}

[System.Serializable]
public class HarvestProperties
{
    public EResourceType resourceType;
    public int harvestQuantity;
    public float harvestSpeed;
    public float harvestRange;
}