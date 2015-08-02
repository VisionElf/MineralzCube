using UnityEngine;
using System.Collections;

public class AttackEntity : Entity {

    //UNITY PROPERTIES
    public float attackDamage;
    public float attackRange;
    public float attackSpeed;

    //public float scanRange;
    public bool allowScan;
    [Range(0.5f, 5f)]
    public float scanSpeed;

    public Entity projectileType;
    public Transform projectileOrigin;

    //PROPERTIES
    HealthEntity mainTargetEntity;
    HealthEntity targetEntity;

    public void AttackTo(Entity entity)
    {
        StopCoroutine("Attack");
        targetEntity = null;
        mainTargetEntity = entity.healthProperties;
        Collider structureHit;
        if (!Pathfinding.instance.PathExists(transform.position, entity.transform.position, basicProperties.radius))
        {
            Pathfinding.instance.FindPath(transform.position, entity.transform.position, basicProperties.radius, false, true, out structureHit);

            if (structureHit != null)
                targetEntity = structureHit.GetComponent<HealthEntity>();
            else
                print("[ERROR] No structure found and no path without structure found");

        }
        else
            targetEntity = mainTargetEntity;

        AttackSingle(targetEntity);
    }
    public void AttackSingle(Entity target)
    {
        StopAllCoroutines();
        targetEntity = target.healthProperties;
        StartCoroutine("Attack");
    }

    public bool AttackTarget(HealthEntity target)
    {
        if (target != null && basicProperties.CanReach(target, attackRange))
        {
            basicProperties.LookAt(target.transform.position);
            if (projectileType != null)
                LaunchProjectile(target);
            else
                target.Damage(attackDamage);
            return targetEntity != null && targetEntity.IsAlive() && !target.healthProperties.IsPotentiallyDead();
        }
        return false;
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

    public HealthEntity GetClosestEnemy()
    {
        HealthEntity closestTarget = null;
        float minDistance = 0f;
        foreach (Entity target in Map.instance.GetAllEnemies())
        {
            if (target != null && target.HaveHealth() && target.healthProperties.IsAlive() && !target.healthProperties.IsPotentiallyDead() && basicProperties.CanReach(target, attackRange))
            {
                float distance = Vector3.Distance(target.transform.position, transform.position);
                if (distance < minDistance || closestTarget == null)
                {
                    closestTarget = target.healthProperties;
                    minDistance = distance;
                }
            }
        }
        return closestTarget;
    }

    IEnumerator Scan()
    {
        while (targetEntity == null)
        {
            targetEntity = GetClosestEnemy();
            if (targetEntity != null)
                break;
            yield return new WaitForSeconds(scanSpeed);
        }

        AttackSingle(targetEntity);
    }

    public void LaunchProjectile(HealthEntity target)
    {
        GameObject obj = GameObject.Instantiate(projectileType.gameObject);
        ProjectileEntity projectile = obj.GetComponent<ProjectileEntity>();
        if (projectileOrigin != null)
            projectile.transform.position = projectileOrigin.position;
        else
            projectile.transform.position = transform.position;
        projectile.SetTarget(this, target);
    }

    IEnumerator Attack()
    {
        while (targetEntity != null && targetEntity.IsAlive() && !targetEntity.IsPotentiallyDead())
        {
            //REACH
            while (!basicProperties.Reached(targetEntity, attackRange))
                yield return null;
            while (AttackTarget(targetEntity))
                yield return new WaitForSeconds(attackSpeed);
        }

        targetEntity = null;

        if (mainTargetEntity != null && mainTargetEntity.IsAlive())
            AttackTo(mainTargetEntity);
        else
            StartScan();
    }
}
