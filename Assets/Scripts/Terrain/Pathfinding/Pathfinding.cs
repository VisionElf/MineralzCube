using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class Pathfinding : MonoBehaviour
{
    Grid grid;

    public Transform start;
    public Transform end;

    public bool displayPathfinding;
    public bool displaySmoothing;

    static public Pathfinding instance;

    void Start()
    {
        grid = GetComponent<Grid>();
        instance = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            FindPath(start.position, end.position);
    }

    public List<Vector3> FindPath(Vector3 start, Vector3 end)
    {
        Collider temp;
        return FindPath(start, end, false, out temp);
    }
    public List<Vector3> FindPath(Vector3 start, Vector3 end, bool ignoreStructure, out Collider structureHit)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();

        Node startNode = grid.GetNodeAt(start);
        Node endNode = grid.GetNodeAt(end);

        List<Node> path = new List<Node>();

        structureHit = null;
        RaycastHit hitInfo;
        Collider startObstacle = null;
        Collider endObstacle = null;
        if (Physics.Raycast(start + new Vector3(0, 5f, 0), Vector3.down, out hitInfo, 10f, grid.mask))
            startObstacle = hitInfo.collider;
        if (Physics.Raycast(end + new Vector3(0, 5f, 0), Vector3.down, out hitInfo, 10f, grid.mask))
            endObstacle = hitInfo.collider;

        Heap<Node> openList = new Heap<Node>(grid.gridSize);
        HashSet<Node> closedList = new HashSet<Node>();

        openList.Add(startNode);

        while (openList.Count > 0)
        {
            Node currentNode = openList.RemoveFirst();
            closedList.Add(currentNode);
            if (currentNode == endNode)
            {
                path = RetracePath(endNode, startNode);
                if (ignoreStructure)
                {
                    foreach (Node n in path)
                    {
                        if (n.obstacle != null)
                        {
                            structureHit = n.obstacle;
                            break;
                        }
                    }
                }
                break;
            }

            foreach (Node n in grid.GetNeighbours(currentNode))
            {
                if (closedList.Contains(n))
                    continue;

                if (!n.walkable)
                {
                    if (n.obstacle != startObstacle && n.obstacle != endObstacle)
                        if (!ignoreStructure || n.obstacle.tag != "Building")
                            continue;
                }

                float newGScore = currentNode.gCost + grid.GetDistance(currentNode, n);
                if (newGScore < n.gCost || !openList.Contains(n))
                {
                    n.gCost = newGScore;
                    n.hCost = grid.GetDistance(n, endNode);
                    n.parent = currentNode;

                    if (!openList.Contains(n))
                        openList.Add(n);
                    else
                        openList.UpdateItem(n);
                }
            }
        }

        if (path.Count > 0)
            path = BestSmooth(path, startObstacle, endObstacle);
        return NodeListToWaypoints(path);
    }

    public List<Node> RetracePath(Node end, Node start)
    {
        List<Node> path = new List<Node>();
        Node node = end;
        while (node != start)
        {
            path.Add(node);
            node = node.parent;
        }
        path.Add(start);

        path.Reverse();
        return path;
    }

    public List<Node> SmoothPath(List<Node> path)
    {
        List<Node> list = new List<Node>();
        Node lastNode = path[0];
        list.Add(lastNode);
        RaycastHit hitInfo;
        for (int i = 1; i < path.Count; i++)
        {
            Node p = path[i - 1];
            Node n = path[i];
            if (lastNode != null)
                if (!Physics.Raycast(lastNode.position + Vector3.up, (n.position - lastNode.position).normalized, out hitInfo, Vector3.Distance(n.position, lastNode.position), grid.mask))
                    continue;
            list.Add(p);
            lastNode = p;
        }
        Node endNode = path[path.Count - 1];
        if (!list.Contains(endNode))
            list.Add(endNode);
        return list;
    }

    public List<Vector3> NodeListToWaypoints(List<Node> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        foreach (Node n in path)
            waypoints.Add(n.position);
        return waypoints;
    }

    public List<Node> BestSmooth(List<Node> path, Collider startCol, Collider endCol)
    {
        List<Node> list = new List<Node>();
        Node lastNode = path[0];
        int lastIndex = 0;
        Node endNode = path[path.Count - 1];
        list.Add(lastNode);

        Node bestNode = lastNode;
        RaycastHit hitInfo;
        while (lastNode != endNode)
        {
            bestNode = null;
            for (int i = lastIndex + 1; i < path.Count; i++)
            {
                Node n = path[i];
                if (!Physics.Raycast(lastNode.position + Vector3.up, (n.position - lastNode.position).normalized, out hitInfo, Vector3.Distance(n.position, lastNode.position), grid.mask)
                    || (hitInfo.collider == startCol || hitInfo.collider == endCol))
                {
                    bestNode = n;
                    lastIndex = i;
                }
            }
            if (bestNode == null)
                bestNode = endNode;
            list.Add(bestNode);
            lastNode = bestNode;
        }
        return list;
    }
}
