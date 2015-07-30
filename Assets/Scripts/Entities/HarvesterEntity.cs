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

    //PROPERTIES
    int stock;
    HarvestableEntity targetEntity;

    //FUNCTION

    public void Harvest(HarvestableEntity target)
    {
        targetEntity = target;
        StartHarvesting();
    }

    void StartHarvesting()
    {
        StopCoroutine("Harvesting");
        StartCoroutine("Harvesting");
    }
    void StopHarvest()
    {
        StopCoroutine("Harvesting");
        targetEntity = null;
    }

    public bool IsHarvesting()
    {
        return targetEntity != null;
    }

    void BringCargo()
    {
        StartCoroutine("BringingCargo");
    }

    IEnumerator BringingCargo()
    {
        DepotEntity depot = new DepotEntity(); //TODO: CLOSED DEPOT
        while (!basicProperties.Reached(depot))
            yield return null;

        if (depot != null)
        {
            foreach (ResourceStock r in resourcesStock)
                r.stock -= depot.AddResource(r.resourceType, r.stock);
            yield return null;
        }

    }

    IEnumerator Harvesting()
    {
        while (!basicProperties.Reached(targetEntity, harvestRange))
        {
            yield return null;
        }
    }

}
