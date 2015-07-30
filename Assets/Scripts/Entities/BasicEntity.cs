using UnityEngine;
using System.Collections;

public class BasicEntity : Entity {

    //UNITY PROPERTIES
    public float radius;

    //FUNCTION

    public bool Reached(Entity entity)
    {
        return Reached(entity, 0f);
    }
    public bool Reached(Entity entity, float range)
    {
        if (Vector3.Distance(entity.transform.position, transform.position) <= range + entity.basicProperties.radius + radius)
            return true;
        else if (IsMovable())
        {
            //BOUGER
            return false;
        }
        return false;
    }
}
