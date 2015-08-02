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
    public bool displayCacheDebug;
    public bool displaySmoothing;

    static public Pathfinding instance;
    public int callCount;
    public int cacheCallCount;

    public bool useCache;
    public bool useEndPrecisePosition;
    public bool useBestSmooth;

    List<float> averageTimes;
    public float GetAverageTime()
    {
        if (averageTimes.Count > 0)
        {
            float count = 0f;
            foreach (float f in averageTimes)
                count += f;
            return count / averageTimes.Count;
        }
        return 0f;
    }

    public int cacheCount
    {
        get { return cachePathfinding.Count; }
    }

    void Start()
    {
        grid = GetComponent<Grid>();
        instance = this;

        cachePathfinding = new Dictionary<string, List<Vector3>>();
        averageTimes = new List<float>();
    }

    //bool advance;

    Dictionary<string, List<Vector3>> cachePathfinding;
    public void RefreshCache()
    {
        cachePathfinding.Clear();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            FindPath(start.position, end.position, 0f);

        /*if (Input.GetKeyDown(KeyCode.O))
            advance = true;*/
    }

    public bool PathExists(Vector3 start, Vector3 end, float radius)
    {
        Collider temp;
        return FindPath(start, end, radius, true, false, out temp).Count > 0;
    }
    public List<Vector3> FindPath(Vector3 start, Vector3 end, float radius)
    {
        Collider temp;
        return FindPath(start, end, radius, false, false, out temp);
        //return new List<Vector3>();
    }
    public List<Vector3> FindPath(Vector3 start, Vector3 end, float radius, bool checkExists, bool ignoreStructure, out Collider structureHit)
    {
        structureHit = null;
        Node startNode = grid.GetNodeAt(start);
        Node endNode = grid.GetNodeAt(end);

        if (displayPathfinding)
            grid.ResetColors();

        string key = startNode.x + "," + startNode.y + ";" + endNode.x + "," + endNode.y;
        if (useCache && cachePathfinding.ContainsKey(key))
        {
            List<Vector3> cacheResult = cachePathfinding[key];
            if (displayPathfinding)
                foreach (Vector3 r in cacheResult)
                    grid.GetNodeAt(r).SetColor(Color.magenta);
            if (displayCacheDebug)
                print("[FOUND] Path of " + cacheResult.Count + " waypoint(s) in cache: " + key);
            cacheCallCount++;
            return new List<Vector3>(cacheResult);
        }

        callCount++;
        Stopwatch sw = new Stopwatch();
        sw.Start();


        List<Node> path = new List<Node>();

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
            if (displayPathfinding)
                currentNode.SetColor(Color.blue);

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

                    if (displayPathfinding)
                        n.SetColor(Color.yellow);

                    if (!openList.Contains(n))
                        openList.Add(n);
                    else
                        openList.UpdateItem(n);
                }
            }
            showNodes.Remove(currentNode);
            if (displayPathfinding)
                currentNode.SetColor(Color.red);
        }

        List<Vector3> result = new List<Vector3>();
        if (path.Count > 0 && !checkExists)
        {
            if (displayPathfinding)
                foreach (Node n in path)
                    n.SetColor(new Color(0f, 0.4f, 0f));

            if (useBestSmooth)
                path = BestSmooth(path, startObstacle, endObstacle);

            if (displayPathfinding)
                foreach (Node n in path)
                    n.SetColor(Color.green);
        }

        if (path.Count > 0)
        {
            result = NodeListToWaypoints(path);

            if (useEndPrecisePosition)
            {
                if (result.Count > 0)
                    result.RemoveAt(result.Count - 1);
                result.Add(end);
            }
            
            /*
            if (radius > 0)
                result = CorrectPathForRadius(result, radius);*/
        }
        if (!Grid.colorLocked)
            showVector = result;

        if (result.Count > 0 && useCache && !checkExists)
        {
            cachePathfinding.Add(key, result);
            if (displayCacheDebug)
                print("[ADDED] Path of " + result.Count + " waypoint(s) in cache: " + key);
        }
        sw.Stop();

        float time = sw.ElapsedMilliseconds;
        if (time > 0)
            averageTimes.Add(time);

        return new List<Vector3>(result);
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
        RaycastHit[] lastHits = new RaycastHit[0];
        while (lastNode != endNode)
        {
            bestNode = null;
            for (int i = lastIndex + 1; i < path.Count; i++)
            {
                Node n = path[i];
                RaycastHit[] hits = Physics.RaycastAll(lastNode.position + Vector3.up, (n.position - lastNode.position).normalized, Vector3.Distance(n.position, lastNode.position), grid.mask);
                if (hits.Length == 0 || (grid.cornerCutting && n.IsInDiagonal(lastNode)) || HitContainsOnlyCollider(hits, startCol) || HitContainsOnlyCollider(hits, endCol))
                {
                    bestNode = n;
                    lastIndex = i;
                }
                else
                    lastHits = hits;
            }
            if (bestNode == null)
            {
                if (!Grid.colorLocked)
                {
                    print("No path found, list=" + list.Count + ",path=" + path.Count);
                    print("lastIndex=" + lastIndex + " lastNode=" + lastNode);
                    showPoints.Clear();
                    for (int i = 0; i < lastHits.Length; i++)
                    {
                        print(lastHits[i].collider.name + " - " + lastHits[i].point);
                        showPoints.Add(lastHits[i].point);
                    }
                    if (displayPathfinding)
                        foreach (Node n in list)
                            n.SetColor(Color.cyan);
                    path[0].SetColor(Color.blue);
                    lastNode.SetColor(Color.magenta);
                    endNode.SetColor(new Color(0f, 0f, 0.5f));

                    Grid.colorLocked = true;
                }


                return list;
            }
            list.Add(bestNode);
            lastNode = bestNode;
        }
        return list;
    }

    public List<Vector3> CorrectPathForRadius(List<Vector3> path, float radius)
    {
        List<Vector3> correctPath = new List<Vector3>();
        foreach (Vector3 pos in path)
        {
            Vector3 newPos = pos;
            RaycastHit hit;
            if (Physics.SphereCast(newPos, radius, Vector3.right, out hit, radius, grid.mask))
                newPos -= (hit.point - newPos).normalized * radius;
            correctPath.Add(newPos);
        }
        return correctPath;
    }

    public bool HitContainsOnlyCollider(RaycastHit[] hits, Collider collider)
    {
        foreach (RaycastHit r in hits)
            if (r.collider != collider)
                return false;
        return true;
    }

    List<Vector3> showVector = new List<Vector3>();
    List<Vector3> showPoints = new List<Vector3>();
    List<Node> showNodes = new List<Node>();
    void OnDrawGizmos()
    {
        if (displayPathfinding)
        {
            if (showVector.Count > 0)
            {
                Gizmos.color = Color.green;
                for (int i = 1; i < showVector.Count; i++)
                    Gizmos.DrawLine(showVector[i - 1] + Vector3.up * 0.5f, showVector[i] + Vector3.up * 0.5f);
            }

            if (showPoints.Count > 0)
            {
                Gizmos.color = Color.red;
                for (int i = 0; i < showPoints.Count; i++)
                    Gizmos.DrawSphere(showPoints[i], 0.1f);
            }
        }
    }
}

