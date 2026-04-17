using UnityEngine;

[CreateAssetMenu(fileName = "CreatureData", menuName = "Scriptable Objects/CreatureData")]
public class CreatureData : ScriptableObject
{
    [Header("Базовая информация")]
    public string creatureName;
    public CreatureType type;
    public string shortDescription; // Для глоссария (если будет)

    [Header("Визуал")]
    public Sprite icon;                  // Для глоссария (если будет)
    public GameObject enemyPrefab;       // Префаб врага
    public GameObject[] attackPrefabs;   // Префабы атак

    [Header("Параметры")]
    public int health;
    public int damage;
    public float speed;
    public int minGearsDrop;
    public int maxGearsDrop;
}

public enum CreatureType
{
    Easy,
    Regular,
    Medium,
    Heavy,
    Boss
}
