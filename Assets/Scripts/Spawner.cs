using UnityEngine;
using UC;

public class Spawner : MonoBehaviour
{
    float turnTimer;
    BoxCollider2D spawnArea;
    WaveDef waveDef;

    void Start()
    {
        waveDef = GameManager.Instance.GetWave();
        turnTimer = waveDef.turnTime;
        spawnArea = GetComponent<BoxCollider2D>();
        Spawn(waveDef.initialCount);

        GameManager.Instance.onChangeWave += Instance_onChangeWave;
    }

    private void OnDestroy()
    {
        GameManager.Instance.onChangeWave -= Instance_onChangeWave;
    }

    private void Instance_onChangeWave(int wave)
    {
        waveDef = GameManager.Instance.GetWave();
        turnTimer = waveDef.turnTime;
        Spawn(waveDef.initialCount);
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
        if (enemies.Length < waveDef.maxCount)
        {
            var enemy = waveDef.enemyPrefabs.Get();
            var spawnPoint = spawnArea.Random();
            var newObj = Instantiate(enemy, spawnPoint, Quaternion.identity);
        }
    }

    void Update()
    {
        turnTimer -= Time.deltaTime;
        if (turnTimer <= 0.0f)
        {
            Spawn(waveDef.spawnPerTurns);
            turnTimer = waveDef.turnTime;
        }
    }
}
