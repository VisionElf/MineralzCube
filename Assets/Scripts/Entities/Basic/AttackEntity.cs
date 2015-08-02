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

        if (targetEntity != null)
            StartCoroutine("Attack");
    }

    public bool AttackTarget(HealthEntity target)
    {
        if (target != null && basicProperties.CanReach(target, attackRange))
        {
            if (projectileType != null)
                LaunchProjectile(target);
            else
                target.Damage(attackDamage);
            return true;
        }
        return false;
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
        while (targetEntity != null && targetEntity.IsAlive())
        {
            //REACH
            while (!basicProperties.Reached(targetEntity, attackRange))
                yield return null;
            while (targetEntity.IsAlive() && AttackTarget(targetEntity))
                yield return new WaitForSeconds(attackSpeed);
        }

        if (mainTargetEntity != null && mainTargetEntity.IsAlive())
            AttackTo(mainTargetEntity);
    }
}
