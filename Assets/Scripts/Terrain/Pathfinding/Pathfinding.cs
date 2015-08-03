using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using System;

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
    public int requestsPathCount
    {
        get { return pathRequests.Count; }
    }
    static public int requestsPathTotal;

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

    Node n;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            FindPath(new PathfindingParameters(start.position, end.position));

        if (Other.gameStarted)
        {
            if (n != null)
                n.ResetColor();
            n = grid.GetNodeAt(start.position);
            if (n != null)
                n.SetColor(Color.red);
        }

        /*if (Input.GetKeyDown(KeyCode.O))
            advance = true;*/
    }

    static Queue<PathfindingRequest> pathRequests = new Queue<PathfindingRequest>();
    static bool pathRequestActive;

    static public void RequestPath(PathfindingParameters parameters, Action<PathfindingResult> callback)
    {
        requestsPathTotal++;
        pathRequests.Enqueue(new PathfindingRequest(parameters, callback));
        if (!pathRequestActive)
        {
            instance.StopCoroutine("CalculPaths");
            pathRequestActive = true;
            instance.StartCoroutine("CalculPaths");
        }
    }

    IEnumerator CalculPaths()
    {
        while (pathRequests.Count > 0)
        {
            PathfindingRequest request = pathRequests.Dequeue();
            request.callback(FindPath(request.parameter));
            yield return null;
        }

        pathRequestActive = false;
    }

    public bool PathExists(Vector3 start, Vector3 end)
    {
        return FindPath(new PathfindingParameters(start, end) { onlyCheckExists = true }).path.Count > 0;
    }
    public bool PathExists(PathfindingParameters parameters)
    {
        return FindPath(parameters).path.Count > 0;
    }

    PathfindingResult FindPath(PathfindingParameters parameters)
    {
        PathfindingResult result = new PathfindingResult();
        Node startNode = grid.GetNodeAt(parameters.start);
        Node endNode = grid.GetNodeAt(parameters.end);

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
            result.path = new List<Vector3>(cacheResult);
            return result;
        }

        callCount++;
        Stopwatch sw = new Stopwatch();
        sw.Start();


        List<Node> path = new List<Node>();

        RaycastHit hitInfo;
        Collider startObstacle = null;
        Collider endObstacle = null;
        if (Physics.Raycast(parameters.start + new Vector3(0, 5f, 0), Vector3.down, out hitInfo, 10f, grid.mask))
            startObstacle = hitInfo.collider;
        if (Physics.Raycast(parameters.end + new Vector3(0, 5f, 0), Vector3.down, out hitInfo, 10f, grid.mask))
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
                foreach (Node n in path)
                {
                    if (n.obstacle != null)
                    {
                        result.firstStructureHit = n.obstacle;
                        break;
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
                        if (!parameters.ignoreStructure || n.obstacle.tag != "Building")
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

        List<Vector3> resultPath = new List<Vector3>();
        if (path.Count > 0)
        {
            resultPath = NodeListToWaypoints(path);

            if (!parameters.onlyCheckExists)
            {
                if (displayPathfinding)
                    foreach (Node n in path)
                        n.SetColor(new Color(0f, 0.4f, 0f));

                /*if (parameters.radius > 0)
                    resultPath = CorrectPathForRadius(resultPath, parameters.radius);*/
                if (useBestSmooth)
                    resultPath = BestSmoothVector(resultPath, startObstacle, endObstacle);

                if (displayPathfinding)
                    foreach (Node n in path)
                        n.SetColor(Color.green);

                if (useEndPrecisePosition)
                {
                    if (resultPath.Count > 0)
                        resultPath.RemoveAt(resultPath.Count - 1);
                    resultPath.Add(parameters.end);
                }
            }
        }

        if (!Grid.colorLocked)
            showVector = resultPath;

        if (resultPath.Count > 0 && useCache && !parameters.onlyCheckExists)
        {
            cachePathfinding.Add(key, resultPath);
            if (displayCacheDebug)
                print("[ADDED] Path of " + resultPath.Count + " waypoint(s) in cache: " + key);
        }
        sw.Stop();

        float time = sw.ElapsedMilliseconds;
        if (time > 0)
            averageTimes.Add(time);

        result.path = new List<Vector3>(resultPath);
        return result;
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
    
    public List<Vector3> NodeListToWaypoints(List<Node> path)
    {
        List<Vector3> waypoints = new List<Vector3>();
        foreach (Node n in path)
            waypoints.Add(n.position);
        return waypoints;
    }

    public List<Vector3> BestSmoothVector(List<Vector3> path, Collider startCol, Collider endCol)
    {
        List<Vector3> list = new List<Vector3>();
        Vector3 lastPos = path[0];
        int lastIndex = 0;
        Vector3 endPos = path[path.Count - 1];

        Vector3 bestPos = lastPos;
        while (lastPos != endPos)
        {
            bestPos = Vector3.zero;
            for (int i = lastIndex + 1; i < path.Count; i++)
            {
                Vector3 pos = path[i];
                RaycastHit[] hits = Physics.RaycastAll(lastPos + Vector3.up * 0.5f, (pos - lastPos).normalized, Vector3.Distance(pos, lastPos), grid.mask);
                if (hits.Length == 0 || (grid.cornerCutting && Vector3.Distance(pos, lastPos) <= Mathf.Sqrt((grid.nodeSize * grid.nodeSize) * 2)) || HitContainsOnlyCollider(hits, startCol) || HitContainsOnlyCollider(hits, endCol))
                {
                    bestPos = pos;
                    lastIndex = i;
                }
            }
            if (bestPos == Vector3.zero)
            {
                print("[ERROR] BestSmoothVector Path not found");
                return list;
            }
            list.Add(bestPos);
            lastPos = bestPos;
        }
        return list;
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
                RaycastHit[] hits = Physics.RaycastAll(lastNode.position + Vector3.up * 0.5f, (n.position - lastNode.position).normalized, Vector3.Distance(n.position, lastNode.position), grid.mask);
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
            foreach (Collider collider in Physics.OverlapSphere(pos, radius, grid.mask))
            {
                Vector3 point = collider.transform.position;
                showPoints.Add(point);
                newPos += (pos - point).normalized * radius;
            }
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

public class PathfindingRequest
{
    public PathfindingParameters parameter;
    public Action<PathfindingResult> callback;
    public PathfindingRequest(PathfindingParameters _params, Action<PathfindingResult> _callback)
    {
        parameter = _params;
        callback = _callback;
    }
}

public class PathfindingResult
{
    public List<Vector3> path { get; set; }
    public Collider firstStructureHit { get; set; }

    public PathfindingResult()
    {
        path = null;
        firstStructureHit = null;
    }
}

public class PathfindingParameters
{
    public Vector3 start { get; set; }
    public Vector3 end { get; set; }
    public float radius { get; set; }
    public bool onlyCheckExists { get; set; }
    public bool ignoreStructure { get; set; }

    public PathfindingParameters(Vector3 _start, Vector3 _end)
    {
        start = _start;
        end = _end;
        radius = 0f;
        onlyCheckExists = false;
        ignoreStructure = false;
    }
}