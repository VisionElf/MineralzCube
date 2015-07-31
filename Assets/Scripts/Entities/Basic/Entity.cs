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

    //PROPERTIES
    public Color color
    {
        get { return GetComponentInChildren<Renderer>().material.color; }
        set { GetComponentInChildren<Renderer>().material.color = value; }
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

    //HARVESTER
    public bool IsWorker()
    {
        return GetComponent<WorkerEntity>() != null;
    }
    public WorkerEntity workerProperties
    {
        get { return GetComponent<WorkerEntity>(); }
    }

    //BUILDING
    public bool IsBuilding()
    {
        return GetComponent<BuildingEntity>() != null;
    }
    public BuildingEntity buildingProperties
    {
        get { return GetComponent<BuildingEntity>(); }
    }

    //HARVESTABLE
    public bool IsResource()
    {
        return GetComponent<ResourceEntity>() != null;
    }
    public ResourceEntity resourceProperties
    {
        get { return GetComponent<ResourceEntity>(); }
    }
}
