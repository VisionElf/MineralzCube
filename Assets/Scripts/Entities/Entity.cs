using UnityEngine;
using System.Collections;

public class Entity : MonoBehaviour {

    //FUNCTIONS
    public void RemoveObject()
    {
        GameObject.Destroy(gameObject);
    }

    //BASIC
    public bool IsBasic()
    {
        return GetComponent<BasicEntity>() != null;
    }
    public BasicEntity basicProperties
    {
        get {
            BasicEntity temp = GetComponent<BasicEntity>();
            if (temp == null)
                print(this.name + " has no basic properties");
            return temp; }
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
    public DepotEntity depotProperties
    {
        get { return GetComponent<DepotEntity>(); }
    }

    //MOVABLE
    public bool IsMovable()
    {
        return GetComponent<MovableEntity>() != null;
    }
    public MovableEntity movableProperties
    {
        get { return GetComponent<MovableEntity>(); }
    }

    //MOVABLE
    public bool IsHarvester()
    {
        return GetComponent<HarvesterEntity>() != null;
    }
    public HarvesterEntity harvesterProperties
    {
        get { return GetComponent<HarvesterEntity>(); }
    }
}
