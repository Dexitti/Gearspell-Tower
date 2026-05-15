using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Equipment;
using NUnit;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.GraphicsBuffer;

public class CryogenicStabilizerController : EquipmentController
{
    private LineRenderer mainFreezeBeamRenderer;
    private LineRenderer[] focusingBeams = new LineRenderer[4];
    [SerializeField] private GameObject freezeImpactEffect;

    [Header("Луч")]
    [SerializeField] private float beamWidth = 0.12f;
    private float slowMultiplier = 0.85f;

    private List<GameObject> nearestEnemies = new List<GameObject>();
    private HashSet<GameObject> enemiesInBeam = new HashSet<GameObject>();
    Vector3 firePoint = Vector3.zero;
    GameObject attackArea;
    private BoxCollider2D hitCollider;
    private bool isBeamActive = false;

    private bool сoldGround = false;
    private float coldGroundDuration = 3f;
    private Vector3 lastBeamDirection;
    private bool increasedDamageToSlowed = false;
    private float damageMultiplierToSlowed = 1.5f;
    private bool isConeBeam = false;
    private float coneAngle = 33f;
    private float freezeDuration = 3f;

    protected override void OnEnable()
    {
        base.OnEnable();
        firePoint = towerTransform.position + new Vector3(0.713f, 1.2165f);
        CreateBeams();
        CreateCollider();
    }

    private void Update()
    {
        if (!isBeamActive) return;

        Vector3 target = GetAverageFromNearestEnemies();
        if (target != Vector3.zero) UpdateBeamDirection(target);
        else StopBeam();
    }

    private void CreateBeams()
    {
        Material beamMaterial = new Material(Shader.Find("Sprites/Default"));
        beamMaterial.color = Color.cyan;
        beamMaterial.EnableKeyword("_EMISSION");
        beamMaterial.SetColor("_EmissionColor", new Color(0.1f, 0.3f, 0.5f));

        for (int i = 0; i < 4; i++)
        {
            GameObject beamObj = new GameObject($"SubBeam {i}");
            beamObj.transform.SetParent(transform);
            beamObj.transform.localPosition = Vector3.zero;

            focusingBeams[i] = beamObj.AddComponent<LineRenderer>();
            focusingBeams[i].startWidth = beamWidth * 0.09f;
            focusingBeams[i].endWidth = beamWidth * 0.98f;
            focusingBeams[i].positionCount = 2;
            focusingBeams[i].material = beamMaterial;
            focusingBeams[i].startColor = new Color(0.98f, 0.99f, 1f, 1f); // Бледно-голубой
            focusingBeams[i].endColor = new Color(0f, 1f, 1f, 1f); // Циан
            focusingBeams[i].numCapVertices = 10;
            focusingBeams[i].sortingLayerName = "Effects";
            focusingBeams[i].enabled = false;
        }

        if (mainFreezeBeamRenderer == null)
        {
            mainFreezeBeamRenderer = gameObject.AddComponent<LineRenderer>();
            mainFreezeBeamRenderer.startWidth = beamWidth;
            mainFreezeBeamRenderer.endWidth = beamWidth;
            mainFreezeBeamRenderer.positionCount = 2;
            mainFreezeBeamRenderer.material = beamMaterial;
            mainFreezeBeamRenderer.startColor = new Color(0f, 1f, 1f, 1f); // Циан
            mainFreezeBeamRenderer.endColor = new Color(0f, 0.5f, 1f, 0.8f); // Синий
            mainFreezeBeamRenderer.numCapVertices = 10;
            mainFreezeBeamRenderer.sortingLayerName = "Effects";
            mainFreezeBeamRenderer.enabled = false;
        }

        // Создать эффект
    }

    private void CreateCollider()
    {
        attackArea = new GameObject("HitCollider");
        attackArea.transform.SetParent(transform);
        attackArea.transform.localPosition = Vector3.zero;
            
        hitCollider = attackArea.AddComponent<BoxCollider2D>();
        hitCollider.isTrigger = true;
        hitCollider.enabled = false;

        CryogenicBeam trigger = hitCollider.AddComponent<CryogenicBeam>();
        trigger.SetController(this);
    }

    protected override IEnumerator Attack()
    {
        Vector3 target = GetAverageFromNearestEnemies();

        if (target != Vector3.zero)
        {
            StartBeam(target);

            float damageTimer = 0f;
            while (isBeamActive && AreEnemiesInRange())
            {
                damageTimer += Time.deltaTime;
                if (damageTimer >= currentAttackCooldown)
                {
                    DealDamage();
                    damageTimer = 0f;
                }
                yield return null;
            }
            StopBeam();
        }
        else StopBeam();
        yield break;
    }

