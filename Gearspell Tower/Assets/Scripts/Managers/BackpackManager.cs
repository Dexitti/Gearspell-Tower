using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class BackpackManager
{
    [SerializeField] private int backpackUnlockCost = 100;

    [SerializeField] private GameObject backpack;
    private EquipmentController backpackController;

    private bool isBackpackUnlocked = false;

    public bool IsBackpackUnlocked => isBackpackUnlocked;

    private void Start()
    {
        isBackpackUnlocked = G.ProgressManager?.IsBackpackUnlocked() ?? false;
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

    public void SetBackpackUnlocked(bool unlocked) => isBackpackUnlocked = unlocked;
}
