using UnityEngine;
using System.Collections;

public class LightController : MonoBehaviour {

    //UNITY PROPERTIES
    public bool enableDayNightCycle;
    public bool enableVisualDayNight;

    public float dayTime;
    public Color dayColor;
    public Vector3 dayRotation;

    public float nightTime;
    public Color nightColor;
    public Vector3 nightRotation;

    static public LightController instance;

    //PROPERTIES
    bool day;
    float time;

    void Start()
    {
        time = 0f;
        day = true;
        instance = this;
    }

    public Color color
    {
        get { return GetComponent<Light>().color; }
        set { GetComponent<Light>().color = value; }
    }

    void OnDay()
    {
        Map.instance.DestroyCreeps();
    }

    void OnNight()
    {
        Map.instance.StartSpawnCreeps();
    }


    void Update()
    {
        if (Other.gameStarted && enableDayNightCycle)
        {
            if (day)
            {
                if (enableVisualDayNight)
                {
                    color = Color.Lerp(dayColor, nightColor, (time / dayTime));
                    transform.localRotation = Quaternion.Euler((Vector3.Lerp(dayRotation, nightRotation, (time / dayTime))));
                }

                if (time >= dayTime)
                {
                    time = 0f;
                    day = false;
                    OnNight();
                }
            }
            else
            {
                if (enableVisualDayNight)
                {
                    color = Color.Lerp(nightColor, dayColor, (time / nightTime));
                    transform.localRotation = Quaternion.Euler((Vector3.Lerp(nightRotation, dayRotation, (time / nightTime))));
                }

                if (time >= nightTime)
                {
                    time = 0f;
                    day = true;
                    OnDay();
                }
            }
            time += Time.deltaTime;
        }
    }

    void OnGUI()
    {
        if (Other.gameStarted)
        {
            GUI.color = Color.white;
            GUI.Label(new Rect(Screen.width / 2, 0, 300, 50), "time: " + Mathf.Round(time));
        }
    }
}
