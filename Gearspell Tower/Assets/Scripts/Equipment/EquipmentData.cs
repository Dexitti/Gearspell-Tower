using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EquipmentData", menuName = "Scriptable Objects/EquipmentData")]
public class EquipmentData : ScriptableObject
{
    [Header("Базовая информация")]
    public string equipmentName; // Для карточки UI
    //public ElementType element; // tag для улучшений
    public bool isUnlocked = true; // Для баланса в editor (для прогресса используется из SaveManager'а)

    [Header("Объекты")]
    public GameObject decorationPrefab; // Префаб декорации башни
    public GameObject[] projectilesPrefabs;      // Префабы снарядов

    [Header("Параметры")]
    public float damage; // Projectile
    public float size; // Projectile
    public float attackCooldown; // Controller
    public float range; // Controller
    public int projectileCount; // Controller

    [Header("Улучшения")]
    public List<UpgradeStages> stages = new();
}

[Serializable]
public class UpgradeStages
{
    public List<UpgradeData> upgradeData;
    public bool isFork;
}
