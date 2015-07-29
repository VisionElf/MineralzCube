using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Map.instance.GenerateMap();
        }
    }
}
