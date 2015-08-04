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
    public float scanRange;

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
        if (!Pathfinding.instance.PathExists(transform.position, entity.transform.position))
            Pathfinding.RequestPath(new PathfindingParameters(transform.position, entity.transform.position) { ignoreStructure = true }, OnPathFound);
        else
        {
            targetEntity = mainTargetEntity;
            AttackSingle(targetEntity);
        }
        /*movableProperties.ignoreStructure = true;
        StartScan();
        StopCoroutine("AttackAndMove");
        StartCoroutine("AttackAndMove");*/
    }

    IEnumerator AttackAndMove()
    {
        while (targetEntity != null || !basicProperties.Reached(mainTargetEntity))
            yield return null;

        movableProperties.ignoreStructure = false;
        StopAllCoroutines();
    }

    public void OnPathFound(PathfindingResult result)
    {
        if (result.firstStructureHit != null)
        {
            targetEntity = result.firstStructureHit.GetComponent<HealthEntity>();
            AttackSingle(targetEntity);
        }
        else
            print("[ERROR] No structure found and no path without structure found");
    }

    public void AttackSingle(Entity target)
    {
        StopCoroutine("Attack");
        targetEntity = target.healthProperties;
        StartCoroutine("Attack");
    }

    public bool CanAttackEntity(HealthEntity target)
    {
        if (HasEnergy())
            return energyProperties.HasEnoughEnergy();
        return true;
    }

    public bool AttackTarget(HealthEntity target)
    {
        if (target != null && CanAttackEntity(target) && target.IsAlive() && !target.IsPotentiallyDead() && basicProperties.CanReach(target, attackRange))
        {
            if (HasEnergy())
                energyProperties.UseEnergy();
            basicProperties.LookAt(target.transform.position);
            if (projectileType != null)
                LaunchProjectile(target);
            else
                target.Damage(attackDamage);
            return true;
        }
        return false;
    }

    public void StartScan()
    {
        StopCoroutine("Attack");
        StopCoroutine("Scan");
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
            if (targetEntity != null && basicProperties.CanReach(targetEntity, attackRange))
                break;
            else
                targetEntity = null;
            yield return new WaitForSeconds(scanSpeed);
        }

        AttackSingle(targetEntity);
    }

    public HealthEntity GetClosestTarget()
    {
        HealthEntity closestTarget = null;
        float minDistance = 0f;
        foreach (Entity target in Map.instance.healthUnitList)
        {
            if (target != null && target.basicProperties.GetOwner() != basicProperties.GetOwner() && target.HasHealth() && target.healthProperties.IsAlive()
                && !target.healthProperties.IsPotentiallyDead())// && basicProperties.CanReach(target, scanRange))
            {
                float distance = Vector3.Distance(target.transform.position, transform.position);
                if (distance <= scanRange && (distance < minDistance || closestTarget == null))
                {
                    closestTarget = target.healthProperties;
                    minDistance = distance;
                }
            }
        }
        return closestTarget;
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
            while (targetEntity != null && !basicProperties.Reached(targetEntity, attackRange))
                yield return null;
            while (AttackTarget(targetEntity))
                yield return new WaitForSeconds(attackSpeed);
            yield return null;
        }

        targetEntity = null;

        if (mainTargetEntity != null && mainTargetEntity.IsAlive())
            AttackTo(mainTargetEntity);
        else
            StartScan();
    }
}
