using UnityEngine;
using System.Collections;

public class BuildingEntity : Entity {

    //UNITY PROPERTIES
    public int cost;
    public int caseSizeX;
    public int caseSizeY;

    public void Build(int percent)
    {
        if (HaveHealth())
            healthProperties.AddPercentHealth(percent);
    }
}
