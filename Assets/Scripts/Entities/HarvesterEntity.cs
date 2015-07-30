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
    HarvestableEntity targetEntity;
    public MainBaseEntity mainBase { get; set; }

    //FUNCTION

    public void Harvest(HarvestableEntity target)
    {
        StopHarvesting();
        targetEntity = target;
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
            while (ReachTarget())
                yield return null;
            while (HarvestTargetUntilFull())
                yield return new WaitForSeconds(harvestSpeed);

            DepotEntity depot = basicProperties.owner.GetDepot();
            while (ReachCargo(depot))
                yield return null;
            BringCargo(depot);
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
            return resourceStock.stock < maxStock && targetEntity != null;
        }
        return false;
    }

    public bool ReachCargo(DepotEntity depot)
    {
        return !basicProperties.Reached(depot);
    }
    public bool BringCargo(DepotEntity depot)
    {
        foreach (ResourceStock r in resourcesStock)
            r.stock -= depot.AddResource(r.resourceType, r.stock);
        return true;
    }
    public bool ReachBase()
    {
        return !basicProperties.Reached(mainBase, -mainBase.basicProperties.radius);
    }


    IEnumerator Harvesting2()
    {
        while (targetEntity != null)
        {
            DepotEntity depot = basicProperties.owner.GetDepot();
            while (!basicProperties.Reached(depot))
                yield return null;

            if (depot != null)
            {
                foreach (ResourceStock r in resourcesStock)
                    r.stock -= depot.AddResource(r.resourceType, r.stock);
                yield return null;
            }

            if (targetEntity != null || targetEntity.GetPercentResources() > 0)
            {
                while (!basicProperties.Reached(targetEntity, harvestRange))
                    yield return null;

                ResourceStock resourceStock = GetResourceStock(targetEntity.resourceType);
                while (resourceStock.stock < maxStock && targetEntity != null)
                {
                    int qty = targetEntity.Harvest(harvestQuantity);
                    if (qty + resourceStock.stock > maxStock)
                        qty = maxStock - resourceStock.stock;
                    resourceStock.stock += qty;
                    yield return new WaitForSeconds(harvestSpeed);
                }
            }
            else
            {
                while (!basicProperties.Reached(depot, -depot.basicProperties.radius))
                    yield return null;
            }
        }


    }

    void OnGUI()
    {
        if (GetPercentStock() > 0)
        {
            int barWidth = 30;
            int barHeight = 8;
            int barUp = 2;
            int barSize = 2;

            Vector3 pos = basicProperties.owner.playerCamera.WorldToScreenPoint(transform.position + Vector3.forward * basicProperties.radius);
            pos.y = Screen.height - pos.y;

            Rect fullRect = new Rect(pos.x - barWidth / 2, pos.y - barHeight - barUp, barWidth, barHeight);
            Rect rect = new Rect(fullRect.x + barSize, fullRect.y + barSize, Mathf.Max(0, GetPercentStock() * fullRect.width - barSize * 2), fullRect.height - barSize * 2);

            GUI.color = Color.black;
            GUI.DrawTexture(fullRect, Static.basic_texture);
            float g = 0.2f + GetPercentStock() * 0.5f;
            GUI.color = new Color(g, g, g);
            GUI.DrawTexture(rect, Static.basic_texture);
        }
    }

}
