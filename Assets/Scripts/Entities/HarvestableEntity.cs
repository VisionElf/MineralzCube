using UnityEngine;
using System.Collections;

public class HarvestableEntity : Entity {

    //UNITY PROPERTIES
    public int maxResources;
    public int startingResources;
    public EResourceType resourceType;

    public bool removeOnEmpty;

    public Dummy resourceDummy;

    //PRIVATE PROPERTIES
    int resources;

    //UNITY FUNCTIONS
    void Start()
    {
        if (startingResources < 0 || startingResources > maxResources)
            resources = maxResources;
        else
            resources = startingResources;
    }

    //FUNCTIONS
    public int Harvest(int quantity)
    {
        if (quantity > resources)
            quantity = resources;
        resources -= quantity;
        RefreshResourceModel();
        if (removeOnEmpty && resources == 0)
            RemoveObject();
        return quantity;
    }
    public void RefreshResourceModel()
    {
        if (resourceDummy != null)
            resourceDummy.ScaleY(GetPercentResources());
    }

    public float GetPercentResources()
    {
        return (float)resources / maxResources;
    }
}

public enum EResourceType
{
    Rock, Mineral
}