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
    public bool IsHarvester()
    {
        return GetComponent<HarvesterEntity>() != null;
    }
    public HarvesterEntity harvesterProperties
    {
        get { return GetComponent<HarvesterEntity>(); }
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

    //BUILDER
    public bool IsBuilder()
    {
        return GetComponent<BuilderEntity>() != null;
    }
    public BuilderEntity builderProperties
    {
        get { return GetComponent<BuilderEntity>(); }
    }

    //HARVESTABLE
    public bool IsHarvestable()
    {
        return GetComponent<HarvestableEntity>() != null;
    }
    public HarvestableEntity harvestableProperties
    {
        get { return GetComponent<HarvestableEntity>(); }
    }
}
