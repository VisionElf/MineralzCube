using UnityEngine;
using System.Collections;

public class BuilderEntity : ActionEntity {

    //UNITY PROPERTIES
    public int maxStock;

    //PROPERTIES

    //FUNCTIONS
    public override bool ActionCondition()
    {
        return !GetBuildingEntity().isBuilt;
    }

    public override bool DoAction()
    {
        return false;
    }

    public BuildingEntity GetBuildingEntity()
    {
        return targetEntity.buildingProperties;
    }

    public bool IsBuildingEntity()
    {
        return base.IsDoingAction();
    }
}
