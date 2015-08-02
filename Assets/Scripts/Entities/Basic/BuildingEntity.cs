﻿using UnityEngine;
using System.Collections;

public class BuildingEntity : Entity {

    //UNITY PROPERTIES
    public int cost;
    public int caseSizeX;
    public int caseSizeY;

    public Dummy buildingDummy;
    public GameObject model;
    public float sizeY;

    //PROPERTIES
    public bool isBuilt { get; set; }
    int buildResources;

    float defaultY;

    //FUNCTIONS
    void Start()
    {
        ApplyPathMap();
    }

    public void StartBuild()
    {
        model.transform.localPosition = new Vector3(0, -sizeY, 0);
        isBuilt = false;
        buildResources = 0;
        if (HaveHealth())
            healthProperties.health = 1;
    }

    public int Build(int resources)
    {
        if (resources > GetRemainingResources())
            resources = GetRemainingResources();
        buildResources += resources;

        if (HaveHealth())
            healthProperties.AddPercentHealth((float)resources / cost);
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
        if (buildingDummy != null)
            buildingDummy.Hide();
        isBuilt = true;
    }

    public float GetBuildingProgress()
    {
        return (float)buildResources / cost;
    }

    public int GetRemainingResources()
    {
        return cost - buildResources;
    }

}