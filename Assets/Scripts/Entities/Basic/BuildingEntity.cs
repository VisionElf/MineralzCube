using UnityEngine;
using System.Collections;

public class BuildingEntity : Entity {

    //UNITY PROPERTIES
    public int cost;
    public int caseSizeX;
    public int caseSizeY;

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

    public void Build(float percent)
    {
        buildResources += (int)(percent * cost);
    }

}
