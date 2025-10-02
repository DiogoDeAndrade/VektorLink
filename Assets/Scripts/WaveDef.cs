using System.Collections.Generic;
using UC;
using UnityEngine;

[CreateAssetMenu(fileName = "WaveDef", menuName = "Line Frontier/Level Def")]
public class WaveDef : ScriptableObject
{
    public int          initialCount;
    public int          maxCount;
    public float        turnTime;
    public int          spawnPerTurns;
    public EnemyList    enemyPrefabs;
}
