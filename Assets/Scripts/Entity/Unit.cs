using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Unit : Entity {


    public float movementSpeed;

    protected bool isMoving;

    public void OrderMoveTo(Vector3 position)
    {
        StopMove();
        MoveTo(position);
    }
    public void OrderMoveTo(Entity entity)
    {
        StopMove();
        MoveTo(entity);
    }

    public void MoveTo(Vector3 position)
    {
        isMoving = true;
        Pathfinding.RequestPath(new PathfindingParameters(transform.position, position) { radius = radius }, OnPathFound);
    }
    public void MoveTo(Entity entity)
    {
        isMoving = true;
        Pathfinding.RequestPath(new PathfindingParameters(transform.position, entity.transform.position) {
            radius = radius, endNodes = Grid.instance.GetNodesInSquare(entity.transform.position, entity.nodeWidth + 2, entity.nodeHeight + 2)
        }, OnPathFound);
    }

    public void StopMove()
    {
        StopCoroutine("Move");
        isMoving = false;
    }

    public void OnPathFound(PathfindingResult result)
    {
        StopCoroutine("Move");
        StartCoroutine("Move", result.path);
    }

    protected IEnumerator Move(List<Vector3> waypoints)
    {
        int currentIndex = 0;
        while (currentIndex < waypoints.Count)
        {
            while (Vector3.Distance(waypoints[currentIndex], transform.position) > 0)
            {
                transform.position = Vector3.MoveTowards(transform.position, waypoints[currentIndex], movementSpeed * Time.deltaTime);
                LookAt(waypoints[currentIndex]);
                yield return null; 
            }
            currentIndex++;
        }
        isMoving = false;
    }
}
