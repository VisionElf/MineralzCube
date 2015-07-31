using UnityEngine;
using System.Collections;

public class Other {

    static public bool print = true;
    static public int loading;
    static public int maxLoading;
    static public bool gameStarted;

    static System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
    static float deltaTime;
    public static string oldStep;

    static public void StartStep(string step, int _loading, int _maxLoading)
    {
        maxLoading = _maxLoading;
        loading = _loading;
        sw.Start();
        if (print)
            Debug.Log(step + "...");
        oldStep = step;
    }

    static public void NextStep(string step, int _loading)
    {
        loading += _loading;
        deltaTime = sw.ElapsedMilliseconds - deltaTime;
        if (print)
        {
            Debug.Log(oldStep + " done in " + deltaTime + " ms");
            Debug.Log(step + "...");
        }
        oldStep = step;
    }

    static public void StopStep(string step, int _loading)
    {
        loading += _loading;
        sw.Stop();
        deltaTime = sw.ElapsedMilliseconds - deltaTime;
        if (print)
        {
            Debug.Log(oldStep + " done in " + deltaTime + " ms");
            Debug.Log(step + " total done in " + sw.ElapsedMilliseconds + " ms");
        }
        
    }

}
