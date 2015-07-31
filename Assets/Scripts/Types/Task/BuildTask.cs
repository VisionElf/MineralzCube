using UnityEngine;
using System.Collections;

public class BuildTask : Task {

    BuildingEntity building;

    public BuildTask(BuildingEntity _building)
        : base(_building.caseSizeX * _building.caseSizeY)
    {
        building = _building;
    }

    public override Entity GetTarget()
    {
        return building;
    }

    public override bool DoTask(Entity entity)
    {
        if (building != null && !building.isBuilt)
        {
            WorkerEntity worker = entity.workerProperties;
            if (!worker.BuildBuilding(building))
                worker.GatherCargo(building);
            return true;
        }
        return false;
    }


    public override void OnAdd()
    {
        OnUpdateAssign();
    }
    public override void OnRemove()
    {
        if (building != null)
            building.buildingDummy.ResetColor();
    }
    public override void OnUpdateAssign()
    {
        if (building != null)
            building.buildingDummy.color = Assigned() ? new Color(0, 0.5f, 0f) : new Color(0.5f, 0, 0f);
    }
}
