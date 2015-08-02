using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MovableEntity : Entity {
    
    //UNTIY PROPERTIES
    public bool ignoreCollisions;
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
        waypoints = Pathfinding.instance.FindPath(transform.position, target, basicProperties.radius);
    }

    public void MoveTowards(Vector3 destination)
    {
        if (ignoreCollisions)
        {
            DirectMoveTowards(destination);
            return;
        }
        else
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
                DirectMoveTowards(currentWaypoint);
            }
            else
                targetDestination = Vector3.zero;
        }
    }

    public void DirectMoveTowards(Vector3 position)
    {
        basicProperties.LookAt(position);
        transform.position = Vector3.MoveTowards(transform.position, position, speed * Time.deltaTime);
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
}