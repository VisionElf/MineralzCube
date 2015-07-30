using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MovableEntity : Entity {
    
    //UNTIY PROPERTIES
    public float speed;

    //PROPERTIES
    Entity targetDestinationEntity;
    Queue<Waypoint> waypoints;

    void Start()
    {
        waypoints = new Queue<Waypoint>();
    }

    public void MoveToEntity(Entity entity)
    {
        StopMove();
        targetDestinationEntity = entity;
        List<Vector3> path = Pathfinding.instance.FindPath(transform.position, targetDestinationEntity.transform.position);
        for (int i = 0; i < path.Count - 1; i++)
            waypoints.Enqueue(new Waypoint(path[i]));
        waypoints.Enqueue(new Waypoint(path[path.Count - 1], targetDestinationEntity.radius));

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
    }
}

public class Waypoint
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
}