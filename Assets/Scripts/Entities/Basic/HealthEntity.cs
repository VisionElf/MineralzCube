using UnityEngine;
using System.Collections;

public class HealthEntity : Entity {

    //UNITY PROPERTIES
    public float maxHealth;

    //PUBLIC PROPERTIES
    public float health { get; set; }

    float potentialDamage;

    public void AddPotentialDamage(float dmg)
    {
        potentialDamage += dmg;
    }
    public bool IsPotentiallyDead()
    {
        return potentialDamage >= health;
    }

    void Start()
    {
        health = maxHealth;
    }

    public void Damage(float damage)
    {
        if (damage > health)
            damage = health;
        health -= damage;
        potentialDamage -= damage;
        StopCoroutine("DamageVisualEffect");
        StartCoroutine("DamageVisualEffect");
        if (health <= 0)
            OnDeath();
    }

    IEnumerator DamageVisualEffect()
    {
        for (int i = 0; i < Mathf.RoundToInt(1 / Time.deltaTime); i++)
        {
            basicProperties.model.SetColor(1f - i * Time.deltaTime, 0, 0);
            yield return null;
        }
    }

    public void Heal(float heal)
    {
        if (heal > maxHealth - health)
            heal = maxHealth - health;
        health += heal;
    }

    public float GetPercentHealth()
    {
        return health / maxHealth;
    }
    public void AddPercentHealth(float percent)
    {
        health += percent * maxHealth;
    }
    public void RemovePercentHealth(float percent)
    {
        health -= percent * maxHealth;
    }


    public bool IsDead()
    {
        return health <= 0;
    }
    public bool IsAlive()
    {
        return health > 0;
    }
    public virtual void OnDeath()
    {
        RemoveObject();
    }
}
