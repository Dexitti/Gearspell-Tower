using System;
using System.Collections;
using System.Collections.Generic;
using NUnit;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

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
    Vector3 firePoint = Vector3.zero;
    private bool isBeamActive = false;
    private Coroutine beamCoroutine;

    protected override void OnEnable()
    {
        base.OnEnable();
        firePoint = towerTransform.position + new Vector3(0.713f, 1.2165f);

        CreateBeams();
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

    protected override IEnumerator Attack()
    {
        Vector3 target = GetAverageFromNearestEnemies();
        if (target != Vector3.zero)
        {
            StartBeam(target);
        }
        else StopBeam();

        yield break;
    }

    private Vector3 GetAverageFromNearestEnemies()
    {
        GameObject[] enemyArray = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemyArray.Length == 0) return Vector3.zero;

        Array.Sort(enemyArray, (enemy1, enemy2) =>
            Vector3.Distance(enemy1.transform.position, towerTransform.position)
            .CompareTo(Vector3.Distance(enemy2.transform.position, towerTransform.position))
        );

        foreach (GameObject enemy in enemyArray)
        {
            if (Vector3.Distance(enemy.transform.position, towerTransform.position) <= currentRange)
                if (!nearestEnemies.Contains(enemy))
                    nearestEnemies.Add(enemy);
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
        float[,] pivots = new float[4, 2] {{ -0.03f, 0.04f }, { 0.03f, 0.04f }, { 0.03f, -0.04f }, { -0.03f, -0.04f } };
        Vector3 direction = (target - firePoint).normalized;
        float distanceToTarget = Vector3.Distance(firePoint, target);

        float mergeDistance = Mathf.Lerp(0.1f, distanceToTarget * 0.25f, Mathf.Clamp01(distanceToTarget / currentRange));
        Vector3 mergePoint = firePoint + direction * mergeDistance;

        for (int i = 0; i < 4; i++)
        {
            focusingBeams[i].enabled = true;
            focusingBeams[i].SetPosition(0, firePoint + new Vector3(pivots[i, 0], pivots[i, 1]));
            focusingBeams[i].SetPosition(1, mergePoint);
        }

        mainFreezeBeamRenderer.enabled = true;
        mainFreezeBeamRenderer.SetPosition(0, mergePoint);
        mainFreezeBeamRenderer.SetPosition(1, direction * currentRange);

    }

    private void StopBeam()
    {
        if (!isBeamActive) return;

        isBeamActive = false;
        for (int i = 0; i < 4; i++)
            focusingBeams[i].enabled = false;
        mainFreezeBeamRenderer.enabled = false;

        if (beamCoroutine != null)
        {
            StopCoroutine(beamCoroutine);
            beamCoroutine = null;
        }
    }

    private void OnDisable()
    {
        StopBeam();
        nearestEnemies.Clear();
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
