using UnityEngine;
using System.Collections;

public class Entity : MonoBehaviour {

    //UNITY PROPERTIES
    public float radius;

    //FUNCTIONS
    public void RemoveObject()
    {
        GameObject.Destroy(this);
    }

    //HEALTH
    public bool HaveHealth()
    {
        return GetComponent<HealthEntity>() != null;
    }
    public HealthEntity healthProperties
    {
        get { return GetComponent<HealthEntity>(); }
    }

    //DEPOT
    public bool HaveDepot()
    {
        return GetComponent<DepotEntity>() != null;
    }
    public DepotEntity depotProperties()
    {
        return GetComponent<DepotEntity>();
    }
}
