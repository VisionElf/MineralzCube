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
    }

    public DepotEntity GetDepot()
    {
        return mainBase.depotProperties;
    }


    List<float> fpsList = new List<float>();

    void Update()
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
    void OnGUI()
    {
        float fps = 1 / Time.deltaTime;
        fpsList.Add(fps);
        if (fpsList.Count > 10000)
            fpsList.RemoveAt(0);
        float avgFps = 0;
        foreach (float f in fpsList)
            avgFps += f;
        avgFps /= fpsList.Count;

        GUI.Label(new Rect(0, 0, 200, 100), "FPS: " + Math.Round(fps, 2));
        GUI.Label(new Rect(0, 20, 200, 100), "Average FPS: " + Math.Round(avgFps, 2));
    }
}
