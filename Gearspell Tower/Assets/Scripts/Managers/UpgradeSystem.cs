using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UpgradeSystem : MonoBehaviour
{
    [SerializeField] private UpgradeScreen upgradeScreen;
    [SerializeField] private EquipmentData[] allEquipmentUpgrades;

    [Header("Global Abilities")]
    [SerializeField] private int healCost = 200;
    [SerializeField] private float healPercent = 0.25f;
    [SerializeField] private int regenBoostCost = 200;
    [SerializeField] private int regenBoostMultiplier = 12;
    [SerializeField] private float regenBoostDuration = 30f;
    [SerializeField] private int minesCost = 200;
    [SerializeField] private int minesCount = 11;
    [SerializeField] private GameObject minePrefab;

    private Dictionary<EquipmentData, int> equipmentStageTracker = new();
    private List<UpgradeData> cachedAvailableUpgrades;
    private bool isCacheValid = false;

    private List<GameObject> activeMines = new();

    public int HealCost => healCost;
    public float HealPercent => healPercent;
    public int RegenBoostCost => regenBoostCost;
    public float RegenBoostDuration => regenBoostDuration;
    public int MinesCost => minesCost;
    public int MinesCount => minesCount;

    public bool IsHealAvailable => !(G.ProgressManager?.IsHealUsed() ?? false);
    public bool IsRegenBoostAvailable => !(G.ProgressManager?.IsRegenBoostUsed() ?? false);
    public bool IsTrapsAvailable => !(G.ProgressManager?.IsTrapsUsed() ?? false);

    private void Awake()
    {
        G.UpgradeSystem = this;
        allEquipmentUpgrades = Resources.LoadAll<EquipmentData>("Data/EquipmentData");
    }

    private void Start()
    {
        // Загрузка улучшений из сохранения
        upgradeScreen.CardClicked += OnUpgradeSelected;
        equipmentStageTracker.Clear();
        foreach (var eq in allEquipmentUpgrades)
            equipmentStageTracker[eq] = G.EquipmentManager.IsEquipped(eq) ? 1 : 0;
    }

    public void OpenUpgradeMenu()
    {
        if (upgradeScreen.HasSavedOffers())
        {
            upgradeScreen.OpenWithSavedOffers();
            return;
        }

        List<UpgradeData> availableUpgrades = GetAvailableUpgrades();
        int count = Mathf.Min(3, availableUpgrades.Count);
        var initialOffers = availableUpgrades.Take(count).ToList();
        upgradeScreen.Open(initialOffers);
    }

    public List<UpgradeData> GetAvailableUpgrades(bool forceRefresh = false)
    {
        if (!forceRefresh && isCacheValid && cachedAvailableUpgrades != null)
            return cachedAvailableUpgrades;

        List<UpgradeData> available = new();

        // Если есть свободные слоты, добавляем новое снаряжение из открытых
        if (G.EquipmentManager != null && G.EquipmentManager.HasFreeSlot())
        {
            foreach (var eq in allEquipmentUpgrades)
            {
                if (!eq.isUnlocked || G.EquipmentManager.IsEquipped(eq)) continue;

                if (eq.stages.Count > 0 && eq.stages[0].upgradeData.Count > 0)
                {
                    var upgrade = eq.stages[0].upgradeData[0];
                    if (!available.Contains(upgrade))
                        available.Add(upgrade);
                }
            }
        }

        // Добавляем апгрейды для экипированного снаряжения
        var activeEquipment = G.EquipmentManager?.GetActiveControllers();
        foreach (var contr in activeEquipment)
        {
            EquipmentData eq = contr.Data;
            int currentStage = equipmentStageTracker[eq];

            // 1. Обычные апгрейды (всегда, пока есть что улучшать)
            if (currentStage < eq.stages.Count)
            {
                UpgradeStages stageData = eq.stages[currentStage];
                if (!stageData.isFork)
                {
                    foreach (var upgrade in stageData.upgradeData)
                    {
                        if (contr.GetAppliedUpgradeIds().Contains(upgrade.id)) continue;
                        if (!available.Contains(upgrade))
                            available.Add(upgrade);
                    }
                }
            }

            // 2. Fork-апгрейды (если куплен хотя бы один обычный апгрейд)
            if (contr.HasAnyUpgrade && contr.ForkChoice == -1)
            {
                var forkStage = eq.stages.FirstOrDefault(s => s.isFork);
                if (forkStage != null)
                {
                    foreach (var upgrade in forkStage.upgradeData)
                    {
                        if (!available.Contains(upgrade))
                            available.Add(upgrade);
                    }
                }
            }

            // 3. Активная способность (если fork выбран)
            if (contr.ForkChoice != -1)
            {
                var activeStage = eq.stages.FirstOrDefault(s => !s.isFork && s.upgradeData[0]?.cardType == UpgradeCardType.ActiveAbility);
                if (activeStage != null)
                {
                    foreach (var upgrade in activeStage.upgradeData)
                    {
                        if (!available.Contains(upgrade))
                            available.Add(upgrade);
                    }
                }
            }
        }

        cachedAvailableUpgrades = available.OrderBy(x => UnityEngine.Random.Range(0, int.MaxValue)).ToList();
        isCacheValid = true;
        return cachedAvailableUpgrades;
    }

    private void OnUpgradeSelected(UpgradeData upgrade)
    {
        G.ResourceManager.SpendGears(upgrade.cost);

        EquipmentData targetEq = FindEquipmentForUpgrade(upgrade);
        if (targetEq == null) return;

        // Если не экипировано, это новое снаряжение — нужно экипировать
        if (!G.EquipmentManager.IsEquipped(targetEq))
        {
            var prefab = G.EquipmentManager.GetPrefabByName(targetEq.equipmentName);
            int slot = G.EquipmentManager.GetFirstFreeSlot();
            if (slot >= 0)
            {
                G.EquipmentManager.EquipToSlot(prefab, slot);
                equipmentStageTracker[targetEq] = 1;
                G.EventManager?.TriggerEquipmentEquipped(targetEq, slot);
                G.EventManager?.TriggerEquipmentUpgraded(targetEq, 1);
                G.ProgressManager?.SetEquipmentStage(targetEq.equipmentName, 1);
            }
            InvalidateCache();
            return;
        }
        else
        {
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
                controller.ForkChoice = choiceIndex;
            }

            if (upgrade.cardType == UpgradeCardType.ActiveAbility)
            {
                controller.HasActiveAbility = true;
                Debug.Log($"[UpgradeSystem] Active ability purchased for {targetEq.equipmentName}");
            }

            equipmentStageTracker[targetEq] = currentStage + 1;

            // Сохраняем
            G.ProgressManager?.SetEquipmentStage(targetEq.equipmentName, currentStage + 1);
            G.ProgressManager?.AddAppliedUpgrade(targetEq.equipmentName, upgrade.id);
            Debug.Log($"[UpgradeSystem] Applied {upgrade.upgradeName} to {targetEq.equipmentName}, stage now {currentStage + 1}");
        }

        InvalidateCache();
        G.EventManager?.TriggerEquipmentUpgraded(targetEq, equipmentStageTracker[targetEq]);
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

    public void InvalidateCache()
    {
        isCacheValid = false;
        cachedAvailableUpgrades = null;
    }

    public int GetCheapestUpgradeCost() //!
    {
        var availableUpgrades = GetAvailableUpgrades();
        if (availableUpgrades.Count == 0) return int.MaxValue;
        G.EquipmentManager.CanUnlockNextSlot(out int slotCost);
        return Mathf.Min(availableUpgrades.Min(u => u.cost), slotCost);
    }

    // === Глобальные способности (для кнопок в UI) ===
    public bool TryHealTower()
    {
        if (G.ProgressManager?.IsHealUsed() == true) return false;
        if (!G.ResourceManager.SpendGears(healCost)) return false;
        var health = G.Tower?.GetComponent<HealthComponent>();
        if (health != null)
        {
            health.Heal(Mathf.CeilToInt(health.MaxHealth * healPercent));
            G.ProgressManager?.SetHealUsed(true);
            return true;
        }
        return false;
    }

    public bool TryBoostRegen()
    {
        if (G.ProgressManager?.IsRegenBoostUsed() == true) return false;
        if (!G.ResourceManager.SpendGears(regenBoostCost)) return false;
        G.Tower.SetRegeneration(regenBoostMultiplier);
        StartCoroutine(ResetRegen());
        G.ProgressManager?.SetRegenBoostUsed(true);
        return true;
    }

    private IEnumerator ResetRegen()
    {
        yield return new WaitForSeconds(regenBoostDuration);
        G.Tower.SetRegeneration(1);
    }

    public bool TryPlaceTraps()
    {
        if (G.ProgressManager?.IsTrapsUsed() == true) return false;
        if (!G.ResourceManager.SpendGears(minesCost)) return false;
        PlaceMines();
        G.ProgressManager?.SetTrapsUsed(true);
        return true;
    }

    private void PlaceMines()
    {
        Vector3 towerPos = G.Tower.Position;

        for (int i = 0; i < minesCount; i++)
        {
            float distance = UnityEngine.Random.Range(2, 7.5f);
            float angle = UnityEngine.Random.Range(0f, 360f);
            Vector3 spawnPos = towerPos + new Vector3(0, -1.2f, 0) + IsometricExtension.IsoVector(angle, distance);

            GameObject mine = Instantiate(minePrefab, spawnPos, Quaternion.identity, transform);
            StartCoroutine(AnimateMineFlight(mine, towerPos, spawnPos));
            activeMines.Add(mine);
        }
    }

    private IEnumerator AnimateMineFlight(GameObject mine, Vector3 start, Vector3 end)
    {
        float duration = 0.5f;
        float elapsed = 0f;

        SpriteRenderer sprite = mine.GetComponent<SpriteRenderer>();
        Collider2D col = mine.GetComponent<Collider2D>();

        Color c = sprite.color;
        sprite.color = new Color(c.r, c.g, c.b, 0f);

        col.enabled = false;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            Vector3 currentPos = Vector3.Lerp(start, end, t);
            float arc = Mathf.Sin(t * Mathf.PI) * 1.5f;
            currentPos.y += arc;

            mine.transform.position = currentPos;

            float alpha = Mathf.Lerp(0f, 1f, t);
            sprite.color = new Color(c.r, c.g, c.b, alpha);

            yield return null;
        }

        mine.transform.position = end;
        if (col != null) col.enabled = true;
        sprite.color = new Color(c.r, c.g, c.b, 1f);
    }
}
