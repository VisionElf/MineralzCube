using UnityEngine;
using System.Collections;

public class HarvestableEntity : Entity {

    //UNITY PROPERTIES
    public int maxResources;
    public int startingResources;
    public EResourceType resourceType;

    public bool removeOnEmpty;

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
        if (removeOnEmpty && resources == 0)
            RemoveObject();
        return quantity;
    }
    public float GetPercentResources()
    {
        return resources / maxResources;
    }
}

public enum EResourceType
{
    Rock, Mineral
}