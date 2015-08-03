using UnityEngine;
using System.Collections;

[System.Serializable]
public class ResourceCost {

    //UNITY PROPERTIES
    public int cost;
    public EResourceType resourceType;

    //PROPERTY
    public int buildResources { get; set; }

    public bool IsFull()
    {
        return buildResources >= cost;
    }
}
