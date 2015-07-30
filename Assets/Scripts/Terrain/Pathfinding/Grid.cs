using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Grid : MonoBehaviour
{
    public bool drawEditorGizmos;
    public bool drawGameGizmos;

    public bool cornerCutting;

    public bool autoSize;

    public int gridLengthX;
    public int gridSizeX
    {
        get
        {
            if (autoSize)
                return GetComponent<Map>().mapSizeX;
            return gridLengthX;
        }
    }

    public int gridLengthY;
    public int gridSizeY
    {
        get
        {
            if (autoSize)
                return GetComponent<Map>().mapSizeY;
            return gridLengthY;
        }
    }

    public float nodeSize;

    public LayerMask mask;

    Node[,] nodes;

    float gridWorldSizeX
    {
        get {
            return gridSizeX * nodeSize;
        }
    }
    float gridWorldSizeY
    {
        get {
            return gridSizeY * nodeSize;
        }
    }
    Vector3 gridWorldSize
    {
        get { return new Vector3(gridWorldSizeX, 1f, gridWorldSizeY); }
    }

    public void CreateGrid()
    {
        nodes = new Node[gridSizeX, gridSizeY];

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                Vector3 position = GetPositionAt(x, y);
                nodes[x, y] = new Node(x, y, IsWalkable(position), position);
            }
        }
    }
    public List<Node> GetNeighbours(Node node, int distance)
    {
        List<Node> list = new List<Node>();
        for (int i = -distance; i <= distance; i++)
        {
            for (int j = -distance; j <= distance; j++)
            {
                if (i != 0 || j != 0)
                {
                    if (cornerCutting || (i == 0 || j == 0))
                    {
                        int x = i + node.x;
                        int y = j + node.y;
                        if (InBounds(x, y))
                            list.Add(nodes[x, y]);
                    }
                }
            }
        }
        return list;
    }
    public void ResetColors()
    {
        foreach (Node n in nodes)
            n.ResetColor();
    }

    public int gridSize { get { return gridSizeX * gridSizeY; } }

    public bool InBounds(int x, int y) { return x >= 0 && x < gridSizeX && y >= 0 && y < gridSizeY; }

    public bool IsWalkable(Vector3 position)
    {
        RaycastHit hitInfo;
        if (Physics.Raycast(position + new Vector3(0, 5f, 0), Vector3.down, out hitInfo, 10f, mask))
        {
            return false;
        }
        return true;
    }

    public Vector3 GetPositionAt(int x, int y)
    {
        return transform.position + new Vector3(x * nodeSize + nodeSize / 2, 0, y * nodeSize + nodeSize / 2) - gridWorldSize / 2;
    }
    public Node GetNodeAt(Vector3 position)
    {
        position += gridWorldSize / 2;
        int x = Mathf.RoundToInt((position.x - nodeSize / 2) / nodeSize);
        int y = Mathf.RoundToInt((position.z - nodeSize / 2) / nodeSize);

        if (InBounds(x, y))
            return nodes[x, y];
        return null;
    }

    public DistanceType distanceType;
    public enum DistanceType
    {
        Correct, Wrong, Custom, Vector, Direct
    }
    public int GetDistance(Node n1, Node n2)
    {
        switch (distanceType)
        {
            case DistanceType.Correct:
                int dstX = Mathf.Abs(n1.x - n2.x);
                int dstY = Mathf.Abs(n1.y - n2.y);

                if (dstX > dstY)
                    return 14 * dstY + 10 * (dstX - dstY);
                return 14 * dstX + 10 * (dstY - dstX);
            case DistanceType.Wrong:
                int coefD = 14;
                int coefL = 10;

                int dx = Mathf.Abs(n1.x - n2.x);
                int dy = Mathf.Abs(n1.y - n2.y);

                int nbDiago = Mathf.Min(dx, dy);
                int nbLigne = Mathf.Max(dx, dy);

                return nbDiago * coefD + nbLigne * coefL;
            case DistanceType.Vector:
                return Mathf.RoundToInt(Vector3.Distance(n1.position, n2.position) * 10);
            case DistanceType.Custom:
                return n1.x - n2.x;
            case DistanceType.Direct:
                return Mathf.Abs(n1.x - n2.x) + Mathf.Abs(n1.y - n2.y);

            default:
                return 1;

        }
    }

    public List<Node> path = new List<Node>();
    public void OnDrawGizmos()
    {
        if (drawEditorGizmos)
        {
            Gizmos.DrawWireCube(transform.position, gridWorldSize);
            for (int x = 0; x < gridSizeX; x++)
            {
                for (int y = 0; y < gridSizeY; y++)
                {
                    Gizmos.color = Color.black;
                    Gizmos.DrawWireCube(GetPositionAt(x, y), Vector3.one * nodeSize);
                }
            }
        }

        if (nodes != null && drawGameGizmos)
        {
            foreach (Node n in nodes)
            {
                Gizmos.color = n.color;
                float size = nodeSize - 0.1f;
                Gizmos.DrawCube(n.position + new Vector3(0f, size / 2, 0f), Vector3.one * size);
            }
        }

        if (path.Count > 0)
        {
            Gizmos.DrawCube(path[0].position, Vector3.one * nodeSize);
            for (int i = 1; i < path.Count; i++)
            {
                Gizmos.color = new Color(0, 0.5f, 0);
                Gizmos.DrawCube(path[i].position, Vector3.one * nodeSize);
                Gizmos.DrawLine(path[i - 1].position + Vector3.up, path[i].position + Vector3.up);
            }
        }
    }
}
