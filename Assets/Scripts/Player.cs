using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;

public class Player : MonoBehaviour {

    //UNITY PROPERTIES
    public Camera playerCamera;
    public GameObject startingUnit;

    //PROPERTIES
    MainBaseEntity mainBase;

    public void CreateStartingUnits(Case c)
    {
        GameObject obj = GameObject.Instantiate(startingUnit);
        obj.transform.position = c.position;
        mainBase = obj.GetComponent<MainBaseEntity>();
        mainBase.basicProperties.owner = this;

        playerCamera.GetComponent<CameraController>().PanCamera(obj.transform.position);
        OnGameStarted();
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


    List<float> fpsList = new List<float>();

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
                        mainBase.OrderHarvest(hitInfo.collider.GetComponent<HarvestableEntity>());
                    }
                }
            }
        }
    }
    void UpdateKeyboardKeys()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            mainBase.CreateWorkers(5);
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
