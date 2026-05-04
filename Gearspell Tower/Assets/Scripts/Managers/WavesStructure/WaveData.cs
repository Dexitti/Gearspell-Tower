using System;
using System.Linq;
using UnityEngine;

// Насоздавать в Resources в Unity!!!
[CreateAssetMenu(fileName = "WaveData", menuName = "Scriptable Objects/WaveData")]
public class WaveData : ScriptableObject
{
    [Header("Wave Info")]
    public string waveName = "Wave 1";
    public EnemySpawnConfig[] enemySpawns;

    [Header("Dialogs")]
    //public bool isDialog = false; // checkbox в инспекторе: если true, то показывать следующее поле
    public DialogData[] waveDialogs;

    [Header("Rewards")]
    public int gearsReward = 0;

    [Header("Unlocks")]
    public EquipmentData[] equipmentUnlocks;

    public int GetTotalEnemyCount() => enemySpawns.Sum(e => e.count);
}

[Serializable]
public struct EnemySpawnConfig
{
    public GameObject enemyPrefab;
    public int count;
    public float spawnInterval;
}