﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class Pathfinding : MonoBehaviour
{
    Grid grid;

    public Transform start;
    public Transform end;

    public bool displayPathfinding;

    void Start()
    {
        grid = GetComponent<Grid>();
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            StartCoroutine(FindPath(start.position, end.position));
    }

    public IEnumerator FindPath(Vector3 start, Vector3 end)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        grid.ResetColors();

        Node startNode = grid.GetNodeAt(start);
        Node endNode = grid.GetNodeAt(end);

        startNode.color = Color.yellow;
        endNode.color = Color.blue;

        List<Node> path = new List<Node>();

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
                break;
            }

            currentNode.color = Color.blue;

            foreach (Node n in grid.GetNeighbours(currentNode, 1))
            {
                if (!n.walkable || closedList.Contains(n))
                    continue;

                float newGScore = currentNode.gCost + grid.GetDistance(currentNode, n);
                if (newGScore < n.gCost || !openList.Contains(n))
                {
                    n.gCost = newGScore;
                    n.hCost = grid.GetDistance(n, endNode);
                    n.parent = currentNode;

                    n.color = Color.yellow;
                    if (!openList.Contains(n))
                        openList.Add(n);
                    else
                        openList.UpdateItem(n);
                }
            }
            if (displayPathfinding)
                yield return null;

            currentNode.color = Color.red;
            if (displayPathfinding)
                yield return null;
        }

        if (path.Count > 0)
        {
            foreach (Node n in path)
                n.color = Color.green;

            StartCoroutine(BestSmooth(path));
            //return BestSmooth(path);
        }
        //return new List<Vector3>();// path;
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

    public IEnumerator BestSmooth(List<Node> path)
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
                if (!Physics.Raycast(lastNode.position + Vector3.up, (n.position - lastNode.position).normalized, out hitInfo, Vector3.Distance(n.position, lastNode.position), grid.mask))
                {
                    bestNode = n;
                    lastIndex = i;

                    List<Node> temp = new List<Node>(list);
                    temp.Add(bestNode);
                    grid.path = temp;
                    if (displayPathfinding)
                        yield return null;
                }
            }
            if (bestNode == null)
                bestNode = endNode;
            list.Add(bestNode);
            if (displayPathfinding)
                yield return null;
            lastNode = bestNode;
        }

        grid.path = list;
        print("smoothing done");
        grid.ResetColors();
        //return list;
    }
}