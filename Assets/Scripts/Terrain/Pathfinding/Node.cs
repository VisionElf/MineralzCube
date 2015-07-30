using UnityEngine;
using System.Collections;

public class Node : IHeapItem<Node>
{

    public int x;
    public int y;

    public Vector3 position;
    public Color color;
    public bool walkable;

    public float gCost;
    public float hCost;

    public Node parent;

    public float fCost { get { return gCost + hCost; } }

    public Node(int _x, int _y, bool _walkable, Vector3 pos)
    {
        x = _x;
        y = _y;
        walkable = _walkable;
        position = pos;

        ResetColor();
    }

    public void ResetColor() { color = (walkable) ? Color.white : Color.black; }

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
