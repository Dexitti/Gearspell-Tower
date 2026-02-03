using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    [SerializeField] private EquipmentController[] activeEquipment = new EquipmentController[3]; // 3 слота
    private List<EquipmentController> equippedItems = new List<EquipmentController>(); //?

    void Awake()
    {
        InitializeEquipment();
    }

    void Update()
    {
        
    }

    private void InitializeEquipment()
    {
        for (int i = 0; i < activeEquipment.Length; i++)
        {
            if (activeEquipment[i] != null)
            {
                EquipItem(activeEquipment[i], i);
            }
        }
    }

    /// <summary>
    /// Экипировать предмет в указанный слот
    /// </summary>
    public bool EquipItem(EquipmentController newEquipment, int slotIndex)
    {
        // Если в слоте уже есть снаряжение - снимаем его
        if (activeEquipment[slotIndex] != null)
        {
            UnequipItem(slotIndex);
        }

        // Устанавливаем новое снаряжение
        activeEquipment[slotIndex] = newEquipment;

        // Инициализируем снаряжение
        if (newEquipment != null)
        {
            //newEquipment.Initialize(this, slotIndex);
            equippedItems.Add(newEquipment);


            Debug.Log($"Снаряжение {newEquipment.data.equipmentName} экипировано в слот {slotIndex}");
        }

        return true;
    }

    /// <summary>
    /// Снять снаряжение со слота
    /// </summary>
    public void UnequipItem(int slotIndex)
    {
        var equipment = activeEquipment[slotIndex];
        if (equipment != null)
        {
            equippedItems.Remove(equipment);
            activeEquipment[slotIndex] = null;

            // TODO: Удаление визуала
            //Debug.Log($"Снаряжение {equipment.EquipmentName} снято со слота {slotIndex}");
        }
    }
}
