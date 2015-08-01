using UnityEngine;
using System.Collections;

public class BuildingEntity : Entity {

    //UNITY PROPERTIES
    public int cost;
    public int caseSizeX;
    public int caseSizeY;

    public Dummy buildingDummy;

    public GameObject GetModel()
    {
        return GetComponentInChildren<Renderer>().gameObject;
    }

    //PROPERTIES
    public bool isBuilt { get; set; }
    int buildResources;

    float defaultY;

    //FUNCTIONS
    public void StartBuild()
    {
        defaultY = GetModel().transform.localPosition.y;
        GetModel().transform.localPosition = new Vector3(0, -defaultY, 0);
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
        healthProperties.AddPercentHealth((float)resources / cost);
        RefreshBuilding();

        if (GetBuildingProgress() >= 1)
            OnBuildFinish();
        return resources;
    }

    public void RefreshBuilding()
    {
        GetModel().transform.localPosition = new Vector3(0, -defaultY + (defaultY * 2) * GetBuildingProgress(), 0);
    }

    public void OnBuildFinish()
    {
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
