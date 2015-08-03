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

    public Entity creepSpawnEntity;

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
    List<CreepSpawnEntity> creepSpawns;


    //STATIC
    static public Map instance;

    //UNITY FUNCTIONS
    void Start()
    {
        instance = this;

        GenerateMap();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
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
            SmoothMap(smoothThreshold);
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
        
        Case startingCase = startingPoints[randomGenerator.Next(0, startingPoints.Count)];
        GameObject.Find("Player").GetComponent<Player>().CreateStartingUnits(startingCase.position);
        startingPoints.Remove(startingCase);

        CreateCreepSpawns(startingCase, startingPoints);

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
    public void SmoothMap(int threshold)
    {
        emptyCases.Clear();
        for (int x = 0; x < cases.GetLength(0); x++)
        {
            for (int y = 0; y < cases.GetLength(1); y++)
            {
                Case currentCase = cases[x, y];
                KeyValuePair<ECaseType, int> mostTypePair = GetTypeAround(currentCase, 1, false);

                if (mostTypePair.Value > threshold)
                    cases[x, y].caseType = mostTypePair.Key;
                else if (mostTypePair.Value < threshold)
                    cases[x, y].caseType = ECaseType.Empty;

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
                    CreateEntityOnMap(GetEntityFromType(cases[x,y].caseType), cases[x,y].position, true);
            }
        }
    }

    public GameObject CreateEntityOnMap(Entity type, Vector3 position, bool randomHeight)
    {
        GameObject obj = GameObject.Instantiate(type.gameObject);
        obj.transform.position = SnapToGrid(position, obj.GetComponent<Entity>().buildingProperties);
        if (randomHeight)
            obj.GetComponentInChildren<Renderer>().gameObject.transform.position -= new Vector3(0, randomGenerator.Next(0, 100) * 0.25f / 100f, 0f);
        obj.transform.Rotate(Vector3.up * randomGenerator.Next(0, 4) * 90);
        obj.tag = "Terrain";
        return obj;
    }

    public void CreateCreepSpawns(Case startingCase, List<Case> startingPoints)
    {
        creepSpawns = new List<CreepSpawnEntity>();
        Case creepCase = null;
        while (startingPoints.Count > 0 && creepCase == null)
        {
            Case temp = startingPoints[randomGenerator.Next(0, startingPoints.Count)];
            startingPoints.Remove(temp);
            float distance = Vector3.Distance(startingCase.position, temp.position);
            if (Pathfinding.instance.PathExists(temp.position, startingCase.position) && distance > 10f)
            {
                creepCase = temp;
                break;
            }
        }
        if (creepCase == null)
            print("No creep spawn point found");
        else
        {
            creepSpawns.Add(CreateEntityOnMap(creepSpawnEntity, creepCase.position, false).GetComponent<CreepSpawnEntity>());
        }
    }
    public void StartSpawnCreeps()
    {
        foreach (CreepSpawnEntity creepSpawn in creepSpawns)
            creepSpawn.StartSpawn();
    }
    public void DestroyCreeps()
    {
        foreach (GameObject ent in GameObject.FindGameObjectsWithTag("Creep"))
            ent.GetComponent<Entity>().RemoveObject();
    }
    public List<Entity> GetAllEnemies()
    {
        List<Entity> list = new List<Entity>();
        foreach (GameObject obj in GameObject.FindGameObjectsWithTag("Creep"))
        {
            Entity ent = obj.GetComponent<Entity>();
            if (ent != null)
                list.Add(ent);
        }
        return list;
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

    public int GetCountAround(Case c, int distance, ECaseType type, bool borderCount)
    {
        int count = 0;
        for (int i = -distance; i <= distance; i++)
        {
            for (int j = -distance; j <= distance; j++)
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
    public KeyValuePair<ECaseType, int> GetTypeAround(Case c, int distance, bool borderCount)
    {
        Dictionary<ECaseType, int> counts = new Dictionary<ECaseType, int>();
        for (int i = -distance; i <= distance; i++)
        {
            for (int j = -distance; j <= distance; j++)
            {
                if (i != 0 || j != 0)
                {
                    int x = i + c.x;
                    int y = j + c.y;
                    if (InBounds(x, y))
                    {
                        ECaseType type = cases[x, y].caseType;
                        if (counts.ContainsKey(type))
                            counts[type]++;
                        else
                            counts.Add(type, 1);
                    }
                    else if (borderCount)
                    {
                        if (counts.ContainsKey(ECaseType.Rock))
                            counts[ECaseType.Rock]++;
                        else
                            counts.Add(ECaseType.Rock, 1);
                    }
                        
                }
            }
        }

        ECaseType mostType = ECaseType.Empty;
        foreach (KeyValuePair<ECaseType, int> kp in counts)
            if (!counts.ContainsKey(mostType) || kp.Value > counts[mostType])
                mostType = kp.Key;
        return new KeyValuePair<ECaseType,int>(mostType, counts[mostType]);
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
