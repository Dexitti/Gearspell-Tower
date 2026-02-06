using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    [SerializeField] private GameObject[] activeEquipment = new GameObject[3]; // 3 слота
    private Dictionary<string, GameObject> equipmentPrefabs  = new Dictionary<string, GameObject>();

    void Awake()
    {
        LoadAllPrefabs();
        var firedrill = equipmentPrefabs["FireDrillController"];
        EquipItem(firedrill, 0);
    }

    private void LoadAllPrefabs()
    {
        GameObject[] prefabs = Resources.LoadAll<GameObject>("Prefabs/Controllers");

        foreach (GameObject prefab in prefabs)
        {
            equipmentPrefabs[prefab.name] = prefab;
        }
        Debug.Log($"Префабы снаряжения загружены");
    }

    public bool EquipItem(GameObject newEquipment, int slotIndex)
    {
        if (activeEquipment[slotIndex] != null)
        {
            UnequipItem(slotIndex);
        }

        activeEquipment[slotIndex] = newEquipment;
        if (newEquipment != null)
        {
            GameObject controller = Instantiate(newEquipment, transform);

            Debug.Log($"Снаряжение {newEquipment.name} экипировано в слот {slotIndex}");
        }

        return true;
    }

    public void UnequipItem(int slotIndex)
    {
        var equipment = activeEquipment[slotIndex];
        if (equipment != null)
        {
            activeEquipment[slotIndex] = null;

            GameObject controller = GameObject.Find(equipment.name);
            Destroy(controller);
            Debug.Log($"Снаряжение {equipment.name} снято со слота {slotIndex}");
        }
    }
}
