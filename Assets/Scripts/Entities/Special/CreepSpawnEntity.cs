using UnityEngine;
using System.Collections;

public class CreepSpawnEntity : Entity {

    //UNITY PROPERTIES
    public float spawnInterval;
    public int creepCount;

    public Entity creepType;

    Player playerToAttack;

    //PROPERTIES


    //FUNCTIONS
    void Start()
    {
        playerToAttack = GameObject.Find("Player").GetComponent<Player>();
    }

    public void StartSpawn()
    {
        StopCoroutine("Spawn");
        StartCoroutine("Spawn");
    }

    IEnumerator Spawn()
    {
        for (int i = 0; i < creepCount ;i++)
        {
            SpawnCreep();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnCreep()
    {
        Entity enemy = Map.instance.CreateEntityOnMap(creepType, transform.position);
        enemy.tag = "Creep";
        if (enemy.CanAttack() && playerToAttack.mainBase != null)
            enemy.attackProperties.AttackTo(playerToAttack.mainBase);
    }
}
