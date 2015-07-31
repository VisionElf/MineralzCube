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
        public Entity entity;

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
    public bool printDebug;

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

        GenerateMap();
    }

    //FUNCTIONS
    public void GenerateMap()
    {
        StopCoroutine("GenerateMapCR");
        StartCoroutine("GenerateMapCR");
    }
    IEnumerator GenerateMapCR()
    {
        Other.print = printDebug;
        int[] loadings = new int[] { 10, 10, 100, 50, 200, 500, 1000, 100, 50 };
        int max = 0;
        foreach (int i in loadings)
            max += i;
        Other.StartStep("Clearing objects", loadings[0], max);
        yield return null;
        ClearObjects();

        Other.NextStep("Initializing random", loadings[1]);
        yield return null;
        InitRandom();

        Other.NextStep("Creating map", loadings[2]);
        yield return null;
        CreateMap();

        if (smoothBorder)
        {
            Other.NextStep("Creating border", loadings[3]);
            yield return null;
            CreateBorder(borderSize, ECaseType.Rock);
        }

        Other.NextStep("Smoothing map", loadings[4]);
        yield return null;
        for (int i = 0; i < smoothCount; i++)
        {
            SmoothMap(smoothThreshold, ECaseType.Rock, ECaseType.Empty);
            yield return null;
        }

        if (!smoothBorder)
        {
            Other.NextStep("Creating border", loadings[3]);
            yield return null;
            CreateBorder(borderSize, ECaseType.Rock);
        }

        Other.NextStep("Creating objects", loadings[5]);
        yield return null;
        CreateObjects();

        Other.NextStep("Finding starting cases", loadings[6]);
        yield return null;
        List<Case> startingPoints = FindStartingPoints();


        Other.NextStep("Creating pathfinding grid", loadings[7]);
        yield return null;
        GetComponent<Grid>().CreateGrid();

        Other.StopStep("Generating map", loadings[8]);

        GameObject.Find("Player").GetComponent<Player>().CreateStartingUnits(startingPoints[randomGenerator.Next(0, startingPoints.Count)]);
        GameObject.Find("Player").GetComponent<Player>().OnGameStarted();
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
                    CreateEntityOnMap(GetEntityFromType(cases[x,y].caseType), cases[x,y].position);
            }
        }
    }

    public void CreateEntityOnMap(Entity type, Vector3 position)
    {
        GameObject obj = GameObject.Instantiate(type.gameObject);
        obj.transform.position = SnapToGrid(position, obj.GetComponent<Entity>().buildingProperties);
        obj.GetComponentInChildren<Renderer>().gameObject.transform.position -= new Vector3(0, randomGenerator.Next(0, 100) * 0.25f / 100f, 0f);
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

    public Entity GetEntityFromType(ECaseType type)
    {
        foreach (FillType f in fillTypes)
            if (f.type == type)
                return f.entity;
        return null;
    }

    public Vector3 GetPositionAt(int x, int y)
    {
        return transform.position - mapSize / 2 + new Vector3((x + 0.5f) * caseSize, 0f, (y + 0.5f) * caseSize);
    }
    public Vector3 SnapToGrid(Vector3 position, BuildingEntity building)
    {
        Vector3 dec = new Vector3();
        position.x = Mathf.RoundToInt((position.x - caseSize / 2) / caseSize) * caseSize + caseSize / 2;
        position.z = Mathf.RoundToInt((position.z - caseSize / 2) / caseSize) * caseSize + caseSize / 2;
        if (building != null)
        {
            dec.x = (building.caseSizeX % 2 == 0) ? caseSize / 2 : 0;
            dec.z = (building.caseSizeY % 2 == 0) ? caseSize / 2 : 0;
        }
        return position + dec;
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
