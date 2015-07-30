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

    public float GetPercentStock()
    {
        return (float)GetAllResourcesStock() / GetTotalStock();
    }

    //DRAW
    void OnGUI()
    {
        if (GetPercentStock() > 0)
        {
            int barWidth = 100;
            int barHeight = 10;
            int barUp = 5;
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

[System.Serializable]
public class ResourceStock
{
    public EResourceType resourceType;
    public int maxStock;
    public int stock { get; set; }
}