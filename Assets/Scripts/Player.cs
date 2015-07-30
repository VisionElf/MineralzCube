using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;

public class Player : MonoBehaviour {

    //UNITY PROPERTIES

    public CameraController camera;
    public GameObject startingUnit;

    public void CreateStartingUnits(Case c)
    {
        GameObject obj = GameObject.Instantiate(startingUnit);
        obj.transform.position = c.position;

        camera.PanCamera(obj.transform.position);
    }


    List<float> fpsList = new List<float>();
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
