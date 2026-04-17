using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.XR.Haptics;

public class EquipmentManager : MonoBehaviour
{
    [Header("Slots")]
    [SerializeField] private int maxSlots = 3;
    [SerializeField] private int[] slotUnlockCosts = { 0, 50, 300 };
    [SerializeField] private int backpackUnlockCost = 100;

    [Header("Equipment")]
    [SerializeField] private GameObject[] activeEquipment = new GameObject[3]; // 3 слота
    [SerializeField] private GameObject backpack;

    private Dictionary<string, GameObject> allEquipmentPrefabs  = new Dictionary<string, GameObject>();
    private EquipmentController[] slotControllers = new EquipmentController[3];
    private EquipmentController backpackController;

    private int unlockedSlots = 1;
    private bool isBackpackUnlocked = false;

    public int MaxSlots => maxSlots;
    public int UnlockedSlots => unlockedSlots;
    public bool IsBackpackUnlocked => isBackpackUnlocked;

    private void Awake()
    {
        G.EquipmentManager = this;
        LoadAllPrefabs();
    }

    private void Start()
    {
        // Загрузка сохранения
        unlockedSlots = G.ProgressManager?.GetUnlockedSlots() ?? 1;
        isBackpackUnlocked = G.ProgressManager?.IsBackpackUnlocked() ?? false;

        EquipToSlot(allEquipmentPrefabs["CryogenicStabilizerController"], 0);
    }

    private void LoadAllPrefabs()
    {
        GameObject[] prefabs = Resources.LoadAll<GameObject>("Prefabs/Controllers");
        foreach (GameObject prefab in prefabs)
        {
            allEquipmentPrefabs[prefab.name] = prefab;
        }
        Debug.Log($"Префабы снаряжения загружены");
    }

    public bool EquipToSlot(GameObject newEquipment, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= maxSlots || slotIndex >= unlockedSlots)
            return false;

        UnequipSlot(slotIndex);

        activeEquipment[slotIndex] = newEquipment;
        if (newEquipment != null)
        {
            GameObject controllerObj = Instantiate(newEquipment, transform);
            slotControllers[slotIndex] = controllerObj.GetComponent<EquipmentController>();
            slotControllers[slotIndex].EquippedSlotIndex = slotIndex;
            Debug.Log($"Снаряжение {newEquipment.name} экипировано в слот {slotIndex}");
        }

        return true;
    }

    public bool EquipItemInBackpack(GameObject newEquipment)
    {
        if (!isBackpackUnlocked) return false;

        if (backpack == null) backpack = newEquipment;
        else
        {
            // Дать игроку возможность выбрать слот
            //TrySwapSlots(newEquipment, slotIndex); // Заменить снаряжение в слоте снаряжением из рюкзака
            // Поместить newEquipment в рюкзак
        }

        return true;
    }

    public void UnequipSlot(int slotIndex)
    {
        Debug.Log($"Снаряжение {slotControllers[slotIndex].gameObject.name} снято со слота {slotIndex}");
        if (slotControllers[slotIndex] != null)
        {
            Destroy(slotControllers[slotIndex].gameObject);
            slotControllers[slotIndex] = null;
        }
        activeEquipment[slotIndex] = null;
    }

    // === Для UpgradeSystem ===
    public bool TryUnlockNextSlot()
    {
        if (!CanUnlockNextSlot(out int cost)) return false;
        if (!G.ResourceManager.SpendGears(cost)) return false;

        if (!isBackpackUnlocked)
        {
            isBackpackUnlocked = true;
            G.ProgressManager?.SetBackpackUnlocked(true);
            Debug.Log("[EquipmentManager] Backpack unlocked");
        }
        else if (unlockedSlots < maxSlots)
        {
            unlockedSlots++;
            G.ProgressManager?.SetUnlockedSlots(unlockedSlots);
            Debug.Log($"[EquipmentManager] Unlocked slot {unlockedSlots}");
        }

        return true;
    }

    public bool CanUnlockNextSlot(out int cost)
    {
        cost = 0;
        if (!isBackpackUnlocked)
        {
            cost = backpackUnlockCost;
            return true;
        }
        if (unlockedSlots < maxSlots)
        {
            cost = slotUnlockCosts[unlockedSlots];
            return true;
        }
        return false;
    }

    public bool HasFreeSlot() => Array.FindIndex(activeEquipment, 0, unlockedSlots, s => s == null) >= 0;

    public int GetFirstFreeSlot() => Array.FindIndex(activeEquipment, 0, unlockedSlots, s => s == null);

    public EquipmentController[] GetAllActiveControllers()
    {
        var list = new List<EquipmentController>();
        foreach (var contr in slotControllers)
        {
            if (contr != null) list.Add(contr);
            return list.ToArray();
        }
        return list.ToArray();
    }

    public EquipmentController GetControllerForData(string equipmentName)
    {
        foreach (var contr in slotControllers)
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
    public void SetBackpackUnlocked(bool unlocked) => isBackpackUnlocked = unlocked;
    public GameObject GetPrefabByName(string name) => allEquipmentPrefabs.GetValueOrDefault(name);

    private void OnApplicationQuit()
    {
        PlayerPrefs.Save();
    }
}
