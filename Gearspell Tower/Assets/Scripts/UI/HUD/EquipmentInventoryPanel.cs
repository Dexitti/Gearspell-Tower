using System;
using UnityEngine;
using System.Linq;

public class EquipmentInventoryPanel : MonoBehaviour
{
    [SerializeField] private EquipmentSlotUI[] UISlots;

    public event Action<int> OnClick;

    public void Initialize(bool interactive)
    {
        for (int i = 0; i < UISlots.Length; i++)
        {
            if (UISlots[i] != null)
            {
                UISlots[i].Initialize(i, interactive);
                UISlots[i].OnClick += (int index) => OnClick?.Invoke(index);
            }
        }
    }

    public void Refresh()
    {
        var equipManager = G.EquipmentManager;
        if (equipManager == null) return;

        int unlockedSlots = equipManager.UnlockedSlots;
        var controllers = equipManager.EquipmentSlots;

        for (int i = 0; i < controllers.Count; i++)
        {
            if (UISlots[i] == null) continue;

            bool isUnlocked = i < unlockedSlots;
            var controller = controllers[i];

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