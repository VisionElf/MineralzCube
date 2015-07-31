using UnityEngine;
using System.Collections;

public class HealthEntity : Entity {

    //UNITY PROPERTIES
    public float maxHealth;

    //PUBLIC PROPERTIES
    public float health { get; set; }

    void Start()
    {
        health = maxHealth;
    }

    public void Damage(float damage)
    {
        if (damage > health)
            damage = health;
        health -= damage;
        if (health <= 0)
            OnDeath();
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


    public bool IsDead()
    {
        return health <= 0;
    }
    public virtual void OnDeath()
    {
        RemoveObject();
    }
}
