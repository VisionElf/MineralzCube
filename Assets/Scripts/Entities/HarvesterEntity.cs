using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HarvesterEntity : ActionEntity {

    //UNITY PROPERTIES
    public ResourceContainer resourceContainer;

    //PROPERTIES
    public MainBaseEntity mainBase { get; set; }

    //FUNCTION
    void Start()
    {
        resourceContainer.Initialize(gameObject);
    }

    public override bool SetEntity(Entity target)
    {
        targetEntity = target;
        if (targetEntity != null)
            GetHarvestableEntity().SetHarvester(this);
        return targetEntity != null;
    }

    public HarvestableEntity GetHarvestableEntity()
    {
        return targetEntity.harvestableProperties;
    }

    public void SeekHarvest()
    {
        StopAction();
        if (SetEntity(mainBase.GetHarvestableEntity()))
            StartAction();
    }

    public override void OnStartAction()
    {
        base.OnStartAction();
    }
    public override void OnStopAction()
    {
        base.OnStopAction();
        if (targetEntity != null)
            GetHarvestableEntity().SetHarvester(null);
        targetEntity = null;
    }

    public bool IsHarvesting()
    {
        return base.IsDoingAction();
    }
    
    public override bool ActionCondition()
    {
        return targetEntity != null && !resourceContainer.IsFull();
    }

    public override bool DoAction()
    {
        if (targetEntity != null)
        {
            resourceContainer.AddResource(GetHarvestableEntity().resourceType, GetHarvestableEntity().Harvest(Mathf.RoundToInt(actionQuantity)));
            return !resourceContainer.IsFull() && targetEntity != null;
        }
        return false;
    }
    public override void OnActionDone()
    {
        base.OnActionDone();
        StopCoroutine("Cargo");
        StartCoroutine("Cargo");
    }

    IEnumerator Cargo()
    {
        DepotEntity depot = basicProperties.owner.GetDepot();
        if (!depot.resourceContainer.IsFull())
        {
            while (ReachCargo(depot))
                yield return null;
            BringCargo(depot);
            yield return null;
        }
        else
            SetEntity(null);

        if (targetEntity != null && GetHarvestableEntity().GetPercentResources() > 0)
        {
            ActionOnEntity(targetEntity);
        }
        else
        {
            while (ReachBase())
                yield return null;
        }

    }

    public bool ReachCargo(DepotEntity depot)
    {
        return !basicProperties.Reached(depot);
    }
    public bool BringCargo(DepotEntity depot)
    {
        foreach (ResourceStock r in resourceContainer.resourcesStocks)
            resourceContainer.RemoveResource(r.resourceType, depot.resourceContainer.AddResource(r.resourceType, r.stock));
        return true;
    }
    public bool ReachBase()
    {
        return !basicProperties.Reached(mainBase, -mainBase.basicProperties.radius);
    }


}
