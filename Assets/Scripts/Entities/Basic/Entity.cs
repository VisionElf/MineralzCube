using UnityEngine;
using System.Collections;

public class Entity : MonoBehaviour {

    //FUNCTIONS
    public void DisableCollider()
    {
        foreach (Collider collider in GetComponentsInChildren<Collider>())
            if (collider != null)
                GetComponentInChildren<Collider>().enabled = false;
    }
    public void RemoveObject()
    {
        RemovePathMap();
        GameObject.Destroy(gameObject);
    }

    bool pathMapApplied;
    public bool PathMapApplied() { return pathMapApplied; }
    public void ApplyPathMap()
    {
        pathMapApplied = true;
        foreach (Collider collider in GetComponentsInChildren<Collider>())
            if (collider != null)
                GetComponentInChildren<Collider>().enabled = true;
        Grid.instance.RefreshGrid(transform.position, basicProperties.radius);
        Pathfinding.instance.RefreshCache();
    }
    public void RemovePathMap()
    {
        pathMapApplied = false;
        foreach (Collider collider in GetComponentsInChildren<Collider>())
            if (collider != null)
                GetComponentInChildren<Collider>().enabled = false;
        Grid.instance.RefreshGrid(transform.position, basicProperties.radius);
        Pathfinding.instance.RefreshCache();
    }

    //PROPERTIES

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

    //ATTACK
    public bool CanAttack()
    {
        return GetComponent<AttackEntity>() != null;
    }
    public AttackEntity attackProperties
    {
        get { return GetComponent<AttackEntity>(); }
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
