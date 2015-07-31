using UnityEngine;
using System.Collections;

[System.Serializable]
public class ResourceStock
{
    public EResourceType resourceType;
    public int maxStock;
    public int stock { get; set; }
}