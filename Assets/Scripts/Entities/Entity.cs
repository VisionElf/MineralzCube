using UnityEngine;
using System.Collections;

public class Entity : MonoBehaviour {
    public void RemoveObject()
    {
        GameObject.Destroy(this);
    }
}
