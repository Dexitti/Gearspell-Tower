using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    [Header("Slots")]
    [SerializeField] private int maxSlots = 3;
    [SerializeField] private int[] slotUnlockCosts = { 0, 50, 300 };
    
    [Header("Equipment")]
    [SerializeField] private EquipmentData[] startUnlockedEquipment;
    [SerializeField] private EquipmentController[] activeEquipment = new EquipmentController[3];
    
    private Dictionary<string, EquipmentController> allEquipmentPrefabs  = new Dictionary<string, EquipmentController>();
    private int unlockedSlots = 1;
    
    public int MaxSlots => maxSlots;
    public int UnlockedSlots => unlockedSlots;
    public IReadOnlyList<EquipmentController> EquipmentSlots => activeEquipment;

    private void Awake()
    {
        G.EquipmentManager = this;
        LoadAllPrefabs();
    }

    private void Start()
    {
        if (G.EquipmentManager == null)
            G.EquipmentManager = this;

        InitializeGameplay();
    }

    private void InitializeGameplay()
    {
        G.EventManager?.ResetGameplayInitialized(); // For debug (for running Game)

        // Load saved session data
        unlockedSlots = G.ProgressManager?.GetUnlockedSlots() ?? 1;

        // Load initial equipment set
        startUnlockedEquipment = Resources.LoadAll<EquipmentData>("Data/EquipmentData")
            .Where(eqData => eqData.name == "WindmillData" || eqData.name == "FireDrillData" || eqData.name == "LightningCogsData").ToArray();

        //if (!G.SaveManager.IsEquipmentUnlocked("CryogenicStabilizer"))
        //    G.SaveManager.UnlockEquipment("CryogenicStabilizer"); // For debug

        G.Tower?.Initialize();

        if (!G.ProgressManager.HasSession)
        {
            foreach (var eq in startUnlockedEquipment)
            {
                if (!G.SaveManager.IsEquipmentUnlocked(eq.equipmentName))
                    G.SaveManager.UnlockEquipment(eq.equipmentName);
            }
            EquipStartEquipment();
        }
        else
        {
            // Apply saved session
            var session = G.ProgressManager.LoadSession();
            if (session != null)
                G.ProgressManager.ApplySession(session);
        }

        G.EventManager?.TriggerGameplayInitialized();
    }

    private void LoadAllPrefabs()
    {
        GameObject[] prefabs = Resources.LoadAll<GameObject>("Prefabs/Equipment/Controllers");
        foreach (GameObject prefab in prefabs)
        {
            EquipmentController contr = prefab.GetComponent<EquipmentController>();
            allEquipmentPrefabs[contr.Data.equipmentName] = contr;
        }
        Debug.Log($"[EquipmentManager] Equipment prefabs {allEquipmentPrefabs.Count} loaded");
    }

    private void EquipStartEquipment()
    {
        var allEquipment = Resources.LoadAll<EquipmentData>("Data/EquipmentData");
        var unlockedEquipment = allEquipment.Where(eq => G.SaveManager.IsEquipmentUnlocked(eq.equipmentName)).ToList();

        if (unlockedEquipment.Count == 0)
        {
            Debug.LogWarning("[EquipmentManager] No unlocked equipment to equip!");
            return;
        }

        if (!G.ProgressManager.HasSession)
            EquipToSlot(allEquipmentPrefabs["PoisonRotors"], 0); // Changed to Windmill
        else
        {
            var randomEq = unlockedEquipment[UnityEngine.Random.Range(0, unlockedEquipment.Count)];
            string prefabName = randomEq.equipmentName;
            EquipToSlot(allEquipmentPrefabs[prefabName], 0);
        }
    }

    public bool EquipToSlot(EquipmentController newEquipment, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxSlots || slotIndex >= unlockedSlots)
            return false;

        if (activeEquipment[slotIndex] != null)
            UnequipSlot(slotIndex);

        EquipmentController contr = Instantiate(newEquipment, transform);
        Debug.Log($"[EquipmentManager] Equipped {newEquipment.name} into slot {slotIndex}");

        activeEquipment[slotIndex] = contr;
        
        if (contr.Data != null)
            G.EventManager?.TriggerEquipmentEquipped(contr.Data, slotIndex);

        return true;
    }

    public void UnequipSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxSlots) return;

        var eq = activeEquipment[slotIndex];
        if (eq != null)
        {
            Debug.Log($"[EquipmentManager] Unequipped {eq.name} from slot {slotIndex}");
            Destroy(eq.gameObject);
        }
        activeEquipment[slotIndex] = null;
    }

    // === For UpgradeSystem ===
    public bool TryUnlockNextSlot()
    {
        if (!CanUnlockNextSlot(out int cost)) return false;
        if (!G.ResourceManager.SpendGears(cost)) return false;

        if (unlockedSlots < maxSlots)
        {
            unlockedSlots++;
            G.ProgressManager?.SetUnlockedSlots(unlockedSlots);
            G.EventManager?.TriggerSlotUnlocked(unlockedSlots - 1);
            Debug.Log($"[EquipmentManager] Slot {unlockedSlots} unlocked");
        }

        return true;
    }

    public bool CanUnlockNextSlot(out int cost)
    {
        cost = 9999;
        if (unlockedSlots < maxSlots)
        {
            cost = slotUnlockCosts[unlockedSlots];
            return true;
        }
        return false;
    }

    public int GetSlotUnlockCost(int slotIndex)
    {
        if (slotIndex < unlockedSlots) return 0;

        if (slotIndex >= 0 && slotIndex < slotUnlockCosts.Length)
            return slotUnlockCosts[slotIndex];

        return 0;
    }

    public bool HasFreeSlot() => Array.FindIndex(activeEquipment, 0, unlockedSlots, s => s == null) >= 0;

    public int GetFirstFreeSlot() => Array.FindIndex(activeEquipment, 0, unlockedSlots, s => s == null);

    public EquipmentController[] GetActiveControllers() => activeEquipment.Where(eq => eq != null).ToArray();

    public EquipmentController GetControllerForData(string equipmentName)
    {
        foreach (var contr in activeEquipment)
            if (contr != null && contr.Data.equipmentName == equipmentName)
                return contr;
        return null;
    }

    public EquipmentController GetControllerForData(EquipmentData data)
    {
        return GetControllerForData(data.equipmentName);
    }

    public bool IsEquipped(EquipmentData data) => GetControllerForData(data) != null;

    public void SetUnlockedSlots(int slots) => unlockedSlots = Mathf.Clamp(slots, 1, maxSlots);
    public EquipmentController GetPrefabByName(string name) => allEquipmentPrefabs.GetValueOrDefault(name);

    private void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }
}
