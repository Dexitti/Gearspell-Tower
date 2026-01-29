using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    [SerializeField] private EquipmentController[] activeEquipment = new EquipmentController[4]; // 4 слота
    private List<EquipmentController> equippedItems = new List<EquipmentController>(); //?
    [SerializeField] private Transform equipmentPivot;

    void Awake()
    {
        InitializeEquipment();
    }

    // Update is called once per frame
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
        if (slotIndex < 0 || slotIndex >= activeEquipment.Length)
        {
            Debug.LogError($"Некорректный индекс слота: {slotIndex}");
            return false;
        }

        // Если в слоте уже есть снаряжение - снимаем его
        if (activeEquipment[slotIndex] != null)
        {
            UnequipItem(slotIndex);
        }

        // Устанавливаем новое снаряжение
        activeEquipment[slotIndex] = newEquipment;

        //// Инициализируем снаряжение
        //if (newEquipment != null)
        //{
        //    newEquipment.Initialize(this, slotIndex);
        //    equippedItems.Add(newEquipment);

        //    // Визуал: размещаем модельку на башне
        //    if (equipmentPivot != null)
        //    {
        //        // TODO: Логика позиционирования разных типов снаряжения
        //        // Можно использовать систему трансформов для разных слотов
        //        Instantiate(newEquipment.EquipmentVisual, equipmentPivot);
        //    }

        //    Debug.Log($"Снаряжение {newEquipment.EquipmentName} экипировано в слот {slotIndex}");
        //}

        return true;
    }

    /// <summary>
    /// Снять снаряжение со слота
    /// </summary>
    public void UnequipItem(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= activeEquipment.Length)
        {
            Debug.LogError($"Некорректный индекс слота: {slotIndex}");
            return;
        }

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
