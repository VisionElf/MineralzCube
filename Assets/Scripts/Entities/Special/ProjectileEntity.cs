using UnityEngine;
using System.Collections;

public class ProjectileEntity : Entity {

    Entity parent;
    Entity target;
    Vector3 targetDestination;

    public void SetTarget(Entity _parent, Entity _target)
    {
        parent = _parent;
        target = _target;
        targetDestination = target.transform.position;
        StartCoroutine("Launch");
    }

    IEnumerator Launch()
    {
        while (!basicProperties.Reached(targetDestination))
        {
            if (target != null)
                targetDestination = target.transform.position;
            yield return null;
        }

        ApplyDamage();
    }

    public void ApplyDamage()
    {
        if (target != null)
        {
            if (target.HaveHealth() && parent.CanAttack())
                target.healthProperties.Damage(parent.attackProperties.attackDamage);
        }
        RemoveObject();
    }
}
