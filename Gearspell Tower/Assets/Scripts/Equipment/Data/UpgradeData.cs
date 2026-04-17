using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeData", menuName = "Scriptable Objects/UpgradeData")]
public class UpgradeData : ScriptableObject
{
    public string id = Guid.NewGuid().ToString(); // Для Save
    public UpgradeCardType cardType;
    public string upgradeName;
    public Sprite icon;
    [TextArea] public string description;
    public int cost = 30;
}

public enum UpgradeCardType
{
    Equipment,
    Common,
    Fork,
    ActiveAbility
}
