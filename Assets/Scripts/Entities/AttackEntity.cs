using UnityEngine;
using System.Collections;

public class AttackEntity : Entity {

    //UNITY PROPERTIES
    public float attackDamage;
    public float attackRange;
    public float attackSpeed;
    public float scanRange;

    //PROPERTIES
    HealthEntity targetEntity;

    public void AttackEntity(Entity entity)
    {
        if (entity.HaveHealth())
        {
            targetEntity = entity.healthProperties;
        }
    }

    IEnumerator ScanTarget()
    {
        HealthEntity potentialTarget = null;
        float minDistance = 0;
        foreach (HealthEntity entity in (HealthEntity[])GameObject.FindObjectsOfType(typeof(HealthEntity)))
        {
            float distance = Vector3.Distance(entity.transform.position, transform.position);
            if (distance <= scanRange)
            {
                if (potentialTarget == null)
                {
                    potentialTarget = entity;
                    minDistance = distance;
                }
            }
        }

        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator Attack()
    {
        yield return null;
    }

    
}
