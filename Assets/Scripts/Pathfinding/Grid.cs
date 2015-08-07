using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid : MonoBehaviour {

    //PUBLIC
    public Map map;
    public float nodeSize;

    public LayerMask obstaclesMask;

    public bool drawGizmos;

    public bool useVectorDistance;
    public bool cornerCutting;

    public bool drawGridLine;

    //PRIVATE
    Node[,] nodes;

    int gridWidth;
    int gridHeight;

    public int gridMaxSize { get { return gridWidth * gridHeight; } }

    static public Grid instance;

    void Start()
    {
        instance = this;
        CreateGrid(Mathf.RoundToInt(map.mapWidth / nodeSize), Mathf.RoundToInt(map.mapHeight / nodeSize));
    }

    public void CreateGrid(int width, int heigth)
    {
        gridWidth = width;
        gridHeight = heigth;

        nodes = new Node[gridWidth, gridHeight];
        for (int x = 0; x < nodes.GetLength(0); x++)
        {
            for (int y = 0; y < nodes.GetLength(1); y++)
            {
                Vector3 position = GetPositionAt(x, y);
                nodes[x, y] = new Node(x, y, position);
                nodes[x, y].SetWalkable(true);
                //UpdateWalkable(nodes[x, y]);
            }
        }
    }

    public bool IsObstacle(Vector3 position, float radius)
    {
        return Physics.OverlapSphere(position + Vector3.up, radius, obstaclesMask).Length > 0;
    }
    public bool GetWalkable(Node node, float radius)
    {
        if (IsObstacle(node.position, radius))
            node.SetWalkable(false);
        else
            node.SetWalkable(true);
        return node.walkable;
    }
    public void UpdateWalkable(Node node)
    {
        if (Physics.Raycast(node.position + Vector3.up, Vector3.down, 2f, obstaclesMask))
            node.SetWalkable(false);
        else
            node.SetWalkable(true);
    }

    public Vector3 GetPositionAt(int x, int y)
    {
        return transform.position - map.mapSize / 2 + new Vector3(x * nodeSize + nodeSize / 2, 0, y * nodeSize + nodeSize / 2);
    }
    public Node GetNodeAt(Vector3 position)
    {
        position += map.mapSize / 2;
        int x = Mathf.RoundToInt((position.x - nodeSize / 2) / nodeSize);
        int y = Mathf.RoundToInt((position.z - nodeSize / 2) / nodeSize);

        if (x < 0)
            x = 0;
        else if (x >= nodes.GetLength(0))
            x = nodes.GetLength(0) - 1;
        if (y < 0)
            y = 0;
        else if (y >= nodes.GetLength(1))
            y = nodes.GetLength(1) - 1;

        return nodes[x, y];
    }
    public List<Node> GetNeighbours(Node center)
    {
        List<Node> list = new List<Node>();
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if ((i != 0 || j != 0) && (cornerCutting || i == 0 || j == 0))
                {
                    int x = i + center.x;
                    int y = j + center.y;

                    if (InBounds(x, y))
                        list.Add(nodes[x, y]);
                }
            }
        }
        return list;
    }
    public Vector3 SnapPositionToGrid(Vector3 position)
    {
        float size = nodeSize / 2f;
        return new Vector3(Mathf.RoundToInt(position.x / size) * size, position.y, Mathf.RoundToInt(position.z / size) * size);
    }

    public List<Node> GetNodesInSquare(Vector3 position, int sizeX, int sizeY)
    {
        List<Node> list = new List<Node>();
        Vector3 pos = SnapPositionToGrid(position) - new Vector3(sizeX / 2f - 0.5f, 0, sizeY / 2f - 0.5f) * nodeSize;
        for (int i = 0; i < sizeX; i++ )
        {
            for (int j = 0; j < sizeY ; j++)
            {
                Vector3 vector = pos + new Vector3(i, 0, j) * nodeSize;
                if (InBounds(vector))
                {
                    Node node = GetNodeAt(vector);
                    node.color = Color.magenta;
                    list.Add(node);
                }
            }
        }
        return list;
    }

    public int GetDistance(Node n1, Node n2)
    {
        if (useVectorDistance)
            return Mathf.RoundToInt(Vector3.Distance(n1.position, n2.position));
        else
        {
            int dstX = Mathf.Abs(n1.x - n2.x);
            int dstY = Mathf.Abs(n1.y - n2.y);

            if (dstX > dstY)
                return 14 * dstY + 10 * (dstX - dstY);
            return 14 * dstX + 10 * (dstY - dstX);
        }
    }
    public void ResetColors()
    {
        foreach (Node n in nodes)
            n.ResetColor();
    }

    public bool GridLine(Vector3 startPos, Vector3 endPos, float radius)
    {
        Vector3 pos = startPos;
        Vector3 dir = (endPos - startPos).normalized;
        Node node = GetNodeAt(startPos);
        Node endNode = GetNodeAt(endPos);

        while (node != endNode)
        {
            if (drawGridLine)
                node.color = Color.blue;
            if (IsObstacle(pos, radius))
            {
                if (drawGridLine)
                    node.color = Color.red;
                return true;
            }
            pos += dir * nodeSize / 1f;
            node = GetNodeAt(pos);
        }
        return IsObstacle(pos, radius);
    }

    public bool InBounds(int x, int y) { return x >= 0 && x < nodes.GetLength(0) && y >= 0 && y < nodes.GetLength(1); }
    public bool InBounds(Vector3 pos) { return pos.x > -map.mapSize.x / 2 && pos.x < map.mapSize.x / 2 && pos.y >= -map.mapSize.z / 2 && pos.y < map.mapSize.z / 2; }

    void OnDrawGizmos()
    {
        if (nodes != null && drawGizmos)
        {
            foreach (Node n in nodes)
            {
                Gizmos.color = n.color;
                Gizmos.DrawCube(n.position, new Vector3(1, 0.1f, 1) * nodeSize);
            }
        }
    }
}
