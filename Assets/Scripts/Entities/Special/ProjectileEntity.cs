using UnityEngine;
using System.Collections;

public class ProjectileEntity : Entity {

    AttackEntity parent;
    HealthEntity target;
    Vector3 targetDestination;

    public void SetTarget(AttackEntity _parent, HealthEntity _target)
    {
        parent = _parent;
        target = _target;
        targetDestination = target.transform.position;
        target.AddPotentialDamage(parent.attackDamage);
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
            target.Damage(parent.attackDamage);
        RemoveObject();
    }
}
