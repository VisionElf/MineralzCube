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
    MainBaseEntity mainBase;

    BuildingEntity currentBuild;
    GameObject previewBuild;

    public void CreateStartingUnits(Case c)
    {
        GameObject obj = GameObject.Instantiate(startingUnit);
        mainBase = obj.GetComponent<MainBaseEntity>();
        obj.transform.position = Map.instance.SnapToGrid(c.position, mainBase.buildingProperties);
        mainBase.basicProperties.owner = this;

        playerCamera.GetComponent<CameraController>().PanCamera(obj.transform.position);
    }

    public void OnGameStarted()
    {
        Other.gameStarted = true;
        GameObject.Find("LoadingScreen").SetActive(false);
    }

    public DepotEntity GetDepot()
    {
        return mainBase.depotProperties;
    }


    public void StartBuild(BuildingEntity building)
    {
        currentBuild = building;
        if (previewBuild != null)
            GameObject.Destroy(previewBuild);
        previewBuild = GameObject.Instantiate(previewBuildObject);
        StopCoroutine("UpdateBuild");
        StartCoroutine("UpdateBuild");
    }

    List<float> fpsList = new List<float>();

    IEnumerator UpdateBuild()
    {
        Vector3 mousePosition = new Vector3();
        RaycastHit hit;
        while (!Input.GetMouseButtonDown(0) && currentBuild != null)
        {
            if (GameObject.Find("Grid").GetComponent<Collider>().Raycast(playerCamera.ScreenPointToRay(Input.mousePosition), out hit, 50f))
                mousePosition = hit.point;
            previewBuild.transform.position = Map.instance.SnapToGrid(mousePosition, currentBuild);
            if (Input.GetMouseButtonDown(1))
                currentBuild = null;
            yield return null;
        }

        if (currentBuild != null)
        {
            GameObject buildingObj = GameObject.Instantiate(currentBuild.gameObject);
            BuildingEntity building = buildingObj.GetComponent<BuildingEntity>();
            building.transform.position = previewBuild.transform.position;
            building.StartBuild();
            mainBase.AssignTask(new BuildTask(building));
        }

        currentBuild = null;
        GameObject.Destroy(previewBuild);
        previewBuild = null;
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
            mainBase.CreateWorkers(5);

        for (int i = 0; i < buildList.Count; i++)
            if (Input.GetKeyDown(buildShortcuts[i]))
                StartBuild(buildList[i]);
    }
    void Update()
    {
        UpdateMouseButtons();
        UpdateKeyboardKeys();
    }

    void OnGUI()
    {
        if (Other.gameStarted)
        {
            float fps = 1 / Time.deltaTime;
            fpsList.Add(fps);
            if (fpsList.Count > 10000)
                fpsList.RemoveAt(0);
            float avgFps = 0;
            foreach (float f in fpsList)
                avgFps += f;
            avgFps /= fpsList.Count;

            int y = 0;
            GUI.Label(new Rect(0, y, 200, 100), "FPS: " + Math.Round(fps, 2));
            y += 20;
            GUI.Label(new Rect(0, y, 200, 100), "Average FPS: " + Math.Round(avgFps, 2));
            y += 20;
            GUI.Label(new Rect(0, y, 200, 100), "Pathfinding Count: " + Pathfinding.instance.callCount);
            y += 20;
            GUI.Label(new Rect(0, y, 200, 100), "Task Count: " + mainBase.GetTaskCount());
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
