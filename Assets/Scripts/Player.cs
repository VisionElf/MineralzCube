using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;

public class Player : MonoBehaviour {

    //UNITY PROPERTIES
    public Camera playerCamera;
    public GameObject startingUnit;

    public List<BuildingEntity> buildList;
    public GameObject previewBuildObject;
    KeyCode[] buildShortcuts = new KeyCode[] { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7 };

    //PROPERTIES
    public MainBaseEntity mainBase { get; set; }
    List<DepotEntity> depots = new List<DepotEntity>();

    BuildingEntity currentBuild;
    GameObject previewBuild;

    public void CreateStartingUnits(Vector3 position)
    {
        GameObject obj = GameObject.Instantiate(startingUnit);
        mainBase = obj.GetComponent<MainBaseEntity>();
        obj.transform.position = Map.instance.SnapToGrid(position, mainBase.buildingProperties);
        mainBase.basicProperties.owner = this;
        mainBase.buildingProperties.OnBuildFinish();

        playerCamera.GetComponent<CameraController>().PanCamera(obj.transform.position);
        depots.Add(mainBase.depotProperties);
    }

    public void OnGameStarted()
    {
        Other.gameStarted = true;
        GameObject.Find("LoadingScreen").SetActive(false);
    }

    public DepotEntity GetNearestDepotNotEmpty(Vector3 position)
    {
        DepotEntity nearestDepot = null;
        float minDistance = 0f;
        foreach (DepotEntity depot in depots)
        {
            if (depot.buildingProperties.isBuilt && !depot.IsEmpty() && Pathfinding.instance.PathExists(depot.transform.position, position, 0f))
            {
                float distance = Vector3.Distance(depot.transform.position, position);
                if (distance < minDistance || nearestDepot == null)
                {
                    nearestDepot = depot;
                    minDistance = distance;
                }
            }
        }
        return nearestDepot;
    }
    public DepotEntity GetNearestDepotNotFull(Vector3 position)
    {
        DepotEntity nearestDepot = null;
        float minDistance = 0f;
        foreach (DepotEntity depot in depots)
        {
            if (depot.buildingProperties.isBuilt && !depot.IsFull() && Pathfinding.instance.PathExists(depot.transform.position, position, 0f))
            {
                float distance = Vector3.Distance(depot.transform.position, position);
                if (distance < minDistance || nearestDepot == null)
                {
                    nearestDepot = depot;
                    minDistance = distance;
                }
            }
        }
        return nearestDepot;
    }

    public int GetAvailableResources()
    {
        int count = 0;
        foreach (DepotEntity depot in depots)
            count += depot.resourceContainer.GetAllResourcesStock();
        return count;
    }


    public void PlaceBuilding(BuildingEntity building)
    {
        currentBuild = building;
        if (previewBuild != null)
            GameObject.Destroy(previewBuild);
        previewBuild = GameObject.Instantiate(previewBuildObject);
        previewBuild.transform.localScale = new Vector3(building.caseSizeX, 1, building.caseSizeY);
        StopCoroutine("UpdateBuild");
        StartCoroutine("UpdateBuild");
    }
    public void StartBuild()
    {
        if (currentBuild != null)
        {
            GameObject buildingObj = GameObject.Instantiate(currentBuild.gameObject);
            BuildingEntity building = buildingObj.GetComponent<BuildingEntity>();
            building.transform.position = previewBuild.transform.position;
            building.basicProperties.owner = this;
            building.StartBuild();
            if (building.HaveDepot())
                depots.Add(building.depotProperties);
            mainBase.AssignTask(new BuildTask(building));
            currentBuild = null;
        }
    }

    List<float> fpsList = new List<float>();

    IEnumerator UpdateBuild()
    {
        Vector3 mousePosition = new Vector3();
        RaycastHit hit;
        bool canBuild = true;

        while (!Input.GetMouseButtonDown(0) && currentBuild != null)
        {
            if (GameObject.Find("Grid").GetComponent<Collider>().Raycast(playerCamera.ScreenPointToRay(Input.mousePosition), out hit, 500f))
                mousePosition = hit.point;
            previewBuild.transform.position = Map.instance.SnapToGrid(mousePosition, currentBuild);

            canBuild = true;
            previewBuild.GetComponentInChildren<Dummy>().ResetColor();
            Vector3 pos = previewBuild.transform.position - new Vector3(currentBuild.caseSizeX, 0, currentBuild.caseSizeY) * Grid.instance.nodeSize / 4 + Vector3.up;
            showPos = pos;
            for (int i = 0; i < currentBuild.caseSizeX; i++)
            {
                for (int j = 0; j < currentBuild.caseSizeY; j++)
                {
                    if (Physics.Raycast(pos + new Vector3(i, 0, j) * Grid.instance.nodeSize, Vector3.down, out hit, 50f, Grid.instance.mask))
                    {
                        canBuild = false;
                        previewBuild.GetComponentInChildren<Dummy>().SetColor(1f, 0f, 0f);
                        print("hit: " + hit.collider);
                        break;
                    }
                }
                if (!canBuild)
                    break;
            }
            
            if (Input.GetMouseButtonDown(1))
                currentBuild = null;
            yield return null;
        }

        StartBuild();
        GameObject.Destroy(previewBuild);
        previewBuild = null;
    }

