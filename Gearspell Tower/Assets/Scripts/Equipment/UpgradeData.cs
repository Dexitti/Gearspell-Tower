using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class UpgradeData : ScriptableObject
{
    public string id = Guid.NewGuid().ToString(); // Для Save
    public UpgradeCardType cardType;
    public string upgradeNameKey;
    public Sprite icon;
    [TextArea] public string descriptionKey;
    public float[] formatArgs;
    public int cost;
}

public enum UpgradeCardType
{
    Equipment,
    Common,
    Fork,
    ActiveAbility
}
