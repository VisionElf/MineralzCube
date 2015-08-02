﻿using UnityEngine;
using System.Collections;

public class ResourceEntity : Entity {

    //UNITY PROPERTIES
    public int maxResources;
    public int startingResources;
    public EResourceType resourceType;

    public bool removeOnEmpty;

    public Dummy resourceDummy;

    //PRIVATE PROPERTIES
    int resources;

    Entity harvester;

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
    public int AddResources(int quantity)
    {
        if (quantity + resources > maxResources)
            quantity = maxResources - resources;
        resources += quantity;
        return quantity;
    }
    public int GetRemainingResources()
    {
        return resources;
    }

    public void SetHarvester(Entity entity)
    {
        harvester = entity;
    }
    public bool IsHarvested()
    {
        return harvester != null;
    }

    public bool Empty()
    {
        return resources <= 0;
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