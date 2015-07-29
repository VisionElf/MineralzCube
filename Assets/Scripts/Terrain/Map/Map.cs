using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Map : MonoBehaviour {

    //UNITY INSPECTOR PROPERTIES
    //MAP SIZE
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

    //RANDOM
    public string randomSeed;
    public bool useRandomSeed;

    public List<FillType> fillTypes;
    [System.Serializable]
    public struct FillType
    {
        [Range(0, 100)]
        public int randomFillPercent;
        public ECaseType type;
        public GameObject obj;

        static public int Compare(FillType t1, FillType t2)
        {
            return t1.randomFillPercent.CompareTo(t2.randomFillPercent);
        }
    }

    public int smoothCount;
    public int smoothThreshold;

    public int borderSize;
    public bool smoothBorder;

    public bool drawGizmos;

    //PUBLIC PROPERTIES
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

    //PRIVATE PROPERTIES
    Case[,] cases;
    System.Random randomGenerator;

    //STATIC
    static public Map instance;

    //UNITY FUNCTIONS
    void Start()
    {
        instance = this;
    }

    //FUNCTIONS
    public void GenerateMap()
    {
        ClearObjects();
        InitRandom();
        CreateMap();
        if (smoothBorder)
            CreateBorder(borderSize, ECaseType.Rock);
        for (int i = 0; i < smoothCount; i++)
            SmoothMap(smoothThreshold, ECaseType.Rock, ECaseType.Empty);
        if (!smoothBorder)
            CreateBorder(borderSize, ECaseType.Rock);
        CreateObjects();
    }

    public void InitRandom()
    {
        if (useRandomSeed)
            randomGenerator = new System.Random();
        else
            randomGenerator = new System.Random(randomSeed.GetHashCode());
    }
    public void CreateMap()
    {
        cases = new Case[mapSizeX, mapSizeY];
        for (int x = 0; x < cases.GetLength(0); x++)
        {
            for (int y = 0; y < cases.GetLength(1); y++)
            {
                ECaseType type = ECaseType.Empty;
                int rnd = randomGenerator.Next(0, 100);
                fillTypes.Sort(FillType.Compare);
                foreach (FillType t in fillTypes)
                {
                    if (rnd < t.randomFillPercent)
                    {
                        type = t.type;
                        break;
                    }
                }
                cases[x, y] = new Case(x, y, type);
            }
        }
    }
    public void SmoothMap(int threshold, ECaseType type, ECaseType smoothType)
    {
        for (int x = 0; x < cases.GetLength(0); x++)
        {
            for (int y = 0; y < cases.GetLength(1); y++)
            {
                if (cases[x, y].caseType == type || cases[x, y].caseType == smoothType)
                {
                    int tmp = GetCountAround(cases[x, y], 1, type, true);
                    if (tmp > threshold)
                        cases[x, y].caseType = type;
                    else if (tmp < threshold)
                        cases[x, y].caseType = smoothType;
                }
            }
        }
    }
    public void CreateBorder(int size, ECaseType borderType)
    {
        for (int x = 0; x < cases.GetLength(0); x++)
        {
            for (int y = 0; y < cases.GetLength(1); y++)
            {
                if (x < size || x >= cases.GetLength(0) - size || y < size || y >= cases.GetLength(1) - size)
                    cases[x, y].caseType = borderType;
            }
        }
    }
    public void ClearObjects()
    {
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Terrain"))
        {
            GameObject.Destroy(obj);
        }
    }
    public void CreateObjects()
    {
        for (int x = 0; x < cases.GetLength(0); x++)
        {
            for (int y = 0; y < cases.GetLength(1); y++)
            {
                if (cases[x,y].caseType != ECaseType.Empty)
                    CreateObjectOnMap(GetObjectFromType(cases[x,y].caseType), GetPositionAt(cases[x,y]));
            }
        }
    }

    public void CreateObjectOnMap(GameObject type, Vector3 position)
    {
        GameObject obj = GameObject.Instantiate(type);
        obj.transform.position = GetCasePositionAt(position);
        obj.transform.position += new Vector3(0, randomGenerator.Next(0, 100) * 0.005f - 0.25f, 0f);
        obj.transform.Rotate(Vector3.up * randomGenerator.Next(0, 4) * 90);
        obj.tag = "Terrain";
    }

    

    public GameObject GetObjectFromType(ECaseType type)
    {
        foreach (FillType f in fillTypes)
            if (f.type == type)
                return f.obj;
        return null;
    }

    public Vector3 GetPositionAt(Case c)
    {
        return transform.position - mapSize / 2 + new Vector3((c.x + 0.5f) * caseSize, 0f, (c.y + 0.5f) * caseSize);
    }
    public Vector3 GetCasePositionAt(Vector3 vec)
    {
        return new Vector3((vec.x / caseSize), 0f, (vec.z / caseSize));
    }
    public List<Case> GetNodesAround(Case c, int radius)
    {
        List<Case> list = new List<Case>();
        for (int i = -radius; i <= radius; i++)
        {
            for (int j = -radius; j <= radius; j++)
            {
                if (i != 0 || j != 0)
                {
                    int x = i + c.x;
                    int y = j + c.y;
                    if (InBounds(x,y))
                        list.Add(cases[x, y]);
                }
            }
        }
        return list;
    }
    public int GetCountAround(Case c, int radius, ECaseType type, bool borderCount)
    {
        int count = 0;
        for (int i = -radius; i <= radius; i++)
        {
            for (int j = -radius; j <= radius; j++)
            {
                if (i != 0 || j != 0)
                {
                    int x = i + c.x;
                    int y = j + c.y;
                    if (InBounds(x, y))
                    {
                        if (cases[x, y].caseType == type)
                            count++;
                    }
                    else if (borderCount)
                        count++;
                }
            }
        }
        return count;
    }
    public bool InBounds(int x, int y)
    {
        return x >= 0 && x < cases.GetLength(0) && y >= 0 && y < cases.GetLength(1);
    }

    //DRAW
    public void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, mapSize);

        if (drawGizmos)
        {
            if (cases != null)
            {
                foreach (Case c in cases)
                {
                    Gizmos.color = c.GetColor();
                    Gizmos.DrawCube(GetPositionAt(c), Vector3.one * caseSize);
                }
            }
        }
    }

}
