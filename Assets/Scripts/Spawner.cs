using System.Collections.Generic;
using UnityEngine;
using UC;

public class Spawner : MonoBehaviour
{
    [SerializeField] private int            initialCount;
    [SerializeField] private int            maxCount;
    [SerializeField] private float          turnTime;
    [SerializeField] private int            spawnPerTurns;
    [SerializeField] private List<Enemy>    enemyPrefabs;

    float turnTimer;
    BoxCollider2D spawnArea;

    void Start()
    {
        turnTimer = turnTime;
        spawnArea = GetComponent<BoxCollider2D>();
        Spawn(initialCount);
    }

    private void Spawn(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Spawn();
        }
    }


    private void Spawn()
    {
        var enemies = GameObject.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        if (enemies.Length < maxCount)
        {
            var enemy = enemyPrefabs.Random();
            var spawnPoint = spawnArea.Random();
            var newObj = Instantiate(enemy, spawnPoint, Quaternion.identity);
        }
    }

    void Update()
    {
        turnTimer -= Time.deltaTime;
        if (turnTimer <= 0.0f)
        {
            Spawn(spawnPerTurns);
            turnTimer = turnTime;
        }
    }
}
