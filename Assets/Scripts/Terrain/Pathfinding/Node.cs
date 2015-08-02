using UnityEngine;
using System.Collections;

public class Node : IHeapItem<Node>
{

    public int x;
    public int y;

    Vector3 _position;
    public Vector3 position { get { return _position; } }
    public Color color;
    public bool walkable;

    public float gCost;
    public float hCost;

    public Node parent;
    public Collider obstacle;

    public float fCost { get { return gCost + hCost; } }

    public Node(int _x, int _y, Vector3 pos)
    {
        x = _x;
        y = _y;
        _position = pos;

        SetWalkable(true, null);
        ResetColor();
    }

    public void SetWalkable(bool _walkable, Collider collider)
    {
        walkable = _walkable;
        if (!walkable)
            obstacle = collider;
        else
            obstacle = null;
        color = walkable ? Color.white : Color.black;
    }

    public void ResetColor() { color = (walkable) ? Color.white : Color.black; }
    public bool IsInDiagonal(Node node)
    {
        return (node.x == x + 1 && node.y == y + 1
            || node.x == x - 1 && node.y == y + 1
            || node.x == x + 1 && node.y == y - 1
            || node.x == x - 1 && node.y == y - 1);
    }

    public override string ToString()
    {
        return "x: " + x + " , y:" + y;
    }

    public int heapIndex;
    public int HeapIndex { get { return heapIndex; } set { heapIndex = value; } }

    public int CompareTo(Node nodeToCompare)
    {
        int compare = fCost.CompareTo(nodeToCompare.fCost);
        if (compare == 0)
            compare = hCost.CompareTo(nodeToCompare.hCost);
        return -compare;
    }
}
