using UnityEngine;
using System.Collections;

public class Action {

    //PROPERTIES
    public Entity target;

    public Action(Entity _target)
    {
        target = _target;
    }

    public virtual bool DoAction(Entity parent) { return false; }
}
