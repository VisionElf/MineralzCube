using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ResourceStock
{
    //UNITY PROPERTIES
    public EResourceType resourceType;
    public int maxStock;
    public int stock { get; set; }

    public GameObject parent;
    public int dummyListStart;
    public int dummyListEnd;

    //PROPERTIES
    List<Dummy> dummyList;
    float dummyTotalSize;

    //FUNCTIONS
    public bool IsEmpty()
    {
        return stock <= 0;
    }

    //DUMMY FUNCTIONS
    public void CreateDummyList()
    {
        dummyList = new List<Dummy>();
        foreach (Transform obj in parent.transform)
        {
            Dummy dummy;
            if ((dummy = obj.GetComponent<Dummy>()) != null)
                dummyList.Add(dummy);
        }
        foreach (Dummy d in dummyList)
            dummyTotalSize += d.defaultScale.y * d.defaultScale.x * d.defaultScale.y;

        RefreshDummyList();
    }
    public void RefreshDummyList()
    {
        float percent = 0;
        if (maxStock > 0)
            percent = (float)stock / maxStock;
        foreach (Dummy d in dummyList)
        {
            float dp = (d.defaultScale.y * d.defaultScale.x * d.defaultScale.y) / dummyTotalSize;

            d.ScaleY(Mathf.Min(percent / dp, 1));
            percent = Mathf.Max(percent - dp, 0);
        }
    }

}