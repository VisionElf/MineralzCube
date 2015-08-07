using UnityEngine;
using System.Collections;

public class Node : IHeapItem<Node> {

    //PUBLIC
    public Vector3 position;
    public Color color;

    public int gCost;
    public int hCost;
    public int fCost { get { return gCost + hCost; } }

    public int x;
    public int y;

    public bool walkable;
    public Node parent;

    public int HeapIndex { get { return heapIndex; } set { heapIndex = value; } }

    //PRIVATE


    int heapIndex;


    public Node(int _x, int _y, Vector3 pos)
    {
        x = _x;
        y = _y;
        position = pos;
        color = Color.white;
        ResetColor();
    }

    public void SetWalkable(bool _walkable)
    {
        walkable = _walkable;
        if (!walkable)
            color = Color.black;
        //ResetColor();
    }

    public int CompareTo(Node nodeToCompare)
    {
        int compare = fCost.CompareTo(nodeToCompare.fCost);
        if (compare == 0)
            compare = hCost.CompareTo(nodeToCompare.hCost);
        return -compare;
    }

    public void ResetColor()
    {
        color = walkable ? Color.white : Color.black;
    }
}
