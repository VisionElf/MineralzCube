using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;

public class Player : MonoBehaviour {

    //UNITY PROPERTIES
    public Camera playerCamera;
    public GameObject startingUnit;

    public List<BuildingEntity> buildList;

    public Material previewBuildMaterial;
    KeyCode[] buildShortcuts = new KeyCode[] { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7 };

    //PROPERTIES
    public MainBaseEntity mainBase { get; set; }
    List<DepotEntity> depots = new List<DepotEntity>();

    BuildingEntity currentBuild;
    Dummy previewBuild;

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
        GameObject loadingScreen = GameObject.Find("LoadingScreen");
        if (loadingScreen != null)
            loadingScreen.SetActive(false);
    }

    public DepotEntity GetNearestDepotNotEmpty(Entity entity, EResourceType resourceType)
    {
        DepotEntity nearestDepot = null;
        float minDistance = 0f;
        foreach (DepotEntity depot in depots)
        {
            if (depot.buildingProperties.isBuilt && !depot.IsEmpty(resourceType) && entity.basicProperties.CanReach(depot))
            {
                float distance = Vector3.Distance(depot.transform.position, entity.transform.position);
                if (distance < minDistance || nearestDepot == null)
                {
                    nearestDepot = depot;
                    minDistance = distance;
                }
            }
        }
        return nearestDepot;
    }
    public DepotEntity GetNearestDepotNotFull(Entity entity, EResourceType resourceType)
    {
        DepotEntity nearestDepot = null;
        float minDistance = 0f;
        foreach (DepotEntity depot in depots)
        {
            if (depot.buildingProperties.isBuilt && !depot.IsFull(resourceType) && entity.basicProperties.CanReach(depot))
            {
                float distance = Vector3.Distance(depot.transform.position, entity.transform.position);
                if (distance < minDistance || nearestDepot == null)
                {
                    nearestDepot = depot;
                    minDistance = distance;
                }
            }
        }
        return nearestDepot;
    }

    public int GetAvailableResources(EResourceType resourceType)
    {
        int count = 0;
        foreach (DepotEntity depot in depots)
            count += depot.resourceContainer.GetCurrentResourceStock(resourceType);
        return count;
    }

    public void PlaceBuilding(BuildingEntity building)
    {
        StopCoroutine("UpdateBuild");

        currentBuild = building;
        if (previewBuild != null)
            previewBuild.DestroyDummy();
        GameObject previewObj = GameObject.Instantiate(building.model);
        previewBuild = previewObj.GetComponent<Dummy>();
        previewBuild.defaultMaterial = previewBuildMaterial;
        previewBuild.material = previewBuildMaterial;

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


    IEnumerator UpdateBuild()
    {
        Vector3 mousePosition = new Vector3();
        RaycastHit hit;
        bool canBuild = true;

        while (currentBuild != null)
        {
            if (GameObject.Find("Grid").GetComponent<Collider>().Raycast(playerCamera.ScreenPointToRay(Input.mousePosition), out hit, 500f))
                mousePosition = hit.point;
            previewBuild.transform.position = Map.instance.SnapToGrid(mousePosition, currentBuild);

            canBuild = true;
            previewBuild.ResetColor();
            Vector3 pos = previewBuild.transform.position - new Vector3(currentBuild.caseSizeX, 0, currentBuild.caseSizeY) * Grid.instance.nodeSize / 4 + Vector3.up * 10f;
            for (int i = 0; i < currentBuild.caseSizeX; i++)
            {
                for (int j = 0; j < currentBuild.caseSizeY; j++)
                {
                    if (Physics.Raycast(pos + new Vector3(i, 0, j) * Grid.instance.nodeSize, Vector3.down, out hit, 20f, Grid.instance.mask))
                    {
                        canBuild = false;
                        previewBuild.SetColor(1f, 0f, 0f);                       
                        break;
                    }
                }
                if (!canBuild)
                    break;
            }
            
            if (Input.GetMouseButtonDown(0) && canBuild)
                break;
            else if (Input.GetMouseButtonDown(1))
                currentBuild = null;
            yield return null;
        }

        StartBuild();
        if (previewBuild != null)
            previewBuild.DestroyDummy();
        previewBuild = null;
    }

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
            {
                PlaceBuilding(buildList[i]);
                break;
            }
    }
    void Update()
    {
        UpdateMouseButtons();
        UpdateKeyboardKeys();

        UpdateFPS();
    }

    List<float> fpsList = new List<float>();
    float avgFps;
    void UpdateFPS()
    {
        float fps = 1 / Time.deltaTime;
        fpsList.Add(fps);
        if (fpsList.Count > 10000)
            fpsList.RemoveAt(0);

        avgFps = 0;
        foreach (float f in fpsList)
            avgFps += f;
        avgFps /= fpsList.Count;
    }

    void OnGUI()
    {
        if (Other.gameStarted)
        {
            DrawInterface();
            //DrawMinimap();

            if (Other.showDebug)
            {
                int textWidth = 500;
                GUI.color = Color.black;
                int y = 0;
                GUI.Label(new Rect(0, y, textWidth, 100), "FPS: " + Math.Round(fpsList[fpsList.Count - 1], 2));
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
            }
        }
    }

    void DrawInterface()
    {
        int iconSize = 64;
        int interval = 5;
        int totalDistance = (iconSize * buildList.Count) + (interval * (buildList.Count - 1));
        int startX = totalDistance / 2;

        Vector3 position = new Vector3(Screen.width / 2 - startX, Screen.height - iconSize);
        for (int i = 0; i < buildList.Count; i++)
        {
            GUI.color = Color.white;
            if (currentBuild == buildList[i])
                GUI.color = Color.green;
            Rect rect = new Rect(position.x, position.y, iconSize, iconSize);
            GUI.DrawTexture(rect, buildList[i].buildingIcon);

            GUIStyle style = new GUIStyle();
            style.font = Resources.Load("AGENCYB") as Font;
            style.fontSize = 14;
            style.normal.textColor = Color.white;

            GUIContent content = new GUIContent(buildList[i].name);
            Vector3 textSize = style.CalcSize(content);
            Rect textRect = new Rect(rect.x + (iconSize - textSize.x)  / 2, rect.y + iconSize - textSize.y, rect.width, rect.height);
            GUI.Label(textRect, content, style);

            GUIContent content2 = new GUIContent(buildShortcuts[i].ToString());
            //Vector3 textSize2 = style.CalcSize(content2);
            Rect textRect2 = new Rect(rect.x, rect.y, rect.width, rect.height);
            GUI.Label(textRect2, content2, style);

            position.x += iconSize + interval;
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
