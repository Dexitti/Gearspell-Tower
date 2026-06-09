using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class WindmillController : EquipmentController
{
    [SerializeField] private Animator animator;
    private int currentDirection = -1;

    Vector3 firePoint = Vector3.zero;
    Vector3 spawnPosition;

    private float spreadAngle = 2f;
    private bool hasPiercing;
    private int pierceCount;
    private float armorPenetration;
    private bool forkATaken = false;
    private bool forkBTaken = false;

    protected override void OnEnable()
    {
        base.OnEnable();
        firePoint = G.Tower.Position + new Vector3(0, 0.25f, 0);
        animator = decorationInstance.GetComponent<Animator>();
    }

    protected override IEnumerator Attack()
    {
        Vector3? target = GetNearestEnemy();
        if (target == null || IsometricExtension.IsoDistance(detectionOrigin, (Vector3)target) > currentRange) yield break;

        Vector3 direction = ((Vector3)target - firePoint).normalized;
        UpdateWindmillDecorationDirection(direction);
        float randomAngle = UnityEngine.Random.Range(-spreadAngle, spreadAngle);

        // Разброс
        direction = Quaternion.Euler(0, 0, randomAngle) * direction;
        spawnPosition = firePoint + direction * 0.4f;

        SpawnProjectile(direction);

        yield break;
    }

    private Vector3? GetNearestEnemy()
    {
        List<GameObject> enemyList = GameObject.FindGameObjectsWithTag("Enemy").ToList();
        enemyList.AddRange(GameObject.FindGameObjectsWithTag("FlyingEnemy"));
        if (enemyList.Count == 0) return null;
        return enemyList
            .OrderBy(enemy => IsometricExtension.IsoDistance(detectionOrigin, enemy.transform.position))
            .FirstOrDefault().transform.position;
    }

    private void UpdateWindmillDecorationDirection(Vector3 direction)
    {
        if (animator == null) return;
        // Конвертируем: 0 = вправо → 0 = вниз
        float angle = -Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;
        if (angle < 0) angle += 360f;

        // Определяем индекс направления (0-7)
        int dirIndex = Mathf.FloorToInt((angle + 22.5f) / 45f) % 8;

        if (dirIndex != currentDirection)
        {
            currentDirection = dirIndex;
            animator.SetInteger("Direction", currentDirection);
        }
    }

    private void SpawnProjectile(Vector3 direction)
    {
        GameObject prefab = forkBTaken ? data.projectilesPrefabs[1] : data.projectilesPrefabs[0];

        GameObject proj = Instantiate(prefab, spawnPosition, Quaternion.identity, transform);
        G.AudioManager.PlaySFX("wind cut", 0.35f);
        if (forkATaken)
        {
            Vector3 scale = proj.transform.localScale;
            scale.x *= 1.33f;
            proj.transform.localScale = scale;
        }

        WindProjectile projScript = proj.GetComponent<WindProjectile>();
        projScript.Direction = direction;
        projScript.Damage = currentDamage;
        projScript.Range = currentRange;

        if (hasPiercing)
            projScript.SetPiercing(pierceCount);
        projScript.SetArmorPenetration(armorPenetration);
    }

    protected override void ApplyEffect(string upgradeId)
    {
        switch (upgradeId)
        {
            case "Windmill_1":
                currentDamage = Mathf.RoundToInt(currentDamage * 2f);
                break;

            case "Windmill_2":
                hasPiercing = true;
                pierceCount = 2;
                armorPenetration = 1f;
                break;

            case "Windmill_3": // fork A
                forkATaken = true;
                spreadAngle = 25f;
                currentAttackCooldown *= 0.65f;
                currentDamage = Mathf.RoundToInt(currentDamage * 0.7f);
                break;

            case "Windmill_4": // fork B
                forkBTaken = true;
                hasPiercing = true;
                pierceCount = 4;
                currentDamage = Mathf.RoundToInt(currentDamage * 1.3f);
                currentAttackCooldown *= 1.2f;
                armorPenetration = 1.15f;
                break;

            case "Windmill_5": // Active
                HasActiveAbility = true;
                break;

            default:
                Debug.LogWarning($"[Windmill] Unknown upgradeId: {upgradeId}");
                break;
        }
    }

    protected override void ActivateAbility()
    {
        if (!HasActiveAbility) return;
        StartCoroutine(Hurricane());
    }

    private IEnumerator Hurricane()
    {
        int totalProjectiles = 20;
        float angleStep = 90f / totalProjectiles;
        Vector3 baseDirection = GetNearestEnemy() != null
            ? ((Vector3)GetNearestEnemy() - firePoint).normalized
            : Vector3.right;
        float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;

        for (int i = 0; i < totalProjectiles; i++)
        {
            float angle = baseAngle - 45f + angleStep * i;
            Vector3 dir = new Vector3(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad),
                0
            );
            SpawnProjectile(dir);
            yield return new WaitForSeconds(0.05f);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(spawnPosition, 0.05f);
    }
}
