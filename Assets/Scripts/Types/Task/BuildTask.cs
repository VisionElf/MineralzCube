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
        return worker.BuildBuilding(building);
    }

    public override bool Done()
    {
        return building.isBuilt;
    }

    public override bool PauseCondition()
    {
        int workerPaused = 0;
        foreach (WorkerEntity worker in workersList)
            if (PauseCondition(worker))
                workerPaused++;
        paused = (workerPaused == workersList.Count);
        return paused;
    }

    public override bool PauseCondition(WorkerEntity worker)
    {
        int count = 0;
        bool empty = true;
        foreach (ResourceCost cost in building.resourcesCost)
            if (!cost.IsFull())
            {
                count += worker.basicProperties.owner.GetAvailableResources(cost.resourceType);
                empty = empty && worker.resourceContainer.IsEmpty(cost.resourceType);
            }

        return empty && count == 0;
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
