using UnityEngine;
using System.Collections;

public class Map : MonoBehaviour {

    //UNITY INSPECTOR PROPERTIES
    [Tooltip("Detects automatically terrain size.")]
    public bool autoSize;

    public float mapWorldWidth;
    public float mapWidth
    {
        get {
            if (autoSize)
                return transform.localScale.x * 10;
            return mapWorldWidth;
        }
        set { mapWorldWidth = value; }
    }

    public float mapWorldHeight;
    public float mapHeight
    {
        get
        {
            if (autoSize)
                return transform.localScale.z * 10;
            return mapWorldHeight;
        }
        set { mapWorldHeight = value; }
    }

    [Range(0.1f, 10f)]
    public float caseSize;

    //PROPERTIES
    Case[,] cases;

    public int mapSizeX
    {
        get { return Mathf.RoundToInt(mapWidth / caseSize); }
    }
    public int mapSizeY
    {
        get { return Mathf.RoundToInt(mapHeight / caseSize); }
    }
    public Vector3 mapSize
    {
        get {
            return new Vector3(mapWidth, 0, mapHeight);
        }
    }

    //FUNCTIONS
    public void CreateMap()
    {
        cases = new Case[mapSizeX, mapSizeY];
    }

    //DRAW
    public void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, mapSize);
    }

}
