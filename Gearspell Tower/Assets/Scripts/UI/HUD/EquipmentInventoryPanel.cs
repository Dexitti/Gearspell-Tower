using System;
using UnityEngine;
using System.Linq;

public class EquipmentInventoryPanel : MonoBehaviour
{
    [SerializeField] private EquipmentSlotUI[] UISlots;

    public void Initialize(Action<int> slotClickCallback, bool interactive)
    {
        for (int i = 0; i < UISlots.Length; i++)
        {
            if (UISlots[i] != null)
                UISlots[i].Initialize(i, slotClickCallback, interactive);
        }
    }

    public void Refresh()
    {
        Debug.Log($"[InventoryPanel] Refresh on {(gameObject.scene.name == "Game" ? "HUD" : "UpgradeScreen")}");
        var equipManager = G.EquipmentManager;
        if (equipManager == null) return;

        int unlockedSlots = equipManager.UnlockedSlots;
        var controllers = equipManager.GetAllActiveControllers();

        for (int i = 0; i < UISlots.Length && i < equipManager.MaxSlots; i++)
        {
            if (UISlots[i] == null) continue;

            bool isUnlocked = i < unlockedSlots;
            var controller = controllers.FirstOrDefault(c => c.EquippedSlotIndex == i);

            if (!isUnlocked)
            {
                int cost = equipManager.GetSlotUnlockCost(i);
                UISlots[i].SetState(EquipmentSlotState.Locked, null, cost);
            }
            else if (controller != null)
            {
                UISlots[i].SetState(EquipmentSlotState.Equipped, controller.Data?.icon);
            }
            else
            {
                UISlots[i].SetState(EquipmentSlotState.Empty);
            }
        }
    }
}