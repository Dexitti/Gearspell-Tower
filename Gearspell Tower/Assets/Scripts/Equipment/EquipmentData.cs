using UnityEngine;

[CreateAssetMenu(fileName = "EquipmentData", menuName = "Scriptable Objects/EquipmentData")]
public class EquipmentData : ScriptableObject
{
    [Header("Базовая информация")]
    public string equipmentName; // Для карточки UI
    public string description;  // Для карточки UI
    //public ElementType element; // tag для улучшений

    [Header("Визуал")]
    public Sprite icon;                     // Для карточки UI
    public GameObject decorationPrefab; // Префаб декорации башни
    public GameObject[] projectilesPrefabs;      // Префабы снарядов

    [Header("Параметры")]
    public float damage; // Projectile
    public float size; // Projectile
    public float attackCooldown; // Controller
    public float range; // Controller
    public int projectileCount; // Controller

    //[Header("Улучшения")]
    //public UpgradeTree upgradeTree;

    //[Header("Активная способность")]
    //public ActiveAbilityData activeAbility;
}
