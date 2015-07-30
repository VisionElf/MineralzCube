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

    public int isolatedCasesRadius;

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

    List<Case> emptyCases;

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
        StopCoroutine("GenerateMapCR");
        StartCoroutine("GenerateMapCR");
    }
    IEnumerator GenerateMapCR()
    {
        Other.StartStep("Clear objects");
        yield return null;
        ClearObjects();

        Other.NextStep("Init random");
        yield return null;
        InitRandom();

        Other.NextStep("Create map");
        yield return null;
        CreateMap();

        if (smoothBorder)
        {
            Other.NextStep("Create border");
            yield return null;
            CreateBorder(borderSize, ECaseType.Rock);
        }

        Other.NextStep("Smoothing");
        yield return null;
        for (int i = 0; i < smoothCount; i++)
        {
            SmoothMap(smoothThreshold, ECaseType.Rock, ECaseType.Empty);
            yield return null;
        }

        if (!smoothBorder)
        {
            Other.NextStep("Create border");
            yield return null;
            CreateBorder(borderSize, ECaseType.Rock);
        }

        Other.NextStep("Create objects");
        yield return null;
        CreateObjects();

        Other.NextStep("Finding starting cases");
        yield return null;
        List<Case> startingPoints = FindStartingPoints();

        Other.StopStep("Generating map");

        GetComponent<Grid>().CreateGrid();
        GameObject.Find("Player").GetComponent<Player>().CreateStartingUnits(startingPoints[randomGenerator.Next(0, startingPoints.Count)]);
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
        emptyCases = new List<Case>();
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
                cases[x, y] = new Case(x, y, type, GetPositionAt(x, y));
                if (type == ECaseType.Empty)
                    emptyCases.Add(cases[x, y]);
            }
        }
    }
    public void SmoothMap(int threshold, ECaseType type, ECaseType smoothType)
    {
        emptyCases.Clear();
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
                if (cases[x, y].caseType == ECaseType.Empty)
                    emptyCases.Add(cases[x, y]);
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
                    CreateObjectOnMap(GetObjectFromType(cases[x,y].caseType), cases[x,y].position);
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

    public List<Case> FindStartingPoints()
    {
        List<Case> potentialsCases = FindIsolatedCases(isolatedCasesRadius);
        List<CaseGroup> groups = new List<CaseGroup>();
        foreach (Case c in potentialsCases)
            c.color = Color.green;
        int a = 1000;
        while (potentialsCases.Count > 0 && a > 0)
        {
            CaseGroup group = new CaseGroup();
            List<Case> openList = new List<Case>();
            openList.Add(potentialsCases[0]);

            while (openList.Count > 0)
            {
                Case tempCase = openList[0];
                group.Add(tempCase);
                openList.Remove(tempCase);
                potentialsCases.Remove(tempCase);
                foreach (Case c in GetCasesAround(tempCase, 1))
                    if (potentialsCases.Contains(c) && !group.Contains(c) && !openList.Contains(c))
                        openList.Add(c);
            }
            groups.Add(group);
            a--;
        }
        if (a == 0)
            print("Max loops reached " + potentialsCases.Count);

        List<Case> startingCases = new List<Case>();
        foreach (CaseGroup g in groups)
        {
            Case c = g.GetCenterCase();
            c.color = Color.red;
            startingCases.Add(c);
        }

        return startingCases;
    }
    public List<Case> FindIsolatedCases(int radius)
    {
        List<Case> isolatedCases = new List<Case>();
        foreach (Case c in emptyCases)
        {
            float count = GetPercentInCircle(c, radius, ECaseType.Empty);
            if (count >= 1f)
                isolatedCases.Add(c);
        }
        return isolatedCases;
    }

    public GameObject GetObjectFromType(ECaseType type)
    {
        foreach (FillType f in fillTypes)
            if (f.type == type)
                return f.obj;
        return null;
    }

    public Vector3 GetPositionAt(int x, int y)
    {
        return transform.position - mapSize / 2 + new Vector3((x + 0.5f) * caseSize, 0f, (y + 0.5f) * caseSize);
    }
    public Vector3 GetCasePositionAt(Vector3 vec)
    {
        return new Vector3((vec.x / caseSize), 0f, (vec.z / caseSize));
    }
    public List<Case> GetCasesAround(Case c, int distance)
    {
        List<Case> list = new List<Case>();
        for (int i = -distance; i <= distance; i++)
        {
            for (int j = -distance; j <= distance; j++)
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
    public List<Case> GetCasesInCircle(Case center, float radius)
    {
        List<Case> list = new List<Case>();
        int distance = Mathf.CeilToInt(radius);
        for (int i = -distance; i <= distance; i++)
        {
            for (int j = -distance; j <= distance; j++)
            {
                if (i != 0 || j != 0)
                {
                    int x = i + center.x;
                    int y = j + center.y;
                    if (InBounds(x, y) && Vector3.Distance(center.position, cases[x,y].position) <= radius)
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
    public float GetPercentInCircle(Case center, float radius, ECaseType type)
    {
        int count = 0;
        int total = 0;
        int distance = Mathf.CeilToInt(radius);
        for (int i = -distance; i <= distance; i++)
        {
            for (int j = -distance; j <= distance; j++)
            {
                if (i != 0 || j != 0)
                {
                    int x = i + center.x;
                    int y = j + center.y;
                    if (InBounds(x, y) && Vector3.Distance(center.position, cases[x, y].position) <= radius)
                    {
                        if (cases[x, y].caseType == type)
                            count++;
                        total++;
                    }
                }
            }
        }
        return (float)count / total;
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
                    Gizmos.DrawCube(c.position, Vector3.one * caseSize);
                }
            }
        }
    }

}
