using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DepotEntity : Entity {

    //UNTIY PROPERTIES
    public bool shareResourcesStocks;
    public int maxTotalStock;
    public List<ResourceStock> resourcesStocks;

    //FUNCTIONS
    public int AddResource(EResourceType resourceType, int quantity)
    {
        ResourceStock resourceStock = GetResourceStock(resourceType);
        if (resourceStock != null)
        {
            int total = resourceStock.maxStock;
            int current = resourceStock.stock;
            if (shareResourcesStocks)
            {
                current = GetAllResourcesStock();
                total = maxTotalStock;
            }
            if (quantity + current > total)
                quantity = total - current;
            resourceStock.stock += quantity;
            return quantity;
        }
        else
            return 0;
    }
    public int RemoveResource(EResourceType resourceType, int quantity)
    {
        ResourceStock resourceStock = GetResourceStock(resourceType);
        if (resourceStock != null)
        {
            int current = resourceStock.stock;
            if (quantity > current)
                quantity = current;
            resourceStock.stock -= quantity;
            return quantity;
        }
        else
            return 0;
    }

    public ResourceStock GetResourceStock(EResourceType resourceType)
    {
        foreach (ResourceStock r in resourcesStocks)
            if (r.resourceType == resourceType)
                return r;
        return null;
    }
    public int GetAllResourcesStock()
    {
        int count = 0;
        foreach (ResourceStock r in resourcesStocks)
            count += r.stock;
        return count;
    }
}

[System.Serializable]
public class ResourceStock
{
    public EResourceType resourceType;
    public int maxStock;
    public int stock { get; set; }
}