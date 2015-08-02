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
        GameObject creepObj = GameObject.Instantiate(creepType.gameObject);
        creepObj.transform.position = transform.position;
        creepObj.tag = "Creep";
        Entity enemy = creepObj.GetComponent<Entity>();
        if (enemy.CanAttack())
        {
            enemy.attackProperties.AttackTo(playerToAttack.mainBase);
        }
    }
}
