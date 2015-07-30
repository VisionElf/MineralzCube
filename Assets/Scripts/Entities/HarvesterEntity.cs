using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HarvesterEntity : Entity {

    //UNITY PROPERTIES
    public float harvestSpeed;
    public float harvestRange;
    public int harvestQuantity;

    public bool shareResourcesStock;
    public int maxStock;
    public List<ResourceStock> resourcesStock;

    public Dummy resourceDummy;
    Color resourceModelBaseColor;

    //PROPERTIES
    HarvestableEntity targetEntity;
    public MainBaseEntity mainBase { get; set; }

    //FUNCTION
    void Start()
    {
        RefreshResourceModel();
    }

    public void Harvest(HarvestableEntity target)
    {
        StopHarvesting();
        targetEntity = target;
        StartHarvesting();
    }

    public void SeekHarvest()
    {
        StopHarvesting();
        targetEntity = mainBase.GetHarvestableEntity();
        StartHarvesting();
    }

    void StartHarvesting()
    {
        StopCoroutine("Harvesting");
        StartCoroutine("Harvesting");
    }
    void StopHarvesting()
    {
        StopCoroutine("Harvesting");
        targetEntity = null;
    }

    public bool IsHarvesting()
    {
        return targetEntity != null;
    }
    public ResourceStock GetResourceStock(EResourceType resourceType)
    {
        foreach (ResourceStock r in resourcesStock)
            if (r.resourceType == resourceType)
                return r;
        return null;
    }

    public int GetAllResources()
    {
        int stock = 0;
        foreach (ResourceStock r in resourcesStock)
            stock += r.stock;
        return stock;
    }
    public int GetTotalMaxResources()
    {
        if (shareResourcesStock)
            return maxStock;
        int stock = 0;
        foreach (ResourceStock r in resourcesStock)
            stock += r.maxStock;
        return stock;
    }

    public bool IsFull()
    {
        return GetAllResources() == GetTotalMaxResources();
    }
    public float GetPercentStock()
    {
        return (float)GetAllResources() / GetTotalMaxResources();
    }

    IEnumerator Harvesting()
    {
        while (targetEntity != null)
        {
            if (!IsFull())
            {
                while (ReachTarget())
                    yield return null;
                while (HarvestTargetUntilFull())
                    yield return new WaitForSeconds(harvestSpeed);
            }

            if (targetEntity == null)
                targetEntity = mainBase.GetHarvestableEntity();

            DepotEntity depot = basicProperties.owner.GetDepot();
            if (!depot.IsFull())
            {
                while (ReachCargo(depot))
                    yield return null;
                BringCargo(depot);
                yield return null;
            }
            else
                targetEntity = null;
        }


        while (ReachBase())
            yield return null;
    }

    public bool ReachTarget()
    {
        return !basicProperties.Reached(targetEntity, harvestRange);
    }
    public bool HarvestTargetUntilFull()
    {
        if (targetEntity != null)
        {
            ResourceStock resourceStock = GetResourceStock(targetEntity.resourceType);
            int qty = targetEntity.Harvest(harvestQuantity);
            if (qty + resourceStock.stock > maxStock)
                qty = maxStock - resourceStock.stock;
            resourceStock.stock += qty;
            RefreshResourceModel();
            return resourceStock.stock < maxStock && targetEntity != null;
        }
        return false;
    }

    public void RefreshResourceModel()
    {
        if (resourceDummy != null)
            resourceDummy.ScaleY(GetPercentStock());
    }

    public bool ReachCargo(DepotEntity depot)
    {
        return !basicProperties.Reached(depot);
    }
    public bool BringCargo(DepotEntity depot)
    {
        foreach (ResourceStock r in resourcesStock)
            r.stock -= depot.AddResource(r.resourceType, r.stock);
        RefreshResourceModel();
        return true;
    }
    public bool ReachBase()
    {
        return !basicProperties.Reached(mainBase, -mainBase.basicProperties.radius);
    }


}
