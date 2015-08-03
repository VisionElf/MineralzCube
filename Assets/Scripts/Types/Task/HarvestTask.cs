using UnityEngine;
using System.Collections;

public class HarvestTask : Task {

    ResourceEntity resource;

    public HarvestTask(ResourceEntity _resource)
        : base(1)
    {
        resource = _resource;
    }

    public override Entity GetTarget()
    {
        return resource;
    }

    public override bool DoTask(WorkerEntity worker)
    {
        return worker.HarvestResource(resource);
    }

    public override bool Done()
    {
        return resource == null || resource.Empty();
    }

    public override bool PauseCondition()
    {
        foreach (WorkerEntity worker in workersList)
            paused = PauseCondition(worker);
        return paused;
    }
    public override bool PauseCondition(WorkerEntity worker)
    {
        return worker.resourceContainer.IsFull(resource.resourceType) && worker.basicProperties.owner.GetNearestDepotNotFull(worker.transform.position, resource.resourceType) == null;
    }

    public override void OnAdd()
    {
        OnUpdateAssign();
    }
    public override void OnRemove()
    {
        if (resource != null)
            resource.resourceDummy.ResetColor();
    }
    public override void OnUpdateAssign()
    {
        if (resource != null)
        {
            if (Paused())
                resource.resourceDummy.SetColor(0.5f, 0.5f, 0f);
            else if (Assigned())
                resource.resourceDummy.SetColor(0, 0.5f, 0f);
            else
                resource.resourceDummy.SetColor(0.5f, 0, 0f);

        }
    }
}
