using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UpgradeData : ScriptableObject
{
    public string id = Guid.NewGuid().ToString(); // Для Save
    public UpgradeCardType cardType;
    public string upgradeName;
    public Sprite icon;
    [TextArea] public string description;
    public int cost;
}

public enum UpgradeCardType
{
    Equipment,
    Common,
    Fork,
    ActiveAbility
}
