using UnityEngine;
using System.Collections;

public class Entity : MonoBehaviour {

    //FUNCTIONS
    public void RemoveObject()
    {
        Map.instance.RemoveEntityFromMap(this);
        RemovePathMap();
        Destroy(gameObject);
    }

    protected bool pathMapApplied = true;
    public void ApplyPathMap()
    {
        if (!pathMapApplied)
        {
            pathMapApplied = true;
            foreach (Collider collider in GetComponentsInChildren<Collider>())
                if (collider != null)
                    GetComponentInChildren<Collider>().enabled = true;
            Grid.instance.RefreshGrid(transform.position, basicProperties.radius);
        }
    }
    public void RemovePathMap()
    {
        if (pathMapApplied)
        {
            pathMapApplied = false;
            foreach (Collider collider in GetComponentsInChildren<Collider>())
                if (collider != null)
                    GetComponentInChildren<Collider>().enabled = false;
            Grid.instance.RefreshGrid(transform.position, basicProperties.radius);
        }
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
    public bool HasHealth()
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
    public bool HasDepot()
    {
        return GetComponent<DepotEntity>() != null;
    }
    public DepotEntity depotProperties
    {
        get { return GetComponent<DepotEntity>(); }
    }

    //MAIN BASE
    public bool IsMainBase()
    {
        return GetComponent<MainBaseEntity>() != null;
    }
    public MainBaseEntity mainBaseProperties
    {
        get { return GetComponent<MainBaseEntity>(); }
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

    //ENERGY
    public bool HasEnergy()
    {
        return GetComponent<EnergyEntity>() != null;
    }
    public EnergyEntity energyProperties
    {
        get { return GetComponent<EnergyEntity>(); }
    }

    //GENERATOR
    public bool CanGenerate()
    {
        return GetComponent<GeneratorEntity>() != null;
    }
    public GeneratorEntity generatorProperties
    {
        get { return GetComponent<GeneratorEntity>(); }
    }
}
