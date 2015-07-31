using UnityEngine;
using System.Collections;

public class Action {

    //PROPERTIES
    public float actionRadius;
    public Entity target;

    public Action(Entity _target, float _actionRadius)
    {
        actionRadius = _actionRadius;
        target = _target;
    }

    public bool DoAction() { return false; }
}
