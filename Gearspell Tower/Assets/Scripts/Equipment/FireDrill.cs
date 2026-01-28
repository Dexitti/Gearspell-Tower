using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class FireDrill : BaseEquipment
{
    //    private class ActiveDrill
    //    {
    //        public Vector3 position;
    //        public float spawnTime;
    //        public float lifetime;
    //        public GameObject visualEffect;
    //        public bool hasActiveUpgrade;
    //    }

    //    private List<ActiveDrill> activeDrills = new List<ActiveDrill>();
    //    private int numberOfDrills = 5;
    //    private float drillCooldown = 0.5f;
    //    private float drillLifetime = 3f;
    //    private float burnDamagePerSecond = 10f;

    //    // Улучшения
    //    private bool upgradeIncreasedDrills = false;
    //    private bool upgradeBurnZone = false;
    //    private bool upgradeVolcanicAnvil = false;

    //    public override void Initialize(EquipmentConfig config)
    //    {
    //        base.Initialize(config);
    //        numberOfDrills = 5;
    //    }

    //    //public override void Update(float deltaTime)
    //    //{
    //        //attackTimer += deltaTime;

    //        //if (attackTimer >= (1f / attackSpeed))
    //        //{
    //        //    PerformAttack();
    //        //    attackTimer = 0f;
    //        //}

    //        //// Обновление активных буров
    //        //UpdateActiveDrills(deltaTime);
    //    //}

    //    private void PerformAttack()
    //    {
    //        int drillsToSpawn = numberOfDrills;
    //        if (upgradeIncreasedDrills) drillsToSpawn = 6;

    //        for (int i = 0; i < drillsToSpawn; i++)
    //        {
    //            Vector3 spawnPosition = CalculateDrillPosition();
    //            SpawnDrill(spawnPosition);
    //        }
    //    }

    //    private Vector3 CalculateDrillPosition()
    //    {
    //        // Случайная позиция вокруг башни
    //        float angle = UnityEngine.Random.Range(0f, 360f);
    //        float distance = UnityEngine.Random.Range(range * 0.3f, range);

    //        Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
    //        Vector3 position = towerTransform.position + direction * distance;

    //        // Проверка на валидность позиции (не за стенами и т.д.)
    //        position.y = 0f; // Предполагаем плоскую местность

    //        return position;
    //    }

    //    private void SpawnDrill(Vector3 position)
    //    {
    //        //// Создание визуального эффекта бура
    //        //GameObject drillEffect = UnityEngine.Object.Instantiate(config.impactEffect, position, Quaternion.identity);

    //        //ActiveDrill drill = new ActiveDrill
    //        //{
    //        //    position = position,
    //        //    spawnTime = Time.time,
    //        //    lifetime = drillLifetime,
    //        //    visualEffect = drillEffect
    //        //};

    //        //activeDrills.Add(drill);

    //        //// Нанесение урона при появлении
    //        //ApplyImpactDamage(position);

    //    }

    //    private void ApplyImpactDamage(Vector3 position)
    //    {
    //        //float impactRadius = upgradeIncreasedDrills ? range * 0.5f : range * 0.7f;
    //        //List<Enemy> hitEnemies = GetEnemiesInRange(position, impactRadius);

    //        //foreach (Enemy enemy in hitEnemies)
    //        //{
    //        //    float finalDamage = damage;
    //        //    if (upgradeIncreasedDrills) finalDamage *= 0.7f; // Меньший урон при большем количестве

    //        //    enemy.TakeDamage(finalDamage, DamageType.Fire);

    //        //    if (upgradeVolcanicAnvil)
    //        //    {
    //        //        enemy.ApplyEffect(new StunEffect(1f)); // Оглушение на 1 секунду
    //        //    }
    //        //}
    //    }

    //    private void UpdateActiveDrills(float deltaTime)
    //    {
    //        //for (int i = activeDrills.Count - 1; i >= 0; i--)
    //        //{
    //        //    ActiveDrill drill = activeDrills[i];

    //        //    if (Time.time - drill.spawnTime > drill.lifetime)
    //        //    {
    //        //        // Уничтожение бура
    //        //        Object.Destroy(drill.visualEffect);
    //        //        activeDrills.RemoveAt(i);
    //        //    }
    //        //}
    //    }

    //    public override void ActivateAbility()
    //    {
    //        // Огненный смерч
    //        StartCoroutine(Firestorm());
    //    }

    //    private IEnumerator Firestorm()
    //    {
    //        float abilityDuration = 5f;
    //        float tickRate = 0.2f;
    //        float elapsedTime = 0f;

    //        //while (elapsedTime < abilityDuration)
    //        //{
    //        //    // Создание вихрей между всеми активными бурами
    //        //    for (int i = 0; i < activeDrills.Count; i++)
    //        //    {
    //        //        for (int j = i + 1; j < activeDrills.Count; j++)
    //        //        {
    //        //            CreateFireVortex(activeDrills[i].position, activeDrills[j].position);
    //        //        }
    //        //    }

    //        yield return new WaitForSeconds(tickRate);
    //        elapsedTime += tickRate;
    //        //}
    //    }

    //    public override void Upgrade(int upgradeIndex)
    //    {
    //        level++;

    //        switch (upgradeIndex)
    //        {
    //            case 0: // Увеличение количества буров
    //                upgradeIncreasedDrills = true;
    //                numberOfDrills = 8;
    //                break;

    //            case 1: // Увеличение урона
    //                damage *= 1.5f;
    //                break;

    //            case 2: // Увеличение частоты
    //                attackSpeed *= 1.4f;
    //                break;

    //            case 3: // Развилка: Вулканическая наковальня
    //                upgradeVolcanicAnvil = true;
    //                break;

    //            case 4: // Развилка: Зона горения
    //                upgradeBurnZone = true;
    //                break;
    //        }
    //    }
}