    Vector3 showPos;

    public void OnResourcesChanged()
    {
        mainBase.NotifyWorkers();
    }

    void UpdateMouseButtons()
    {
        if (Input.GetMouseButton(1))
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(playerCamera.ScreenPointToRay(Input.mousePosition), out hitInfo))
            {
                if (hitInfo.collider.tag == "Terrain")
                {
                    if (mainBase != null)
                    {
                        ResourceEntity resource = hitInfo.collider.GetComponent<ResourceEntity>();
                        mainBase.AssignTask(new HarvestTask(resource));
                    }
                }
            }
        }
    }
    void UpdateKeyboardKeys()
    {
        if (Input.GetKeyDown(KeyCode.W))
            mainBase.CreateWorkers(1);

        if (Input.GetKeyDown(KeyCode.F1))
            Other.showDebug = !Other.showDebug;

        for (int i = 0; i < buildList.Count; i++)
            if (Input.GetKeyDown(buildShortcuts[i]))
                PlaceBuilding(buildList[i]);
    }
    void Update()
    {
        UpdateMouseButtons();
        UpdateKeyboardKeys();
    }

    void OnDrawGizmos()
    {
        if (showPos != null && currentBuild != null)
        {
            Gizmos.color = Color.red;
            for (int i = 0; i < currentBuild.caseSizeX; i++)
                for (int j = 0; j < currentBuild.caseSizeY; j++)
                    Gizmos.DrawCube(showPos + new Vector3(i, 0, j) * Grid.instance.nodeSize, Vector3.one * 0.9f);
        }
    }
    void OnGUI()
    {
        if (Other.gameStarted && Other.showDebug)
        {
            float fps = 1 / Time.deltaTime;
            fpsList.Add(fps);
            if (fpsList.Count > 10000)
                fpsList.RemoveAt(0);
            float avgFps = 0;
            foreach (float f in fpsList)
                avgFps += f;
            avgFps /= fpsList.Count;

            int textWidth = 500;

            GUI.color = Color.black;
            int y = 0;
            GUI.Label(new Rect(0, y, textWidth, 100), "FPS: " + Math.Round(fps, 2));
            y += 20;
            GUI.Label(new Rect(0, y, textWidth, 100), "Average FPS: " + Math.Round(avgFps, 2));
            y += 20;
            GUI.Label(new Rect(0, y, textWidth, 100), "Pathfinding Count: " + Pathfinding.instance.callCount + " - " + Pathfinding.instance.cacheCallCount +
                " (" + Pathfinding.instance.cacheCount + " cached), avg= " + Mathf.Round(Pathfinding.instance.GetAverageTime()) + "ms");
            y += 20;
            GUI.Label(new Rect(0, y, textWidth, 100), "Task Count: " + mainBase.GetTaskCount());
            y += 20;

            int size = 1;
            int width = 300;
            int height = 120;
            Vector2 fpsPos = new Vector2(Screen.width - width, Screen.height - height);
            GUI.color = new Color(0, 0, 0, 0.8f);
            GUI.DrawTexture(new Rect(fpsPos.x, fpsPos.y, width * size, height * size), Static.basic_texture);
            for (int i = Math.Max(0, fpsList.Count - (width / size)); i < fpsList.Count; i++)
            {
                GUI.color = Color.white;
                GUI.DrawTexture(new Rect(fpsPos.x + (fpsList.Count - i) * size, fpsPos.y + height - (fpsList[i] * size), size, size), Static.basic_texture);
            }

            //DrawMinimap();
        }
    }

    MinimapItem[,] minimap;

    void DrawMinimap()
    {
        int width = Map.instance.mapSizeX;
        int height = Map.instance.mapSizeY;
        Vector2 position = new Vector2(0, Screen.height - height);
        Rect rect = new Rect(position.x, position.y, width, height);

        if (minimap == null)
        {
            Map map = Map.instance;

            float sizeX = width * map.caseSize / map.mapSizeX;
            float sizeY = height * map.caseSize / map.mapSizeY;

            minimap = new MinimapItem[map.mapSizeX, map.mapSizeY];
            for (int x = 0; x < map.mapSizeX; x++)
            {
                for (int y = 0; y < map.mapSizeY; y++)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(Map.instance.GetPositionAt(x, y) + Vector3.up * 5f, Vector3.down, out hit, 10))
                    {
                        GameObject obj = hit.collider.gameObject;
                        Entity ent = obj.GetComponent<Entity>();
                        if (ent != null)
                            GUI.color = Color.white;
                        else
                            GUI.color = Color.green;

                        minimap[x, y] = new MinimapItem(GUI.color, new Rect(rect.x + x * sizeX, rect.y + y * sizeY, sizeX, sizeY));
                    }
                }
            }
        }

        GUI.color = Color.black;
        GUI.DrawTexture(rect, Static.basic_texture);
        foreach (MinimapItem c in minimap)
        {
            GUI.color = c.color;
            GUI.DrawTexture(c.rect, Static.basic_texture);
        }
    }
}

public class MinimapItem
{
    public Color color;
    public Rect rect;
    public MinimapItem(Color c, Rect r)
    {
        color = c;
        rect = r;
    }
}
