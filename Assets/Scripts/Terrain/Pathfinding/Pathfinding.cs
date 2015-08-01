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
    public int callCount;

    void Start()
    {
        grid = GetComponent<Grid>();
        instance = this;
    }

    //bool advance;

    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.P))
            StartCoroutine(FindPath(start.position, end.position, false));

        if (Input.GetKeyDown(KeyCode.O))
            advance = true;*/
    }

    public bool PathExists(Vector3 start, Vector3 end)
    {
        return FindPath(start, end).Count > 0;
    }
    public List<Vector3> FindPath(Vector3 start, Vector3 end)
    {
        Collider temp;
        return FindPath(start, end, false, out temp);
        //return new List<Vector3>();
    }
    public List<Vector3> FindPath(Vector3 start, Vector3 end, bool ignoreStructure, out Collider structureHit)
    {
        grid.ResetColors();
        callCount++;
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

        if (startNode == endNode)
            return new List<Vector3>() { end };

        openList.Add(startNode);

        while (openList.Count > 0)
        {
            Node currentNode = openList.RemoveFirst();
            closedList.Add(currentNode);
            currentNode.color = Color.blue;

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

                    n.color = Color.magenta;
                    /*showNodes.Add(n);
                    advance = false;
                    while (displayPathfinding && !advance)
                        yield return null;*/

                    n.color = Color.yellow;

                    if (!openList.Contains(n))
                        openList.Add(n);
                    else
                        openList.UpdateItem(n);
                }
            }
            showNodes.Remove(currentNode);
            currentNode.color = Color.red;
        }

        if (path.Count > 0)
        {
            path = BestSmooth(path, startObstacle, endObstacle);
            foreach (Node n in path)
                n.color = Color.green;
        }
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

    List<Node> showNodes = new List<Node>();
    void OnGUI()
    {/*
        if (displayPathfinding)
        {
            foreach (Node n in showNodes)
            {
                UnityEditor.Handles.color = Color.red;
                UnityEditor.Handles.CubeCap(0, n.position, Quaternion.Euler(new Vector3(0, 90, 0)), 1f);

                GUIStyle style = new GUIStyle();
                style.fontSize = 8;
                UnityEditor.Handles.color = Color.white;

                string label = n.gCost + "";
                UnityEditor.Handles.Label(n.position + new Vector3(-0.5f, 0, .5f), label, style);

                label = n.hCost + "";
                UnityEditor.Handles.Label(n.position + new Vector3(-0.5f, 0, .25f), label, style);

                label = n.fCost + "";
                UnityEditor.Handles.Label(n.position + new Vector3(-0.5f, 0, 0), label, style);
            }
        }*/
    }
}
