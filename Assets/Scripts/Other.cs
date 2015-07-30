﻿using UnityEngine;
using System.Collections;

public class Other {

    static System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
    static float deltaTime;
    static string oldStep;

    static public void StartStep(string step)
    {
        sw.Start();
        Debug.Log(step + "...");
        oldStep = step;
    }

    static public void NextStep(string step)
    {
        deltaTime = sw.ElapsedMilliseconds - deltaTime;
        Debug.Log(oldStep + " done in " + deltaTime + " ms");
        Debug.Log(step + "...");
        oldStep = step;
    }

    static public void StopStep(string step)
    {
        sw.Stop();
        deltaTime = sw.ElapsedMilliseconds - deltaTime;
        Debug.Log(oldStep + " done in " + deltaTime + " ms");
        Debug.Log(step + " total done in " + sw.ElapsedMilliseconds + " ms");
    }

}