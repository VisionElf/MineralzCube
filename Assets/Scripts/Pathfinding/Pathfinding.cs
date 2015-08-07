using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Pathfinding : MonoBehaviour {

    //PUBLIC
    public Grid grid;

    public Transform start;
    public Transform end;
    public float testRadius;

    public Vector3 lastPos;

    public bool drawGizmos;
    public bool drawPathfinding;

    void Start()
    {
        instance = this;
    }
    void Update()
    {
        Vector3 endPos = end.position;
        if (lastPos != endPos || Input.GetKeyDown(KeyCode.Space))
        {
            //grid.ResetColors();
            //grid.GridLine(grid.GetNodeAt(start.position).position, grid.GetNodeAt(end.position).position, testRadius);
            FindPath(new PathfindingParameters(start.position, end.position) { radius = testRadius });
            lastPos = endPos;
        }
    }

    static public Pathfinding instance;
    static public Queue<PathfindingRequest> requests = new Queue<PathfindingRequest>();
    static bool isProcessing;
    static public void RequestPath(PathfindingParameters parameters, Action<PathfindingResult> callback)
    {
        requests.Enqueue(new PathfindingRequest(parameters, callback));
        if (!isProcessing)
            instance.StartCoroutine("CalculPaths");
    }

    IEnumerator CalculPaths()
    {
        isProcessing = true;
        while (requests.Count > 0)
        {
            PathfindingRequest request = requests.Dequeue();
            request.callback(FindPath(request.parameters));
            yield return null;
        }
        isProcessing = false;
    }

    public PathfindingResult FindPath(PathfindingParameters parameters)
    {
        PathfindingResult result = new PathfindingResult();
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        Node startNode = grid.GetNodeAt(parameters.startPos);
        Node endNode = grid.GetNodeAt(parameters.endPos);

        Node bestNode = null;

        Heap<Node> openList = new Heap<Node>(grid.gridMaxSize);
        HashSet<Node> closedList = new HashSet<Node>();

        if (drawPathfinding)
            grid.ResetColors();

        openList.Add(startNode);
        while (openList.Count > 0)
        {
            Node currentNode = openList.RemoveFirst();
            if (currentNode == endNode || (parameters.endNodes != null && parameters.endNodes.Contains(currentNode)))
            {
                sw.Stop();
                //print(sw.ElapsedMilliseconds + "ms");
                result.path = GetPath(RetracePath(currentNode, startNode), parameters);
                return result;
            }
            if (drawPathfinding)
                currentNode.color = Color.red;
            closedList.Add(currentNode);
            foreach (Node n in grid.GetNeighbours(currentNode))
            {
                //if (!n.walkable || closedList.Contains(n))
                if (!grid.GetWalkable(n, parameters.radius) || closedList.Contains(n))
                    continue;

                int newGCost = currentNode.gCost + grid.GetDistance(n, currentNode);
                if (newGCost < n.gCost || !openList.Contains(n))
                {
                    n.gCost = newGCost;
                    n.hCost = grid.GetDistance(n, endNode);
                    n.parent = currentNode;

                    if (drawPathfinding)
                        n.color = Color.yellow;

                    if (bestNode == null || n.hCost < bestNode.hCost)
                        bestNode = n;

                    if (!openList.Contains(n))
                        openList.Add(n);
                    else
                        openList.UpdateItem(n);
                }
            }
        }
        sw.Stop();
        //print("no path found, " + sw.ElapsedMilliseconds + "ms");
        if (bestNode != null)
            result.path = GetPath(RetracePath(bestNode, startNode), parameters);
        else
            result.path = new List<Vector3>();
        return result;
    }

    List<Vector3> waypoints;
    public List<Vector3> GetPath(List<Node> path, PathfindingParameters parameters)
    {
        if (path.Count > 0)
        {
            List<Vector3> bestPath = BestSmooth(GetAllNodePositions(path), parameters.radius);
            bestPath.RemoveAt(0);
            if (grid.InBounds(parameters.endPos))
            {
                if (!grid.IsObstacle(parameters.endPos, parameters.radius))
                {
                    bestPath.RemoveAt(bestPath.Count - 1);
                    bestPath.Add(parameters.endPos);
                }
            }
            waypoints = bestPath;
            return bestPath;
        }
        return new List<Vector3>();
    }

    public List<Node> RetracePath(Node endNode, Node startNode)
    {
        List<Node> list = new List<Node>();
        Node node = endNode;
        while (node != startNode)
        {
            if (drawPathfinding)
                node.color = Color.green;
            list.Add(node);
            node = node.parent;
            if (node == null)
            {
                print("Node = null end:" + endNode + " start:" + startNode);
                return new List<Node>();
            }
        }
        list.Add(startNode);
        list.Reverse();
        return list;
    }
    public List<Vector3> GetAllNodePositions(List<Node> nodes)
    {
        List<Vector3> positions = new List<Vector3>();
        foreach (Node n in nodes)
            positions.Add(n.position);
        return positions;
    }
    public List<Vector3> BestSmooth(List<Vector3> path, float radius)
    {
        List<Vector3> newPath = new List<Vector3>();
        newPath.Add(path[0]);
        Vector3 lastPos = path[0];
        for (int i = 1; i < path.Count - 2; i++)
        {
            if (IsObstructed(lastPos, path[i + 2], radius))
            {
                newPath.Add(path[i + 1]);
                lastPos = path[i + 1];
            }
        }
        newPath.Add(path[path.Count - 1]);
        return newPath;
    }

    public bool IsObstructed(Vector3 v1, Vector3 v2, float radius)
    {
        return grid.GridLine(v1, v2, radius);
    }

    void OnDrawGizmos()
    {
        if (waypoints != null && drawGizmos)
        {
            Gizmos.DrawLine(start.transform.position + Vector3.up * 0.2f, waypoints[0] + Vector3.up * 0.2f);
            for (int i = 0; i < waypoints.Count - 1; i++)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(waypoints[i] + Vector3.up * 0.2f, waypoints[i + 1] + Vector3.up * 0.2f);
            }
        }
    }
}

public class PathfindingParameters
{
    public Vector3 startPos;
    public Vector3 endPos;
    public float radius;

    public List<Node> endNodes;

    public PathfindingParameters(Vector3 _startPos, Vector3 _endPos)
    {
        startPos = _startPos;
        endPos = _endPos;
    }
}
public class PathfindingResult
{
    public List<Vector3> path;
}
public class PathfindingRequest
{
    public PathfindingParameters parameters;
    public Action<PathfindingResult> callback;

    public PathfindingRequest(PathfindingParameters _parameters, Action<PathfindingResult> _callback)
    {
        parameters = _parameters;
        callback = _callback;
    }
}