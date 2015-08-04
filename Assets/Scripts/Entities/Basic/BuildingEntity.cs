using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BuildingEntity : Entity {

    //UNITY PROPERTIES
    public List<ResourceCost> resourcesCost;
    public int caseSizeX;
    public int caseSizeY;


    public Dummy buildingDummy;
    public float sizeY;

    //PROPERTIES
    public bool isBuilt { get; set; }

    float defaultY;

    //FUNCTIONS

    public void StartBuild()
    {
        RemovePathMap();
        basicProperties.model.transform.localPosition = new Vector3(0, -sizeY, 0);
        isBuilt = false;
        foreach (ResourceCost resourceCost in resourcesCost)
            resourceCost.buildResources = 0;

        if (HasHealth())
            healthProperties.health = 1;
    }

    public int Build(int resources, EResourceType resourceType)
    {
        ResourceCost resourceCost = GetResourceCost(resourceType);
        if (resources > GetRemainingResources(resourceType))
            resources = GetRemainingResources(resourceType);
        resourceCost.buildResources += resources;

        if (resourceCost.buildResources > 0)
            ApplyPathMap();

        if (HasHealth())
            healthProperties.AddPercentHealth((float)resources / resourceCost.cost);
        RefreshBuilding();

        if (GetBuildingProgress() >= 1)
            OnBuildFinish();
        return resources;
    }

    public void RefreshBuilding()
    {
        basicProperties.model.transform.localPosition = new Vector3(0, -sizeY + sizeY * GetBuildingProgress(), 0);
    }

    public void OnBuildFinish()
    {
        basicProperties.model.transform.localPosition = new Vector3(0, 0, 0);
        pathMapApplied = false;
        ApplyPathMap();

        if (buildingDummy != null)
            buildingDummy.Hide();

        if (CanAttack())
            attackProperties.StartScan();
        else if (CanGenerate())
            generatorProperties.StartScan();

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

    public bool CanBeBuildAtPosition(Vector3 position)
    {
        Vector3 pos = position - new Vector3(caseSizeX, 0, caseSizeY) * Grid.instance.nodeSize / 4 + Vector3.up * 10f;
        for (int i = 0; i < caseSizeX; i++)
            for (int j = 0; j < caseSizeY; j++)
                if (Physics.Raycast(pos + new Vector3(i, 0, j) * Grid.instance.nodeSize, Vector3.down, 20f, Grid.instance.mask))
                    return false;
        return true;
    }

    public EResourceType GetAvailableResourceCostType()
    {
        foreach (ResourceCost cost in resourcesCost)
            if (!cost.IsFull() && basicProperties.GetOwner().GetAvailableResources(cost.resourceType) > 0)
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
