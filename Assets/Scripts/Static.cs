using UnityEngine;
using System.Collections;

public class Static {

    public static Texture basic_texture = Resources.Load("Blank") as Texture;

    static public GameObject FindChild(GameObject parent, string obj_name)
    {
        foreach (Transform obj in parent.GetComponentsInChildren<Transform>())
            if (obj.name == obj_name)
                return obj.gameObject;
        Debug.Log("Can't find object " + obj_name + " in " + parent.name);
        return null;
    }
}
