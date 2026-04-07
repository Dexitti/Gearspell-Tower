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

    protected override void OnEnable()
    {
        base.OnEnable();
        firePoint = towerTransform.position + new Vector3(0, 0.25f, 0);
        animator = decorationInstance.GetComponent<Animator>();
    }

    protected override IEnumerator Attack()
    {
        Vector3? target = GetNearestEnemy();
        if (target == null || Vector3.Distance(towerTransform.position, (Vector3)target) > currentRange) yield break;

        Vector3 direction = ((Vector3)target - firePoint).normalized;
        UpdateWindmillDecorationDirection(direction);
        spawnPosition = firePoint + direction * 0.4f;

        GameObject proj = Instantiate(
            data.projectilesPrefabs[0],
            spawnPosition,
            Quaternion.identity,
            transform
        );
        WindProjectile projScript = proj.GetComponent<WindProjectile>();
        projScript.Direction = direction;
        projScript.Damage = currentDamage;

        yield break;
    }

    private Vector3? GetNearestEnemy()
    {
        GameObject[] enemyArray = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemyArray.Length == 0) return null;
        return enemyArray
            .OrderBy(enemy => Vector3.Distance(towerTransform.position, enemy.transform.position))
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(spawnPosition, 0.05f);
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
