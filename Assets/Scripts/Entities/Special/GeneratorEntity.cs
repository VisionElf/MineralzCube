using UnityEngine;
using System.Collections;

public class GeneratorEntity : Entity {

    //UNITY PROPERTIES
    public float energyGenerated;
    public float generationSpeed;
    public float generationRange;

    //SCAN
    public bool allowScan;
    [Range(0.5f, 5f)]
    public float scanSpeed;

    public Transform rayonOrigin;
    public int rayonCount;
    public float rayonDispersion;

    //PROPERTY
    EnergyEntity targetEntity;
    LineRenderer lineRenderer { get { return GetComponent<LineRenderer>(); } }

    ProjectileEntity dummyProjectile;

    void Awake()
    {
        ResetLineRenderer();
    }

    public bool GiveEnergy(float quantity, EnergyEntity target)
    {
        target.AddEnergy(quantity);
        return target != null && !target.HasMaxEnergy();
    }

    IEnumerator Generate()
    {
        while (GiveEnergy(energyGenerated, targetEntity))
        {
            float lineTime = 0.5f;
            ApplyLineRenderer();
            yield return new WaitForSeconds(lineTime);
            ResetLineRenderer();
            yield return new WaitForSeconds(generationSpeed - lineTime);
        }

        targetEntity = null;

        StartScan();
    }

    public void SetTarget(EnergyEntity target)
    {
        StopAllCoroutines();
        targetEntity = target;
        StartCoroutine("Generate");
    }

    public void ApplyLineRenderer()
    {
        if (lineRenderer != null && targetEntity != null)
        {
            Vector3 start = rayonOrigin.position;
            Vector3 end = targetEntity.basicProperties.model.origin.position;
            Vector3 direction = (end - start).normalized;
            float norm = Vector3.Distance(start, end) / rayonCount;

            lineRenderer.SetVertexCount(rayonCount + 1);

            Vector3 vector = start;
            for (int i = 0; i < rayonCount; i++)
            {
                vector.y = start.y + Random.Range(-rayonDispersion, rayonDispersion);
                lineRenderer.SetPosition(i, vector);
                direction = (end - vector).normalized;
                vector += direction * norm;
            }
            lineRenderer.SetPosition(rayonCount, end);
        }

    }
    public void ResetLineRenderer()
    {
        if (lineRenderer != null)
        {
            lineRenderer.SetVertexCount(2);
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, transform.position);
        }
    }

    public EnergyEntity GetClosestTarget()
    {
        EnergyEntity closestTarget = null;
        float minDistance = 0f;
        foreach (EnergyEntity target in Map.instance.energyUnitList)
        {
            if (target != null && (!target.IsBuilding() || target.buildingProperties.isBuilt) && !target.HasMaxEnergy() && basicProperties.CanReach(target, generationRange))
            {
                float distance = Vector3.Distance(target.transform.position, transform.position);
                if (distance < minDistance || closestTarget == null)
                {
                    closestTarget = target;
                    minDistance = distance;
                }
            }
        }
        return closestTarget;
    }

    public void StartScan()
    {
        StopAllCoroutines();
        if (allowScan)
            StartCoroutine("Scan");
    }
    public void StopScan()
    {
        StopCoroutine("Scan");
    }
    IEnumerator Scan()
    {
        while (targetEntity == null)
        {
            targetEntity = GetClosestTarget();
            if (targetEntity != null)
                break;
            yield return new WaitForSeconds(scanSpeed);
        }

        SetTarget(targetEntity);
    }
}
