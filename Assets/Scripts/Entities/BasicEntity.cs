using UnityEngine;
using System.Collections;

public class BasicEntity : Entity {

    //UNITY PROPERTIES
    public float radius;

    //PROPERTIES
    public Player owner { get; set; }

    //FUNCTION

    public bool Reached(Entity entity)
    {
        return Reached(entity, 0f);
    }
    public bool Reached(Entity entity, float range)
    {
        float distance = Vector3.Distance(entity.transform.position, transform.position);
        float distanceRange = range + entity.basicProperties.radius + radius;
        if (distance > distanceRange)
        {
            if (IsMovable())
                movableProperties.MoveTowards(entity.transform.position);
            return false;
        }
        return distance <= distanceRange;
    }

    public bool CanReach(Entity entity)
    {
        return CanReach(entity, 0f);
    }
    public bool CanReach(Entity entity, float range)
    {
        if (Vector3.Distance(entity.transform.position, transform.position) <= range + entity.basicProperties.radius + radius)
            return true;
        else
            return IsMovable();
    }

    public void LookAt(Vector3 target)
    {
        Vector3 dir = (target - transform.position).normalized;
        if (dir.sqrMagnitude > 0)
            transform.rotation = Quaternion.LookRotation(dir);
    }
}