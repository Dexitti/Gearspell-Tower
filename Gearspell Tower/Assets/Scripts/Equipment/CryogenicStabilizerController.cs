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
    [SerializeField] private float freezeDuration = 3f;
    [SerializeField] private float slowEffect = 0.85f;

    private List<GameObject> nearestEnemies = new List<GameObject>();
    private HashSet<GameObject> enemiesInBeam = new HashSet<GameObject>();
    Vector3 firePoint = Vector3.zero;
    GameObject attackArea;
    private BoxCollider2D hitCollider;
    private bool isBeamActive = false;

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
            if (Vector3.Distance(enemy.transform.position, towerTransform.position) <= currentRange)
                return true;
        }
        return false;
    }

    private Vector3 GetAverageFromNearestEnemies()
    {
        GameObject[] enemyArray = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemyArray.Length == 0) return Vector3.zero;

        Array.Sort(enemyArray, (enemy1, enemy2) =>
            Vector3.Distance(enemy1.transform.position, towerTransform.position)
            .CompareTo(Vector3.Distance(enemy2.transform.position, towerTransform.position))
        );

        nearestEnemies.Clear();

        foreach (GameObject enemy in enemyArray)
        {
            if (enemy != null && Vector3.Distance(enemy.transform.position, towerTransform.position) <= currentRange)
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
        float[,] pivots = new float[4, 2] {{ -0.03f, 0.04f }, { 0.03f, 0.04f }, { 0.03f, -0.04f }, { -0.03f, -0.04f } };
        Vector3 direction = (target - firePoint).normalized;
        float distanceToTarget = Vector3.Distance(firePoint, target);

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
        float length = Vector3.Distance(mergePoint, beamEnd * 1.1f);
        float angle = Mathf.Atan2(beamEnd.y - mergePoint.y, beamEnd.x - mergePoint.x) * Mathf.Rad2Deg;

        hitCollider.transform.position = center;
        hitCollider.size = new Vector2(length, 0.5f);
        hitCollider.transform.rotation = Quaternion.Euler(0, 0, angle);
        hitCollider.enabled = true;
    }

    private void StopBeam()
    {
        if (!isBeamActive) return;

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
            if (enemy != null)
            {
                HealthComponent health = enemy.GetComponent<HealthComponent>();
                if (health != null)
                {
                    health.TakeDamage(currentDamage);

                    if (freezeImpactEffect != null)
                        Instantiate(freezeImpactEffect, enemy.transform.position, Quaternion.identity);
                }
            }
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
        throw new NotImplementedException();
    }

    protected override void Upgrade(int upgradeIndex)
    {
        throw new System.NotImplementedException();
    }

    protected override void ActivateAbility()
    {
        throw new System.NotImplementedException();
    }
}
