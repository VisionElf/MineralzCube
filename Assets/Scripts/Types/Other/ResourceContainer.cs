using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//[System.Serializable]
public class ResourceContainer : MonoBehaviour {

    //UNTIY PROPERTY
    public List<ResourceStock> resourcesStocks;


    //PROPERTY
    Entity parent;

    //FUNCTIONS
    void Start()
    {
        CreateDummyList();
        parent = transform.root.GetComponent<Entity>();
    }

    public int AddResource(EResourceType resourceType, int quantity)
    {
        ResourceStock resourceStock = GetResourceStock(resourceType);
        if (resourceStock != null)
        {
            int total = resourceStock.maxStock;
            int current = resourceStock.stock;
            if (quantity + current > total)
                quantity = total - current;
            resourceStock.stock += quantity;
            resourceStock.RefreshDummyList();
            if (parent != null && parent.HaveDepot())
                parent.depotProperties.OnResouresChanged();
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
            resourceStock.RefreshDummyList();
            /*if (parent.HaveDepot())
                parent.depotProperties.OnResouresChanged();*/
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

    public EResourceType GetCurrentResourceType()
    {
        foreach (ResourceStock stock in resourcesStocks)
            if (!stock.IsEmpty())
                return stock.resourceType;
        return EResourceType.None;
    }
    public int GetCurrentResourceStock(EResourceType resourceType)
    {
        ResourceStock resource = GetResourceStock(resourceType);
        if (resource != null)
            return resource.stock;
        return 0;
    }

    public int GetMaxStock(EResourceType resourceType)
    {
        ResourceStock resource = GetResourceStock(resourceType);
        if (resource != null)
            return resource.maxStock;
        return 0;
    }

    public int GetEmptyPlace(EResourceType resourceType)
    {
        ResourceStock resource = GetResourceStock(resourceType);
        return resource.maxStock - resource.stock;
    }

    public bool IsFull(EResourceType resourceType)
    {
        ResourceStock resource = GetResourceStock(resourceType);
        if (resource != null)
            return resource.stock == resource.maxStock;
        return true;
    }

    public bool IsEmpty()
    {
        int count = 0;
        foreach (ResourceStock stock in resourcesStocks)
            count += stock.stock;
        return count == 0;
    }
    public bool IsEmpty(EResourceType resourceType)
    {
        ResourceStock resource = GetResourceStock(resourceType);
        if (resource != null)
            return resource.stock == 0;
        return true;
    }

    public float GetPercentStock(EResourceType resourceType)
    {
        ResourceStock resource = GetResourceStock(resourceType);
        if (resource != null)
            return (float)resource.stock / resource.maxStock;
        return 0f;
    }

    void CreateDummyList()
    {
        foreach (ResourceStock stock in resourcesStocks)
            stock.CreateDummyList();
    }
}
