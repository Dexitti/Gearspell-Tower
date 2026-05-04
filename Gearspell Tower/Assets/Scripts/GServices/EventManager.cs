using System;
using UnityEngine;

//Singleton
public class EventManager : MonoBehaviour
{
    // === Состояния игры ===
    public event Action<GameState> OnGameStateChanged;
    public void TriggerGameStateChanged(GameState newState) => OnGameStateChanged?.Invoke(newState);

    // === Башня ===
    public event Action<float, float> OnTowerHealthChanged;
    public void TriggerTowerHealthChanged(float current, float max) => OnTowerHealthChanged?.Invoke(current, max);

    public event Action OnTowerDestroyed;
    public void TriggerTowerDestroyed() => OnTowerDestroyed?.Invoke();

    // === Снаряжение ===
    public event Action<EquipmentData[]> OnEquipmentUnlocked;
    public void TriggerEquipmentUnlocked(EquipmentData[] equipment) => OnEquipmentUnlocked?.Invoke(equipment);

    public event Action<EquipmentData, int> OnEquipmentUpgraded;
    public void TriggerEquipmentUpgraded(EquipmentData data, int newLevel) => OnEquipmentUpgraded?.Invoke(data, newLevel);

    // === Волны ===
    public event Action<int> OnWaveStarted;
    public void TriggerWaveStarted(int waveNumber) => OnWaveStarted?.Invoke(waveNumber);

    public event Action<int> OnWaveCompleted;
    public void TriggerWaveCompleted(int waveNumber) => OnWaveCompleted?.Invoke(waveNumber);

    // === Враги ===
    public event Action<Creature> OnEnemyKilled;
    public void TriggerEnemyKilled(Creature enemy) => OnEnemyKilled?.Invoke(enemy);

    public event Action<Creature> OnEnemyReachedTower;
    public void TriggerEnemyReachedTower(Creature enemy) => OnEnemyReachedTower?.Invoke(enemy);

    // === Ресурсы ===
    public event Action<int> OnGearsChanged;
    public void TriggerGearsChanged(int newAmount) => OnGearsChanged?.Invoke(newAmount);
}