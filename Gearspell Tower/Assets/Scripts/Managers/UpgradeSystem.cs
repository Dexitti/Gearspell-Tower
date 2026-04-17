using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public class UpgradeSystem : MonoBehaviour
{
    [SerializeField] private UpgradeScreen upgradeScreen;
    [SerializeField] private EquipmentData[] allEquipmentUpgrades;
    [SerializeField] private int numberInSelection = 3;

    //[Header("Global Abilities")]
    //[SerializeField] private int healCost = 30;
    //[SerializeField] private float healPercent = 0.25f;
    //[SerializeField] private int regenBoostCost = 20;
    //[SerializeField] private float regenBoostDuration = 10f;
    //[SerializeField] private float regenBoostMultiplier = 3f;
    //[SerializeField] private int mineCost = 40;
    //[SerializeField] private int mineCount = 3;
    //[SerializeField] private GameObject minePrefab;

    private Dictionary<EquipmentData, int> equipmentStageTracker = new();

    //private float regenBoostTimer = 0f;
    //private List<GameObject> activeMines = new();

    //public int HealCost => healCost;
    //public int RegenBoostCost => regenBoostCost;
    //public int MineCost => mineCost;

    private void Awake()
    {
        G.UpgradeSystem = this;
    }

    private void Start()
    {
        // Загрузка улучшений из сохранения
        equipmentStageTracker.Clear();
        foreach (var eq in allEquipmentUpgrades)
            equipmentStageTracker[eq] = G.EquipmentManager.IsEquipped(eq) ? 1 : 0;
    }

    public void OpenUpgradeMenu()
    {
        if (G.GameManager.CurrentState != GameState.Playing) return;

        List<UpgradeData> availableUpgrades = GetAvailableUpgrades();
        upgradeScreen.Open(availableUpgrades, OnUpgradeSelected);
    }

    private List<UpgradeData> GetAvailableUpgrades()
    {
        List<UpgradeData> available = new();

        // Если есть свободные слоты, добавляем новое снаряжение из открытых
        if (G.EquipmentManager.HasFreeSlot())
        {
            foreach (var eq in allEquipmentUpgrades)
            {
                if (!eq.isUnlocked || G.EquipmentManager.IsEquipped(eq)) continue;

                if (eq.stages.Count > 0 && eq.stages[0].upgradeData.Count > 0)
                {
                    available.Add(eq.stages[0].upgradeData[0]);
                }
            }
        }

        // Добавляем апгрейды для экипированного снаряжения
        var activeEquipment = G.EquipmentManager?.GetAllActiveControllers();
        foreach (var contr in activeEquipment)
        {
            EquipmentData eq = contr.Data;
            int currentStage = equipmentStageTracker[eq];
            if (currentStage < eq.stages.Count)
            {
                UpgradeStages stageData = eq.stages[currentStage];
                foreach (var upgrade in stageData.upgradeData)
                {
                    // Проверяем, не взято ли уже (для обычных стадий)
                    if (!stageData.isFork && contr.GetAppliedUpgradeIds().Contains(upgrade.id))
                        continue;
                    available.Add(upgrade);
                }
            }
        }

        // Перемешиваем и берём 3 случайных
        return available.OrderBy(x => UnityEngine.Random.Range(0, int.MaxValue)).Take(numberInSelection).ToList();
    }

    private void OnUpgradeSelected(UpgradeData upgrade)
    {
        G.ResourceManager.SpendGears(upgrade.cost);

        EquipmentData targetEq = FindEquipmentForUpgrade(upgrade);
        if (targetEq == null) return;

        // Если не экипировано, это новое снаряжение — нужно экипировать
        if (!G.EquipmentManager.IsEquipped(targetEq))
        {
            var prefab = G.EquipmentManager.GetPrefabByName(targetEq.equipmentName.Replace(" ", "") + "Controller");
            int slot = G.EquipmentManager.GetFirstFreeSlot();
            if (prefab != null && slot >= 0)
            {
                G.EquipmentManager.EquipToSlot(prefab, slot);
                equipmentStageTracker[targetEq] = 1;
            }
            return;
        }

        // Находим контроллер
        var controller = G.EquipmentManager.GetControllerForData(targetEq);
        if (controller == null) return;

        // Применяем улучшение
        controller.AddUpgradeId(upgrade.id);
        controller.RefreshStats();

        // Обновляем стадию
        int currentStage = equipmentStageTracker[targetEq];
        UpgradeStages stageData = targetEq.stages[currentStage];

        if (stageData.isFork)
        {
            // Развилка — сохраняем выбор
            int choiceIndex = stageData.upgradeData.IndexOf(upgrade);
            G.ProgressManager?.SetForkChoice(targetEq.equipmentName, choiceIndex);
        }

        equipmentStageTracker[targetEq] = currentStage + 1;

        // Сохраняем
        G.ProgressManager?.SetEquipmentStage(targetEq.equipmentName, currentStage + 1);
        G.ProgressManager?.AddAppliedUpgrade(targetEq.equipmentName, upgrade.id);
        Debug.Log($"[UpgradeSystem] Applied {upgrade.upgradeName} to {targetEq.equipmentName}, stage now {currentStage + 1}");
    }

    private EquipmentData FindEquipmentForUpgrade(UpgradeData upgrade)
    {
        foreach (var eq in allEquipmentUpgrades)
        {
            foreach (var stage in eq.stages)
            {
                if (stage.upgradeData.Contains(upgrade))
                    return eq;
            }
        }
        return null;
    }

    public int GetCheapestUpgradeCost()
    {
        var available = GetAvailableUpgrades();
        if (available.Count == 0) return int.MaxValue;
        return available.Min(u => u.cost);
    }

    // === Глобальные способности (для кнопок в UI) ===
    //public bool TryHealTower()
    //{
    //    if (!CanAfford(healCost)) return false;

    //    G.ResourceManager.SpendGears(healCost);

    //    var health = G.Tower?.GetComponent<HealthComponent>();
    //    if (health != null)
    //    {
    //        int healAmount = Mathf.CeilToInt(health.CurrentHealth * healPercent);
    //        health.Heal(healAmount);
    //    }

    //    return true;
    //}

    //public bool TryBoostRegen()
    //{
    //    if (!CanAfford(regenBoostCost)) return false;

    //    G.ResourceManager.SpendGears(regenBoostCost);
    //    // TODO: Применить бафф регенерации
    //    return true;
    //}

    //public bool TryPlaceTraps()
    //{
    //    if (!CanAfford(trapCost)) return false;

    //    G.ResourceManager.SpendGears(trapCost);
    //    // TODO: Разместить ловушку
    //    return true;
    //}
}
