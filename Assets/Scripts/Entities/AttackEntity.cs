using UnityEngine;
using System.Collections;

public class AttackEntity : Entity {

    //UNITY PROPERTIES
    public float attackDamage;
    public float attackRange;
    public float attackSpeed;

    public float scanRange;
    [Range(0.5f, 5f)]
    public float scanSpeed;

    public Entity projectileType;

    //PROPERTIES
    HealthEntity targetEntity;

    public bool Attack(Entity entity)
    {
        if (entity.HaveHealth())
            targetEntity = entity.healthProperties;
        if (targetEntity != null)
        {
            if (Vector3.Distance(entity.transform.position, transform.position) <= attackRange)
                StartAttackTarget();
            else
                targetEntity = null;

        }
        return targetEntity != null;
    }

    void StartAttackTarget()
    {
        StopCoroutine("AttackTarget");
        StartCoroutine("AttackTarget");
    }
    void StartScanTarget()
    {
        StopCoroutine("ScanTarget");
        StartCoroutine("ScanTarget");
    }

    void StopAttack()
    {
        StopAllCoroutines();
        targetEntity = null;
        StartScanTarget();
    }

    IEnumerator ScanTarget()
    {
        HealthEntity potentialTarget = null;
        while (potentialTarget == null)
        {
            float minDistance = 0;
            foreach (HealthEntity entity in (HealthEntity[])GameObject.FindObjectsOfType(typeof(HealthEntity)))
            {
                float distance = Vector3.Distance(entity.transform.position, transform.position);
                if (distance <= scanRange)
                {
                    if (potentialTarget == null || distance < minDistance)
                    {
                        potentialTarget = entity;
                        minDistance = distance;
                    }
                }
            }
            yield return new WaitForSeconds(scanSpeed);
        }
        Attack(potentialTarget);
    }
    IEnumerator AttackTarget()
    {
        while (targetEntity != null && (!targetEntity.HaveHealth() || !targetEntity.healthProperties.IsDead()))
        {
            if (Vector3.Distance(targetEntity.transform.position, transform.position) <= attackRange)
            {
                movableProperties.StopMove();
                LaunchProjectile(targetEntity);
            }
            else if (IsMovable())
                movableProperties.MoveToRangeEntity(targetEntity, attackRange);
            else
                break;
            yield return new WaitForSeconds(attackSpeed);
        }

        StopAttack();
    }

    void LaunchProjectile(HealthEntity target)
    {
        GameObject projectile = GameObject.Instantiate(projectileType.gameObject);
        //TODO: LAUNCH PROJECTILE
    }    
}
