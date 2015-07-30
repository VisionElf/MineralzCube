using UnityEngine;
using System.Collections;

public class Entity : MonoBehaviour {

    //FUNCTIONS
    public void RemoveObject()
    {
        GetComponentInChildren<Collider>().enabled = false;
        Grid.instance.RefreshGrid(transform.position, basicProperties.radius);
        GameObject.Destroy(gameObject);
    }

    public GameObject FindChild(string obj_name)
    {
        foreach (Transform obj in gameObject.GetComponentsInChildren<Transform>())
            if (obj.name == obj_name)
                return obj.gameObject;
        print("Can't find object " + obj_name + " in " + name);
        return null;
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
