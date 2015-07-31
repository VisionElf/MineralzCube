using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ResourceContainer {

    //UNTIY PROPERTY
    public bool shareResourcesStocks;
    public int maxTotalStock;
    public List<ResourceStock> resourcesStocks;


    public int dummyListStart;
    public int dummyListEnd;

    //PROPERTIES
    GameObject parent;
    List<Dummy> dummyList;
    float dummyTotalSize;

    //FUNCTIONS
    public void Initialize(GameObject _parent)
    {
        parent = _parent;
        CreateDummyList();
    }

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
            RefreshDummyList();
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
            RefreshDummyList();
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
    public int GetTotalStock()
    {
        if (shareResourcesStocks)
            return maxTotalStock;
        else
        {
            int total = 0;
            foreach (ResourceStock r in resourcesStocks)
                total += r.maxStock;
            return total;
        }
    }

    public bool IsFull()
    {
        return GetAllResourcesStock() == GetTotalStock();
    }
    public bool IsEmpty()
    {
        return GetAllResourcesStock() == 0;
    }

    public float GetPercentStock()
    {
        return (float)GetAllResourcesStock() / GetTotalStock();
    }


    //DUMMY FUNCTIONS
    public void CreateDummyList()
    {
        dummyList = new List<Dummy>();
        for (int i = dummyListStart; i <= dummyListEnd; i++)
        {
            GameObject obj = Static.FindChild(parent, "R" + i);
            if (obj != null)
            {
                Dummy dummy;
                if ((dummy = obj.GetComponent<Dummy>()) != null)
                    dummyList.Add(dummy);
            }
        }
        foreach (Dummy d in dummyList)
            dummyTotalSize += d.defaultScale.y * d.defaultScale.x * d.defaultScale.y;
        RefreshDummyList();
    }
    public void RefreshDummyList()
    {
        float percent = GetPercentStock();
        foreach (Dummy d in dummyList)
        {
            float dp = (d.defaultScale.y * d.defaultScale.x * d.defaultScale.y) / dummyTotalSize;

            d.ScaleY(Mathf.Min(percent / dp, 1));
            percent = Mathf.Max(percent - dp, 0);
        }
    }

}
