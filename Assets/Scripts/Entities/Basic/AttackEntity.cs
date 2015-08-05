using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

    bool newSystem = true;

    public void AttackTo(Entity entity)
    {
        StopCoroutine("Attack");
        targetEntity = null;
        mainTargetEntity = entity.healthProperties;
        if (!newSystem)
        {
            if (!Pathfinding.instance.PathExists(transform.position, entity.transform.position))
            {
                print("attackto request");
                Pathfinding.RequestPath(new PathfindingParameters(transform.position, entity.transform.position) { ignoreStructure = true }, OnPathFound);
            }
            else
            {
                targetEntity = mainTargetEntity;
                AttackSingle(targetEntity);
            }
        }
        else
        {
            movableProperties.ignoreStructure = true;
            //StartScan();
            StopCoroutine("AttackAndMove");
            StartCoroutine("AttackAndMove");
        }
    }

    IEnumerator AttackAndMove()
    {
        while (targetEntity != null || (mainTargetEntity != null && !basicProperties.Reached(mainTargetEntity)))
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
        if (target != null)
        {
            targetEntity = target.healthProperties;
            StartCoroutine("Attack");
        }
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
            return target != null && target.IsAlive() && !target.IsPotentiallyDead();
        }
        return false;
    }

    public class PotentialTarget
    {
        public Entity entity;
        public float distance;
        public PotentialTarget(Entity ent, float dist)
        {
            entity = ent;
            distance = dist;
        }

        static public int Compare(PotentialTarget p1, PotentialTarget p2)
        {
            if (p1.distance == p2.distance)
                return 0;
            else if (p1.distance > -1 && (p1.distance < p2.distance || p2.distance == -1))
                return -1;
            else
                return 1;
        }

        static public bool Contains(List<PotentialTarget> list, Entity entity)
        {
            foreach (PotentialTarget target in list)
                if (target.entity == entity)
                    return true;
            return false;
        }
        static public PotentialTarget Get(List<PotentialTarget> list, Entity entity)
        {
            foreach (PotentialTarget target in list)
                if (target.entity == entity)
                    return target;
            return null;
        }
    }

    List<PotentialTarget> targets = new List<PotentialTarget>();
    public void OnTriggerEnter(Collider other)
    {
        if (other != null)
        {
            Entity entity = other.GetComponent<Entity>();
            if (entity != null && entity.HasHealth() && entity.basicProperties.GetOwner() != basicProperties.GetOwner() && !PotentialTarget.Contains(targets, entity))
            {
                targets.Add(new PotentialTarget(entity, -1f));
                UpdateTargets();
            }
        }
    }

    public void UpdateTargets()
    {
        foreach (PotentialTarget target in targets.ToArray())
            if (target.entity != null && target.entity.HasHealth() && target.entity.healthProperties.IsAlive())
                Pathfinding.RequestPath(new PathfindingParameters(transform.position, target.entity.transform.position) { entity = target.entity }, OnTargetPathFound);
            else
                targets.Remove(target);
    }

    public void OnTargetPathFound(PathfindingResult result)
    {
        PotentialTarget target = PotentialTarget.Get(targets, result.entity);
        if (target != null)
        {
            target.distance = result.distance;
            if (targets.Count > 1)
                targets.Sort(PotentialTarget.Compare);
            AttackSingle(targets[0].entity);
        }
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
            yield return null;// new WaitForSeconds(scanSpeed);
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

        UpdateTargets();

        /*if (!newSystem && mainTargetEntity != null && mainTargetEntity.IsAlive())
            AttackTo(mainTargetEntity);
        else
            StartScan();*/
    }

    void OnGUI()
    {
        /*float y = 0;
        foreach (PotentialTarget target in targets)
        {
            if (target.entity != null)
            {
                GUI.color = Color.red;
                GUI.Label(new Rect(0, y, 400, 100), target.entity.name + " - " + target.distance);
                y += 20;
            }
        }*/
    }
    void OnDrawGizmos()
    {
        if (targetEntity != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position + Vector3.up * 0.5f, targetEntity.transform.position + Vector3.up * 0.5f);
        }
    }
}