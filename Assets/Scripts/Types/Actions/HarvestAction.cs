using UnityEngine;
using System.Collections;

public class HarvestAction : Action {

    public HarvestAction(Entity target)
        : base(target)
    {

    }

    public override bool DoAction(Entity parent)
    {
        return false;
    }
}
