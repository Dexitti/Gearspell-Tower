using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class FireDrillController : EquipmentController
{
    private Queue<GameObject> shotPool = new Queue<GameObject>();
    private Queue<GameObject> projPool = new Queue<GameObject>();
    private List<Vector3> availableEnemies = new List<Vector3>();
    private List<Vector3> firePoints = new List<Vector3>();

    private float height = 6f;

    protected override void OnEnable()
    {
        base.OnEnable();
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
        //yield return new WaitWhile(() => IsSecondActionRunning());

        //OnInteractionComplete?.Invoke();
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
            if (Vector3.Distance(towerTransform.position, enemy.transform.position) <= currentRange)
            {
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
            if (projPool.Count > 0) {
                drill = projPool.Dequeue();
                drill.SetActive(true);
            }
            else drill = Instantiate(data.projectilesPrefabs[1], transform);
            drill.transform.position = firePoints[i];
            FireDrillProjectile projScript = drill.GetComponent<FireDrillProjectile>();
            projScript.Damage = currentDamage;
        }
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
