using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class AntennaController : EquipmentController
{
    Vector3 firePoint = Vector3.zero;
    bool hasEnemies = false;
    bool isAttacking = false;

    private ParticleSystem wavePS;
    private ParticleSystem.MainModule mainModule;
    private ParticleSystem.EmissionModule emissionModule;
    private ParticleSystem.ShapeModule shapeModule;

    private bool hasStun = false;
    private float stunChance = 0.3f;
    private float stunDuration = 0.8f;
    private bool hasKnockback = false;
    private float knockbackForce = 2f;
    private bool noDamageFalloff = false;
    private bool enemyAuras = false;
    private float auraRadius = 1.5f;
    private float auraDamageMultiplier = 0.4f;

    protected override void OnEnable()
    {
        base.OnEnable();
        firePoint = towerTransform.position + new Vector3(0, 1.25f, 0);
        wavePS = GetComponent<ParticleSystem>();
        mainModule = wavePS.main;
        emissionModule = wavePS.emission;
        shapeModule = wavePS.shape;
        SetupVisualParameters();
        wavePS.Emit(0);
    }

    private void SetupVisualParameters()
    {
        Transform rangeBoundary = transform.Find("RangeBoundary");
        rangeBoundary.position = towerTransform.position;
        rangeBoundary.localScale = new Vector3(currentRange, currentRange);

        shapeModule.radius = currentRange * 0.5f;
        mainModule.startSpeed = currentRange * 1.5f;
        mainModule.startLifetime = currentRange / (currentRange * 1.5f);
        emissionModule.SetBurst(0, new ParticleSystem.Burst(
            _time: 0,
            _count: currentProjectileCount,
            _cycleCount: 1,
            _repeatInterval: 0.5f
        ));
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        Transform mortar = towerTransform.Find("Mortars and Parapet(Clone)");
        if (mortar != null)
        {
            Transform antennaColumn = towerTransform.Find("Antenna column(Clone)");
            if (antennaColumn != null)
                Destroy(antennaColumn.gameObject);
        }
    }

    protected override IEnumerator Attack()
    {
        if (isAttacking) yield break;
        isAttacking = true;
        HaveAndDamageEnemiesInRange();

        if (hasEnemies)
            wavePS.Emit(currentProjectileCount);
        SetupVisualParameters();

        yield return new WaitForSeconds(currentAttackCooldown);
        isAttacking = false;
    }

    private void HaveAndDamageEnemiesInRange()
    {
        GameObject[] enemyArray = GameObject.FindGameObjectsWithTag("Enemy");
        hasEnemies = false;

        foreach (var enemy in enemyArray)
        {
            float distance = IsometricExtension.IsoDistance(towerTransform.position, enemy.transform.position);
            if (distance > currentRange) continue;

            hasEnemies = true;
            HealthComponent enemyHP = enemy.GetComponent<HealthComponent>();
            if (enemyHP == null) continue;
            
            int finalDamage = currentDamage;
            if (!noDamageFalloff)
            {
                float falloff = 1f - (distance / currentRange) * 0.7f; // 100% вблизи -> 30% скраю
                finalDamage = Mathf.RoundToInt(currentDamage * falloff);
            }

            enemyHP.TakeDamage(finalDamage);

            // Оглушающая волна
            if (hasStun && UnityEngine.Random.value < stunChance)
            {
                Creature creature = enemy.GetComponent<Creature>();
                if (creature != null)
                    creature.ApplyStun(stunDuration);
            }

            // Импульсор — отталкивание
            if (hasKnockback)
            {
                Vector3 knockDir = (enemy.transform.position - towerTransform.position).normalized;
                enemy.transform.position += knockDir * knockbackForce * 0.3f;
            }
        }

        if (enemyAuras && !HasActiveAbility)
        {
            ApplyEnemyAuras(enemyArray);
        }
    }

    protected override void ApplyEffect(string upgradeId)
    {
        switch (upgradeId)
        {
            case "Antenna_1":
                currentDamage = Mathf.RoundToInt(currentDamage * 1.5f);
                break;

            case "Antenna_2":
                hasStun = true;
                stunChance = 0.1f;
                stunDuration = 0.5f;
                break;

            case "Antenna_3": // fork A
                hasKnockback = true;
                knockbackForce = 2f;
                currentAttackCooldown *= 0.8f;
                break;

            case "Antenna_4": // fork B
                noDamageFalloff = true;
                currentRange *= 1.2f;
                currentDamage = Mathf.RoundToInt(currentDamage * 1.2f);
                break;

            case "Antenna_5":
                enemyAuras = true;
                HasActiveAbility = true;
                break;

            default:
                Debug.LogWarning($"[Antenna] Unknown upgradeId: {upgradeId}");
                break;
        }
    }

    protected override void ActivateAbility()
    {
        if (!HasActiveAbility) return;
        StartCoroutine(CollectivePsychosis());
    }

    private IEnumerator CollectivePsychosis()
    {
        float duration = 5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            ApplyEnemyAuras(enemies);
            elapsed += 0.5f;
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void ApplyEnemyAuras(GameObject[] enemies)
    {
        foreach (var enemy in enemies)
        {
            float dist = IsometricExtension.IsoDistance(towerTransform.position, enemy.transform.position);
            if (dist > currentRange) continue;

            // Урон всем врагам вокруг этого врага
            Collider2D[] nearby = Physics2D.OverlapCircleAll(enemy.transform.position, auraRadius);
            foreach (var col in nearby)
            {
                if (col.CompareTag("Enemy") && col.gameObject != enemy)
                {
                    HealthComponent hp = col.GetComponent<HealthComponent>();
                    if (hp != null)
                        hp.TakeDamage(Mathf.RoundToInt(currentDamage * auraDamageMultiplier));
                }
            }
        }
    }
}