    private bool AreEnemiesInRange()
    {
        GameObject[] enemyArray = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemyArray)
        {
            if (IsometricExtension.IsoDistance(enemy.transform.position, towerTransform.position) <= currentRange)
                return true;
        }
        return false;
    }

    private Vector3 GetAverageFromNearestEnemies()
    {
        GameObject[] enemyArray = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemyArray.Length == 0) return Vector3.zero;

        Array.Sort(enemyArray, (enemy1, enemy2) =>
            IsometricExtension.IsoDistance(enemy1.transform.position, towerTransform.position)
            .CompareTo(IsometricExtension.IsoDistance(enemy2.transform.position, towerTransform.position))
        );

        nearestEnemies.Clear();

        foreach (GameObject enemy in enemyArray)
        {
            if (enemy != null && IsometricExtension.IsoDistance(enemy.transform.position, towerTransform.position) <= currentRange)
            {
                if (!nearestEnemies.Contains(enemy))
                    nearestEnemies.Add(enemy);
            }
        }

        switch (nearestEnemies.Count)
        {
            case 0: return Vector3.zero;
            case 1:
                return nearestEnemies[0].transform.position;
            case 2:
                return nearestEnemies[1].transform.position;
            case > 2:
                return nearestEnemies[2].transform.position;
            default: return Vector3.zero;
        }
    }

    private void StartBeam(Vector3 target)
    {
        isBeamActive = true;
        UpdateBeamDirection(target);
    }

    private void UpdateBeamDirection(Vector3 target)
    {
        Vector3 direction = (target - firePoint).normalized;
        lastBeamDirection = direction; // Для холодной земли
        float distanceToTarget = IsometricExtension.IsoDistance(firePoint, target);

        if (isConeBeam)
        {
            // Конус вместо луча
            float coneHalf = coneAngle * 0.5f * Mathf.Deg2Rad;
            float coneLength = currentRange;
            Vector3 coneEnd = firePoint + direction * coneLength;
            float coneWidth = Mathf.Tan(coneHalf) * coneLength;

            // Настройка коллайдера как сектора
            if (hitCollider != null)
            {
                hitCollider.transform.position = firePoint + direction * coneLength * 0.5f;
                hitCollider.size = new Vector2(coneLength, coneWidth * 2f);
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                hitCollider.transform.rotation = Quaternion.Euler(0, 0, angle);
                hitCollider.enabled = true;
            }

            // Визуал: широкий луч
            mainFreezeBeamRenderer.enabled = true;
            mainFreezeBeamRenderer.startWidth = coneWidth;
            mainFreezeBeamRenderer.endWidth = coneWidth * 2.5f;
            mainFreezeBeamRenderer.SetPosition(0, firePoint);
            mainFreezeBeamRenderer.SetPosition(1, coneEnd);

            for (int i = 0; i < 4; i++)
                focusingBeams[i].enabled = false;
        }
        else
        {
            float[,] pivots = new float[4, 2] { { -0.03f, 0.04f }, { 0.03f, 0.04f }, { 0.03f, -0.04f }, { -0.03f, -0.04f } };
            float mergeDistance = Mathf.Lerp(0.1f, distanceToTarget * 0.25f, Mathf.Clamp01(distanceToTarget / currentRange));
            Vector3 mergePoint = firePoint + direction * mergeDistance;
            Vector3 beamEnd = firePoint + direction * currentRange;

            for (int i = 0; i < 4; i++)
            {
                focusingBeams[i].enabled = true;
                focusingBeams[i].SetPosition(0, firePoint + new Vector3(pivots[i, 0], pivots[i, 1]));
                focusingBeams[i].SetPosition(1, mergePoint);
            }

            mainFreezeBeamRenderer.enabled = true;
            mainFreezeBeamRenderer.SetPosition(0, mergePoint);
            mainFreezeBeamRenderer.SetPosition(1, beamEnd);

            // Настройка коллайдера
            if (hitCollider == null) return;

            Vector3 center = (mergePoint + beamEnd) / 2;
            float length = IsometricExtension.IsoDistance(mergePoint, beamEnd * 1.1f);
            float angle = Mathf.Atan2(beamEnd.y - mergePoint.y, beamEnd.x - mergePoint.x) * Mathf.Rad2Deg;

            hitCollider.transform.position = center;
            hitCollider.size = new Vector2(length, 0.5f);
            hitCollider.transform.rotation = Quaternion.Euler(0, 0, angle);
            hitCollider.enabled = true;
        }
    }

    private void StopBeam()
    {
        if (!isBeamActive) return;

        if (сoldGround && enemiesInBeam.Count > 0)
        {
            Vector3 groundCenter = firePoint + lastBeamDirection * currentRange * 0.7f;
            CreateColdGround(groundCenter);
        }

        isBeamActive = false;

        if (hitCollider != null)
            hitCollider.enabled = false;

        for (int i = 0; i < 4; i++)
            focusingBeams[i].enabled = false;
        mainFreezeBeamRenderer.enabled = false;

        enemiesInBeam.Clear();
    }

    private void DealDamage()
    {
        GameObject[] enemies = new GameObject[enemiesInBeam.Count];
        enemiesInBeam.CopyTo(enemies);

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null) continue;

            HealthComponent health = enemy.GetComponent<HealthComponent>();
            if (health != null)
            {
                int finalDamage = currentDamage;
                if (increasedDamageToSlowed)
                    finalDamage = Mathf.RoundToInt(finalDamage * damageMultiplierToSlowed);

                health.TakeDamage(finalDamage);

                if (freezeImpactEffect != null)
                    Instantiate(freezeImpactEffect, enemy.transform.position, Quaternion.identity);
            }

            Creature creature = enemy.GetComponent<Creature>();
            if (creature != null)
                creature.ApplySlow(slowMultiplier, 0.5f);
        }
    }

    public void AddEnemyToBeam(GameObject enemy)
    {
        if (!enemiesInBeam.Contains(enemy))
            enemiesInBeam.Add(enemy);
    }

    public void RemoveEnemyFromBeam(GameObject enemy)
    {
        if (enemiesInBeam.Contains(enemy))
            enemiesInBeam.Remove(enemy);
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        StopBeam();
        nearestEnemies.Clear();
        enemiesInBeam.Clear();
    }

    protected override void ApplyEffect(string upgradeId)
    {
        switch (upgradeId)
        {
            case "Cryogenic_1":
                slowMultiplier = 0.5f;
                break;

            case "Cryogenic_2":
                сoldGround = true;
                coldGroundDuration = 4f;
                break;

            case "Cryogenic_3": // fork A
                increasedDamageToSlowed = true;
                damageMultiplierToSlowed = 2f;
                break;

            case "Cryogenic_4": // fork B
                isConeBeam = true;
                coneAngle = 40f;
                currentRange *= 0.45f;
                break;

            case "Cryogenic_5":
                HasActiveAbility = true;
                break;

            default:
                Debug.LogWarning($"[Cryogenic] Unknown upgradeId: {upgradeId}");
                break;
        }
    }

    private void CreateColdGround(Vector3 position)
    {
        // Спрайт земли из prefabs
        GameObject ground = null;
        if (data.projectilesPrefabs[1] != null)
        {
            ground = Instantiate(data.projectilesPrefabs[1], position, Quaternion.identity);
            ground.transform.localScale = Vector3.one * currentRange * 0.75f;
        }
        else
        {
            // Fallback: пустой объект с коллайдером
            ground = new GameObject("ColdGround");
            ground.transform.position = position;
            CircleCollider2D col = ground.AddComponent<CircleCollider2D>();
            col.radius = currentRange * 0.5f;
            col.isTrigger = true;
            SpriteRenderer sr = ground.AddComponent<SpriteRenderer>();
            sr.sprite = null;
            sr.color = new Color(0.5f, 0.8f, 1f, 0.3f);
        }

        Destroy(ground, coldGroundDuration);
        StartCoroutine(SlowEnemiesOnGround(position, coldGroundDuration));
    }

    private IEnumerator SlowEnemiesOnGround(Vector3 center, float duration)
    {
        float elapsed = 0f;
        float tickRate = 0.3f;
        float radius = currentRange * 0.5f;

        while (elapsed < duration)
        {
            Collider2D[] enemies = Physics2D.OverlapCircleAll(center, radius);
            foreach (var col in enemies)
            {
                if (col.CompareTag("Enemy"))
                {
                    Creature creature = col.GetComponent<Creature>();
                    if (creature != null)
                        creature.ApplySlow(slowMultiplier * 1.3f, tickRate + 0.1f);
                }
            }
            elapsed += tickRate;
            yield return new WaitForSeconds(tickRate);
        }
    }

    protected override void ActivateAbility()
    {
        if (!HasActiveAbility) return;
        StartCoroutine(AbsoluteZero());
    }

    private IEnumerator AbsoluteZero()
    {
        // Эффект льда и лед на каждого врага
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemies)
        {
            if (IsometricExtension.IsoDistance(towerTransform.position, enemy.transform.position) <= currentRange)
            {
                Creature creature = enemy.GetComponent<Creature>();
                if (creature != null)
                    creature.ApplyStun(freezeDuration);
            }
        }

        yield return new WaitForSeconds(0.1f);
    }
}
