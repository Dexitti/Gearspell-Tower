using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class WindmillController : EquipmentController
{
    Vector3 firePoint = Vector3.zero;

    protected override IEnumerator Attack()
    {
        Vector3? target = GetNearestEnemy();
        if (target == null || Vector3.Distance(towerTransform.position, (Vector3)target) > currentRange) yield break;

        firePoint = towerTransform.position + new Vector3(0, 0.55f, 0);
        Vector3 direction = ((Vector3)target - firePoint).normalized;
        firePoint += direction * 0.2f;

        GameObject proj = Instantiate(
            data.projectilesPrefabs[0],
            firePoint,
            Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + Random.Range(-5f, 5f)),
            transform
        );

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

    protected override void Upgrade(int upgradeIndex)
    {
        throw new System.NotImplementedException();
    }

    protected override void ActivateAbility()
    {
        throw new System.NotImplementedException();
    }
}
