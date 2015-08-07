using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Builder : Unit {

    public int harvestQuantity;
    public float harvestSpeed;
    public float harvestRange;

    public float buildingRange;
    public List<Building> buildingList;

    public void OrderHarvestResource(Resource resource)
    {
        StopMove();
        HarvestResource(resource);
    }
    public void HarvestResource(Resource resource)
    {
        StopCoroutine("Harvest");
        StartCoroutine("Harvest", resource);
    }
    IEnumerator Harvest(Resource resource)
    {
        while (!resource.IsEmpty())
        {
            if (Vector3.Distance(transform.position, resource.transform.position) <= (harvestRange + resource.radius + radius))
            {
                StopMove();
                LookAt(resource.transform.position);
                resource.Harvest(harvestQuantity);
                yield return new WaitForSeconds(harvestSpeed);
            }
            else if (!isMoving)
                MoveTo(resource);
            else
                yield return null;
        }
    }

    Building buildingToBuild;
    public void OrderBuildBuilding(Building building, Vector3 position)
    {
        StopMove();
        BuildBuilding(building, position);
    }
    public void BuildBuilding(Building building, Vector3 position)
    {
        buildingToBuild = building;
        StopCoroutine("Building");
        StartCoroutine("Building", position);
    }
    IEnumerator Building(Vector3 position)
    {
        while (Vector3.Distance(transform.position, position) > (buildingRange + buildingToBuild.radius + radius))
        {
            if (!isMoving)
                MoveTo(position);
            else
                yield return null;
        }

        GameObject obj = GameObject.Instantiate(buildingToBuild.gameObject);
        obj.transform.position = position;
        Map.instance.OnStartBuild();

        Building building = obj.GetComponent<Building>();
        building.StartBuilding();
    }
}
