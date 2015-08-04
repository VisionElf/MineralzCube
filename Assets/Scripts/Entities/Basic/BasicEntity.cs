using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BasicEntity : Entity {

    //UNITY PROPERTIES
    public float radius;

    public Model model;

    //PROPERTIES
    Player owner;

    public Player GetOwner() { return owner; }
    public void SetOwner(Player player)
    {
        owner = player;
        foreach (PlayerColorObject obj in GetComponentsInChildren<PlayerColorObject>())
            obj.SetColor(player.playerColor);
    }

    //FUNCTION
    public bool Reached(Entity entity)
    {
        return Reached(entity, 0f);
    }
    public bool Reached(Entity entity, float additionnalRange)
    {
        if (entity == null)
        {
            print("reached: entity is null, parent is " + name);
            return true;
        }
        return Reached(entity.transform.position, entity.basicProperties.radius + additionnalRange);
    }
    public bool Reached(Vector3 destination)
    {
        return Reached(destination, 0f);
    }
    public bool Reached(Vector3 destination, float range)
    {
        float distance = Vector3.Distance(destination, transform.position);
        float distanceRange = range + radius;
        if (distance > distanceRange)
        {
            if (IsMovable())
                movableProperties.MoveTowards(destination);
            return false;
        }
        else if (IsMovable())
            movableProperties.StopMove();
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
            return movableProperties.ignoreCollisions || Pathfinding.instance.PathExists(transform.position, entity.transform.position);
        return false;
    }

    public void LookAt(Vector3 target)
    {
        Vector3 dir = (target - transform.position).normalized;

        if (dir.sqrMagnitude > 0)
        {
            Quaternion rotation = Quaternion.LookRotation(dir);
            if (model == null)
                print("[ERROR] " + name + " has no model");
            else
            {
                if (model.enableTurret)
                    model.turret.transform.rotation = rotation;
                else
                    model.transform.rotation = rotation;
            }
        }
    }
}