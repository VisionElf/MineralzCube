using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BasicEntity : Entity {

    //UNITY PROPERTIES
    public float radius;

    //PROPERTIES
    public Player owner { get; set; }
    Queue<Action> actions;
    Action currentAction;

    //ACTION FUNCTION
    void Start()
    {
        actions = new Queue<Action>();
    }
    public void AddAction(Action action)
    {
        actions.Enqueue(action);
    }
    public void ClearActions()
    {
        actions.Clear();
    }

    IEnumerator DoActions()
    {
        while (actions.Count > 0)
        {
            if (currentAction == null)
                currentAction = actions.Dequeue();

            while (!Reached(currentAction.target))
                yield return null;

            while (currentAction.DoAction(this))
                yield return null;
        }
    }

    //FUNCTION
    public bool Reached(Entity entity)
    {
        return Reached(entity, 0f);
    }
    public bool Reached(Entity entity, float range)
    {
        if (entity == null)
        {
            print("reached: entity is null, parent is " + name);
            return false;
        }
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
        if (entity == null)
            return false;
        if (Vector3.Distance(entity.transform.position, transform.position) <= range + entity.basicProperties.radius + radius)
            return true;
        else if (IsMovable())
            return Pathfinding.instance.PathExists(transform.position, entity.transform.position);
        return false;
    }

    public void LookAt(Vector3 target)
    {
        Vector3 dir = (target - transform.position).normalized;
        if (dir.sqrMagnitude > 0)
            transform.rotation = Quaternion.LookRotation(dir);
    }
}