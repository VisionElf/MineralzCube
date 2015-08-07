using UnityEngine;
using System.Collections;
using System;

public class Map : MonoBehaviour {
    
    //PUBLIC
    public float mapWidth;
    public float mapHeight;

    public Vector3 mapSize { get { return new Vector3(mapWidth, 0, mapHeight); } }

    public Builder builder;
    public Material previewMaterial;

    static public Map instance;

    void Start()
    {
        instance = this;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            OnLeftMouseButton(GetHitAtMousePosition());
        else if (Input.GetMouseButtonDown(1))
            OnRightMouseButton(GetHitAtMousePosition());

        if (Input.GetKeyDown(KeyCode.Alpha1))
            StartPlaceBuilding(builder.buildingList[0]);
    }

    GameObject previewBuilding;
    Building buildingToBuild;
    public void StartPlaceBuilding(Building building)
    {
        if (previewBuilding != null)
            GameObject.Destroy(previewBuilding);
        previewBuilding = GameObject.Instantiate(building.model.gameObject);
        previewBuilding.GetComponentInChildren<Renderer>().material = previewMaterial;
        buildingToBuild = building;
        StopCoroutine("PlaceBuilding");
        StartCoroutine("PlaceBuilding");
    }

    public void OnStartBuild()
    {
        GameObject.Destroy(Map.instance.previewBuilding);
        previewBuilding = null;
    }
    IEnumerator PlaceBuilding()
    {
        while (buildingToBuild != null)
        {
            RaycastHit hit;
            if (GetComponent<Collider>().Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100f))
                previewBuilding.transform.position = hit.point;
            yield return null;
        }
    }

    public void OnLeftMouseButton(Nullable<RaycastHit> hit)
    {
        if (builder != null && buildingToBuild != null)
        {
            builder.BuildBuilding(buildingToBuild, previewBuilding.transform.position);
            buildingToBuild = null;
        }
    }
    public void OnRightMouseButton(Nullable<RaycastHit> hit)
    {
        if (hit != null && builder != null)
        {
            Resource resource;
            if (hit.Value.collider.name == "Map")
                builder.OrderMoveTo(hit.Value.point);
            else if ((resource = hit.Value.collider.GetComponent<Resource>()) != null)
                builder.OrderHarvestResource(resource);
        }
    }

    public Nullable<RaycastHit> GetHitAtMousePosition()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
            return hit;
        return null;
    }

    //GIZMOS
    void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position, mapSize);
    }
}
