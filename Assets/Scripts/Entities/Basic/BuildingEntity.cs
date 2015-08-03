using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BuildingEntity : Entity {

    //UNITY PROPERTIES
    public List<ResourceCost> resourcesCost;
    public int caseSizeX;
    public int caseSizeY;

    public Texture buildingIcon;

    public Dummy buildingDummy;
    public GameObject model;
    public float sizeY;

    //PROPERTIES
    public bool isBuilt { get; set; }

    float defaultY;

    //FUNCTIONS

    public void StartBuild()
    {
        DisableCollider();
        model.transform.localPosition = new Vector3(0, -sizeY, 0);
        isBuilt = false;
        foreach (ResourceCost resourceCost in resourcesCost)
            resourceCost.buildResources = 0;

        if (HaveHealth())
            healthProperties.health = 1;
    }

    public int Build(int resources, EResourceType resourceType)
    {
        ResourceCost resourceCost = GetResourceCost(resourceType);
        if (resources > GetRemainingResources(resourceType))
            resources = GetRemainingResources(resourceType);
        resourceCost.buildResources += resources;

        if (!PathMapApplied() && resourceCost.buildResources > 0)
            ApplyPathMap();

        if (HaveHealth())
            healthProperties.AddPercentHealth((float)resources / resourceCost.cost);
        RefreshBuilding();

        if (GetBuildingProgress() >= 1)
            OnBuildFinish();
        return resources;
    }

    public void RefreshBuilding()
    {
        model.transform.localPosition = new Vector3(0, -sizeY + sizeY * GetBuildingProgress(), 0);
    }

    public void OnBuildFinish()
    {
        if (!PathMapApplied())
            ApplyPathMap();

        if (buildingDummy != null)
            buildingDummy.Hide();

        if (CanAttack())
            attackProperties.StartScan();

        isBuilt = true;
    }

    public float GetBuildingProgress()
    {
        int count = 0;
        int max = 0;
        foreach (ResourceCost resourceCost in resourcesCost)
        {
            count += resourceCost.buildResources;
            max += resourceCost.cost;
        }

        return (float)count / max;
    }

    public EResourceType GetResourceCostType()
    {
        foreach (ResourceCost cost in resourcesCost)
            if (!cost.IsFull())
                return cost.resourceType;
        return EResourceType.None;
    }
    public int GetRemainingResources(EResourceType resourceType)
    {
        ResourceCost resourceCost = GetResourceCost(resourceType);
        return resourceCost.cost - resourceCost.buildResources;
    }
    public ResourceCost GetResourceCost(EResourceType resourceType)
    {
        foreach (ResourceCost resource in resourcesCost)
            if (resource.resourceType == resourceType)
                return resource;
        return null;
    }

}