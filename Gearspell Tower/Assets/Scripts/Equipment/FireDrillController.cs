using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Analytics;
using UnityEngine;
using UnityEngine.UIElements;

public class FireDrillController : EquipmentController
{
    private Queue<GameObject> shotPool = new Queue<GameObject>();
    private Queue<GameObject> projPool = new Queue<GameObject>();
    private List<Vector3> availableEnemies = new List<Vector3>();
    private List<Vector3> firePoints = new List<Vector3>();

    private float height = 6f;

    private float drillSizeMultiplier = 1f;
    private bool fireGround = false;
    private float fireGroundDuration = 3f;
    private bool hasStun = false;
    private float stunChance = 0f;
    private float stunDuration = 0f;

    protected override void OnEnable()
    {
        base.OnEnable();
        RebuildPools();
    }

    private void RebuildPools()
    {
        foreach (var shot in shotPool)
            if (shot != null) Destroy(shot);
        foreach (var drill in projPool)
            if (drill != null) Destroy(drill);
        shotPool.Clear();
        projPool.Clear();

        for (int i = 0; i < currentProjectileCount; i++)
        {
            GameObject shot = Instantiate(data.projectilesPrefabs[0], transform);
            shot.SetActive(false);
            shotPool.Enqueue(shot);
        }

        for (int i = 0; i < currentProjectileCount; i++)
        {
            GameObject proj = Instantiate(data.projectilesPrefabs[1], transform);
            proj.SetActive(false);
            projPool.Enqueue(proj);
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        Transform roof = towerTransform.Find("Roof");
        if (roof != null)
            roof.gameObject.SetActive(true);

        Transform antennaColumn = towerTransform.Find("Antenna column(Clone)");
        if (antennaColumn != null)
            Destroy(antennaColumn.gameObject);
    }

    protected override IEnumerator Attack()
    {
        availableEnemies.Clear();
        firePoints.Clear();

        InitializeFirePoints();
        if (firePoints.Count == 0) yield break;

        FireShells();
        yield return new WaitForSeconds(0.75f);

        FireDrills();
    }

    private void InitializeFirePoints()
    {
        GameObject[] enemyArray = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemyArray.Length == 0)
        {
            Debug.LogWarning("Нет врагов на сцене");
            return;
        }

        foreach (var enemy in enemyArray)
        {
            if (IsometricExtension.IsoDistance(towerTransform.position, enemy.transform.position) <= currentRange)
            {
                //Vector3 toTower = (towerTransform.position - enemy.transform.position).normalized;
                //Vector3 predictPos = enemy.transform.position + toTower * 0.5f;
                //availableEnemies.Add(predictPos);
                availableEnemies.Add(enemy.transform.position);
            }
        }

        List<int> positions = new List<int>();
        for (int i = 0; i < Mathf.Min(currentProjectileCount, availableEnemies.Count); i++)
        {
            while (true)
            {
                int randomInt = UnityEngine.Random.Range(0, availableEnemies.Count);
                if (!positions.Contains(randomInt))
                {
                    positions.Add(randomInt);
                    Vector3 randomEnemy = availableEnemies[randomInt];
                    firePoints.Add(randomEnemy + Vector3.up * height);
                    break;
                }
            }
        }
    }

    private void FireShells()
    {
        for (int i = 0; i < Mathf.Min(currentProjectileCount, availableEnemies.Count); i++)
        {
            GameObject shot;
            if (shotPool.Count > 0)
            {
                shot = shotPool.Dequeue();
                shot.SetActive(true);

            }
            else shot = Instantiate(data.projectilesPrefabs[0], transform);
            shot.transform.position = towerTransform.position + Vector3.up * 0.65f;
            if (firePoints[i].x <= towerTransform.position.x)
                shot.transform.Rotate(0f, 0f, UnityEngine.Random.Range(2f, 25f));
            else shot.transform.Rotate(0f, 0f, UnityEngine.Random.Range(-25f, -2f));
        }
    }

    private void FireDrills()
    {
        for (int i = 0; i < Mathf.Min(currentProjectileCount, availableEnemies.Count); i++)
        {
            GameObject drill;
            if (projPool.Count > 0)
            {
                drill = projPool.Dequeue();
                drill.SetActive(true);
            }
            else drill = Instantiate(data.projectilesPrefabs[1], transform);
            drill.transform.position = firePoints[i];
            drill.transform.localScale *= drillSizeMultiplier;

            FireDrillProjectile projScript = drill.GetComponent<FireDrillProjectile>();
            projScript.Damage = currentDamage;
            projScript.SetStun(hasStun, stunChance, stunDuration);
        }
    }

    protected override void ApplyEffect(string upgradeId)
    {
        switch (upgradeId)
        {
            case "FireDrill_1":
                currentProjectileCount = Mathf.RoundToInt(currentProjectileCount * 1.7f);
                break;

            case "FireDrill_2":
                currentDamage = Mathf.RoundToInt(currentDamage * 1.5f);
                drillSizeMultiplier = 1.4f;
                break;

            case "FireDrill_3": // fork A
                currentProjectileCount = Mathf.RoundToInt(currentProjectileCount * 2f);
                drillSizeMultiplier = 0.55f;
                currentDamage = Mathf.RoundToInt(currentDamage * 0.55f);
                break;

            case "FireDrill_4": // fork B
                currentProjectileCount = Mathf.RoundToInt(currentProjectileCount * 0.8f);
                fireGround = true;
                hasStun = true;
                stunChance = 0.15f;
                stunDuration = 0.75f;
                break;

            case "FireDrill_5":
                HasActiveAbility = true;
                break;

            default:
                Debug.LogWarning($"[FireDrill] Unknown upgradeId: {upgradeId}");
                break;
        }

        RebuildPools();
    }

    protected override void ActivateAbility()
    {
        if (!HasActiveAbility) return;
        StartCoroutine(FireStorm());
    }

    private IEnumerator FireStorm()
    {
        // TODO
        yield return null;
    }
}
