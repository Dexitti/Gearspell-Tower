using System.Collections;
using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class EquipmentController : MonoBehaviour
{
    [SerializeField] protected EquipmentData data;
    private GameObject decorationInstance;
    private Transform towerTransform;

    protected int level = 0;

    protected virtual void OnEnable()
    {
        towerTransform = GameObject.Find("Tower").transform;

        if (data.decorationPrefab)
            decorationInstance = Instantiate(data.decorationPrefab, towerTransform);
        // Instance equipment
    }

    private void Start()
    {
        StartCoroutine(AttackManager());
    }

    IEnumerator AttackManager()
    {
        while (true) {
            Attack();
            
            yield return new WaitForSeconds(data.attackCooldown);
        }
    }

    public abstract void Attack();
    public abstract void Upgrade(int upgradeIndex);
    public abstract void ActivateAbility();
}
