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
    public int currentDamage;
    public float currentSize;
    public float currentAttackCooldown;
    public float currentRange;
    public int currentProjectileCount;

    protected virtual void OnEnable()
    {
        towerTransform = GameObject.Find("Tower").transform;
        GameObject decoration = data.decorationPrefab;

        if (decoration)
        {
            if (decoration.name == "Mortars and Parapet")
            {
                Transform roof = towerTransform.Find("Roof");
                if (roof != null)
                    roof.gameObject.SetActive(false);
            }
            else if (decoration.name == "Antenna")
            {
                Transform mortarAndParapet = towerTransform.Find("Mortars and Parapet(Clone)");
                if (mortarAndParapet != null)
                {
                    GameObject column = data.projectilesPrefabs[1];
                    if (column != null && column.name == "Antenna column")
                        Instantiate(column, towerTransform);
                }
            }
            decorationInstance = Instantiate(decoration, towerTransform);
        }
        // Instance equipment
    }

    protected virtual void OnDisable()
    {
        if (decorationInstance != null)
            Destroy(decorationInstance);
    }

    private void Start()
    {
        currentDamage = (int)Mathf.Round(data.damage);
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
