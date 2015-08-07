using UnityEngine;
using System.Collections;

public class Building : Entity {

    public int resourceCost;
    public float buildingTime;

    public float buildingHeight;

    float progress;

    public void StartBuilding()
    {
        StopCoroutine("Build");
        StartCoroutine("Build");
    }

    IEnumerator Build()
    {
        progress = 0;
        while (progress < buildingTime)
        {
            progress += Time.deltaTime;
            model.transform.localPosition = new Vector3(0, Mathf.Min((progress / buildingTime) * buildingHeight - buildingHeight, 0) + 1, 0);
            yield return null;
        }
    }
}
