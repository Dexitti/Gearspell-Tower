using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseEquipment : MonoBehaviour
{
    protected EquipmentConfig config;
    protected Transform towerTransform;
    protected float attackTimer;
    protected List<Creature> currentTargets;

    // Статы
    protected int level = 0;
    protected int projAmount = 1;
    protected int damage;
    protected float attackSpeed;
    protected float range;
    protected float size = 1;

    //    public virtual void Initialize(EquipmentConfig config)
    //    {
    //        this.config = config;
    //        this.damage = config.baseDamage;
    //        this.attackSpeed = config.baseAttackSpeed;
    //        this.range = config.baseRange;
    //        this.size = config.baseSize;
    //        this.attackTimer = 0f;
    //    }

    //    public virtual void SetTargets(List<Creature> enemies)
    //    {
    //        currentTargets = enemies;
    //    }

    //    //public abstract void Update(float deltaTime);
    //    //public abstract void ActivateAbility();
    //    //public abstract void Upgrade(int upgradeIndex);

    //    //protected List<Creature> GetEnemiesInRange(Vector3 position, float radius)
    //    //{
    //    //    List<Creature> enemiesInRange = new List<Creature>();
    //    //    foreach (Creature enemy in currentTargets)
    //    //    {
    //    //        if (enemy != null && Vector3.Distance(position, enemy.transform.position) <= radius)
    //    //        {
    //    //            enemiesInRange.Add(enemy);
    //    //        }
    //    //    }
    //    //    return enemiesInRange;
    //    //}
}

[System.Serializable]
    public class EquipmentConfig
    {
        public string equipmentName;
        public float baseDamage;
        public float baseAttackSpeed;
        public float baseRange;
        public float baseSize;
        public GameObject projectilePrefab;
    }