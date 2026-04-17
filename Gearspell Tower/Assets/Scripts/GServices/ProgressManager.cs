using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EquipmentSaveData
{
    public string equipmentName;
    public int slotIndex = -1;
    public int stage = 0;
    public int forkChoice = -1; // -1 = не выбрано, 0 = A, 1 = B
    public bool hasActiveAbility = false;
    public List<string> appliedUpgradeIds = new();
}

[Serializable]
public class SessionData
{
    public int currentWave = 1;
    public int gears;
    public int towerHealth;
    public int unlockedSlots = 1;
    public bool isBackpackUnlocked = false;
    public List<EquipmentSaveData> equipmentProgress = new();
}

//Singleton
public class ProgressManager : MonoBehaviour
{
    private const string SESSION_KEY = "GameSession";
    public bool HasSession => PlayerPrefs.HasKey(SESSION_KEY);
    private SessionData currentSession;

    private void Awake()
    {
        G.ProgressManager = this;
        DontDestroyOnLoad(gameObject);
    }

    public SessionData LoadSession()
    {
        if (!HasSession) return null;

        string json = PlayerPrefs.GetString(SESSION_KEY);
        currentSession = JsonUtility.FromJson<SessionData>(json);
        Debug.Log($"[ProgressManager] Session loaded");
        return currentSession;
    }

    public void SaveSession()
    {
        var session = new SessionData();
        session.currentWave = G.GameLoopManager?.GetCurrentWaveNumber() ?? 1;
        session.gears = G.ResourceManager?.Gears ?? 0;
        session.towerHealth = G.Tower?.GetComponent<HealthComponent>().CurrentHealth ?? 1;
        session.unlockedSlots = G.EquipmentManager?.UnlockedSlots ?? 1;
        session.isBackpackUnlocked = G.EquipmentManager?.IsBackpackUnlocked ?? false;

        foreach (var eq in G.EquipmentManager.GetAllActiveControllers())
        {
            session.equipmentProgress.Add(new EquipmentSaveData
            {
                slotIndex = eq.EquippedSlotIndex,
                stage = eq.Stage,
                forkChoice = eq.ForkChoice,
                hasActiveAbility = eq.HasActiveAbility,
                appliedUpgradeIds = eq.GetAppliedUpgradeIds()
            });
        }

        string json = JsonUtility.ToJson(session);
        PlayerPrefs.SetString(SESSION_KEY, json);
        PlayerPrefs.Save();
        currentSession = session;
        Debug.Log($"[ProgressManager] Session saved");
    }

    public void ApplySession(SessionData session)
    {
        if (session == null) return;
        G.ResourceManager?.SetGears(session.gears);
        G.Tower?.GetComponent<HealthComponent>()?.SetHealth(session.towerHealth);
        G.EquipmentManager?.SetUnlockedSlots(session.unlockedSlots);
        G.EquipmentManager?.SetBackpackUnlocked(session.isBackpackUnlocked);

        // Экипируем сохранённое снаряжение
        foreach (var data in session.equipmentProgress)
        {
            var controller = G.EquipmentManager?.GetControllerForData(data.equipmentName);
            if (controller != null)
            {
                controller.Stage = data.stage;
                controller.ForkChoice = data.forkChoice;
                controller.HasActiveAbility = data.hasActiveAbility;
                foreach (var id in data.appliedUpgradeIds)
                    controller.AddUpgradeId(id);
                controller.RefreshStats();
            }
        }

        G.GameLoopManager?.SetCurrentWave(session.currentWave);
    }

    public void ClearSession()
    {
        PlayerPrefs.DeleteKey(SESSION_KEY);
        currentSession = null;
        Debug.Log("[ProgressManager] Session cleared");
    }

    private EquipmentSaveData GetProgress(string equipmentName)
    {
        if (currentSession == null) return null;
        return currentSession.equipmentProgress.Find(p => p.equipmentName == equipmentName);
    }

    public void SetUnlockedSlots(int slots)
    {
        if (currentSession != null) 
            currentSession.unlockedSlots = slots;
        SaveSession();
    }

    public void SetBackpackUnlocked(bool unlocked)
    {
        if (currentSession != null)
            currentSession.isBackpackUnlocked = unlocked;
        SaveSession(); 
    }

    public int GetUnlockedSlots() => currentSession?.unlockedSlots ?? 1;

    public bool IsBackpackUnlocked() => currentSession?.isBackpackUnlocked ?? false;

    public void SetEquipmentStage(string equipmentName, int stage)
    {
        var progress = GetProgress(equipmentName);
        if (progress != null) progress.stage = stage;
        else currentSession?.equipmentProgress.Add(new EquipmentSaveData { equipmentName = equipmentName, stage = stage });
        SaveSession();
    }

    public void SetForkChoice(string equipmentName, int choice)
    {
        var progress = GetProgress(equipmentName);
        if (progress != null) progress.forkChoice = choice;
        else currentSession?.equipmentProgress.Add(new EquipmentSaveData { equipmentName = equipmentName, forkChoice = choice });
        SaveSession();
    }

    public void AddAppliedUpgrade(string equipmentName, string upgradeId)
    {
        var progress = GetProgress(equipmentName);
        if (progress != null && !progress.appliedUpgradeIds.Contains(upgradeId))
            progress.appliedUpgradeIds.Add(upgradeId);
        else
            currentSession?.equipmentProgress.Add(new EquipmentSaveData { equipmentName = equipmentName, appliedUpgradeIds = new List<string> { upgradeId } });
        SaveSession();
    }
}