using UnityEngine;
using System.Collections;

public class Resource : Entity {

    public int maxResources;
    public int startingResources;
    
    int resources;

    void Start()
    {
        resources = startingResources;
    }

    public int Harvest(int quantity)
    {
        if (quantity > resources)
            quantity = resources;
        resources -= quantity;
        RefreshModel();
        return quantity;
    }

    public bool IsEmpty()
    {
        return resources == 0;
    }
    public float GetPercentResources()
    {
        return (float)resources / maxResources;
    }

    public void RefreshModel()
    {
        model.transform.localPosition = new Vector3(0, (1f - GetPercentResources()) * -model.maxHeight, 0);
    }
}
