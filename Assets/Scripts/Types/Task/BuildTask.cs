using UnityEngine;
using System.Collections;

public class BuildTask : Task {

    BuildingEntity building;

    public BuildTask(BuildingEntity _building)
        : base(1)//_building.caseSizeX * _building.caseSizeY)
    {
        building = _building;
    }

    public override Entity GetTarget()
    {
        return building;
    }


    public override bool DoTask(WorkerEntity worker)
    {
        if (!worker.BuildBuilding(building))
            if (!worker.GatherCargo(building))
                return !building.isBuilt;
        return true;
    }

    public override bool PauseCondition(WorkerEntity worker)
    {
        paused = worker.resourceContainer.IsEmpty() && worker.basicProperties.owner.GetAvailableResources() == 0;
        return paused;
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
        {
            if (Paused())
                building.buildingDummy.SetColor(0.5f, 0.5f, 0f);
            else if (Assigned())
                building.buildingDummy.SetColor(0, 0.5f, 0f);
            else
                building.buildingDummy.SetColor(0.5f, 0, 0f);
        }
    }
}
