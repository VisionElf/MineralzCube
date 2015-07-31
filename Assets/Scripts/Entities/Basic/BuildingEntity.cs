using UnityEngine;
using System.Collections;

public class BuildingEntity : Entity {

    //UNITY PROPERTIES
    public int cost;
    public int caseSizeX;
    public int caseSizeY;

    public Dummy buildingDummy;

    //PROPERTIES
    public bool isBuilt { get; set; }
    int buildResources;

    //FUNCTIONS
    public void StartBuild()
    {
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
        return resources;
    }
    public int GetRemainingResources()
    {
        return cost - buildResources;
    }

}
