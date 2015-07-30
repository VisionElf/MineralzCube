using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

    //UNITY PROPERTIES

    public GameObject startingUnit;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Map.instance.GenerateMap();
        }
    }

    public void CreateStartingUnits(Case c)
    {
        GameObject obj = GameObject.Instantiate(startingUnit);
        obj.transform.position = c.position;

    }
}
