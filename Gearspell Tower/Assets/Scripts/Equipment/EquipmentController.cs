using System.Collections;
using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.UIElements;

public abstract class EquipmentController : MonoBehaviour
{
    [SerializeField] public EquipmentData data;
    private GameObject decorationInstance;
    protected Transform towerTransform;

    protected int level = 0;
    public float currentDamage;
    public float currentSize;
    public float currentAttackCooldown;
    public float currentRange;
    public int currentProjectileCount;

    protected virtual void OnEnable()
    {
        towerTransform = GameObject.Find("Tower").transform;

        if (data.decorationPrefab)
            decorationInstance = Instantiate(data.decorationPrefab, towerTransform);
        // Instance equipment
    }

    private void Start()
    {
        currentDamage = data.damage;
        currentSize = data.size;
        currentAttackCooldown = data.attackCooldown;
        currentRange = data.range;
        currentProjectileCount = data.projectileCount;
        StartCoroutine(AttackManager());
    }

    IEnumerator AttackManager()
    {
        while (true) {
            yield return StartCoroutine(Attack());


            yield return new WaitForSeconds(data.attackCooldown);
        }
    }

    protected abstract IEnumerator Attack();
    protected abstract void Upgrade(int upgradeIndex);
    protected abstract void ActivateAbility();
}
