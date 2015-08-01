using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MovableEntity : Entity {
    
    //UNTIY PROPERTIES
    public float speed;

    //PROPERTIES
    Vector3 targetDestination;
    List<Vector3> waypoints;

    void Start()
    {
        waypoints = new List<Vector3>();
    }

    public void FindWaypoints(Vector3 target)
    {
        targetDestination = target;
        waypoints = Pathfinding.instance.FindPath(transform.position, target);
    }

    public void MoveTowards(Vector3 destination)
    {
        if (destination == transform.position)
            return;
        if (targetDestination != destination)
            FindWaypoints(destination);

        if (waypoints.Count > 0)
        {
            Vector3 currentWaypoint = waypoints[0];
            float distance = Vector3.Distance(transform.position, currentWaypoint);
            if (distance <= 0)
            {
                waypoints.Remove(currentWaypoint);
                MoveTowards(destination);
                return;
            }
            basicProperties.LookAt(currentWaypoint);
            transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, speed * Time.deltaTime);
        }
        else
            targetDestination = Vector3.zero;
    }

    void OnDrawGizmos()
    {
        if (waypoints != null && waypoints.Count > 0)
        {
            float size = 0.5f;
            Gizmos.color = Color.black;
            Gizmos.DrawLine(transform.position + Vector3.up * size / 2, waypoints[0] + Vector3.up * size / 2);
            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                Gizmos.DrawCube(waypoints[i] + Vector3.up * size / 2, Vector3.one * size);
                Gizmos.DrawLine(waypoints[i] + Vector3.up * size / 2, waypoints[i + 1] + Vector3.up * size / 2);
            }
            Gizmos.DrawCube(waypoints[waypoints.Count - 1] + Vector3.up * size / 2, Vector3.one * size);
        }
    }

    /*public void MoveToEntity(Entity entity)
    {
        StopMove();
        targetDestinationEntity = entity;
        List<Vector3> path = Pathfinding.instance.FindPath(transform.position, targetDestinationEntity.transform.position);
        for (int i = 0; i < path.Count - 1; i++)
            waypoints.Enqueue(new Waypoint(path[i]));
        waypoints.Enqueue(new Waypoint(path[path.Count - 1], targetDestinationEntity.basicProperties.radius));

        StartCoroutine("Move");
    }
    public void MoveToRangeEntity(Entity entity, float range)
    {
        StopMove();
        targetDestinationEntity = entity;
        List<Vector3> path = Pathfinding.instance.FindPath(transform.position, targetDestinationEntity.transform.position);
        Vector3 entityPosition = targetDestinationEntity.transform.position;
        for (int i = 0; i < path.Count - 1; i++)
        {
            if (Vector3.Distance(entityPosition, path[i]) > range)
                waypoints.Enqueue(new Waypoint(path[i]));
        }

        StartCoroutine("Move");
    }
    public void StopMove()
    {
        StopCoroutine("Move");
        targetDestinationEntity = null;
        if (waypoints.Count > 0)
            waypoints.Clear();
    }

    IEnumerator Move()
    {
        while (waypoints.Count > 0)
        {
            Waypoint currentWaypoint = waypoints.Dequeue();
            float distance = Vector3.Distance(transform.position, currentWaypoint.position);
            while (distance > currentWaypoint.CalculateRadius(distance))
            {
                transform.position = Vector3.MoveTowards(transform.position, currentWaypoint.position, speed * Time.deltaTime);
                distance = Vector3.Distance(transform.position, targetDestinationEntity.transform.position);
                yield return null;
            }
        }
    }*/
}

/*public class Waypoint
{
    public Vector3 position { get; set; }
    float radius;
    bool useRadius;
    public Waypoint(Vector3 _position)
    {
        position = _position;
        useRadius = false;
    }
    public Waypoint(Vector3 _position, float _radius)
    {
        position = _position;
        useRadius = true;
        radius = _radius;
    }
    public float CalculateRadius(float entityRadius)
    {
        if (useRadius)
            return 0f;
        else
            return entityRadius + radius;
    }
}*/