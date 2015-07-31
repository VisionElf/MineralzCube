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

    public override bool DoTask(Entity entity)
    {
        if (resource != null && entity != null)
        {
            WorkerEntity worker = entity.workerProperties;
            if (!worker.HarvestResource(resource))
                worker.BringCargo();
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
        if (resource != null)
            resource.resourceDummy.ResetColor();
    }
    public override void OnUpdateAssign()
    {
        if (resource != null)
            resource.resourceDummy.color = Assigned() ? new Color(0, 0.5f, 0f) : new Color(0.5f, 0, 0f);
    }
}
