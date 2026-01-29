using System.Collections;
using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.UIElements;

public class FireDrillController : EquipmentController
{
    private List<GameObject> projPool = new List<GameObject>();

    protected override void OnEnable()
    {
        base.OnEnable();
        foreach (GameObject projPrefab in data.projectilesPrefabs)
        {
            for (int i = 0; i < data.projectileCount; i++)
            {
                projPool[i] = Instantiate(projPrefab);
            }
        }
    }

    public override void Attack()
    {
        
    }

    public override void Upgrade(int upgradeIndex)
    {
        throw new System.NotImplementedException();
    }

    public override void ActivateAbility()
    {
        throw new System.NotImplementedException();
    }
}
