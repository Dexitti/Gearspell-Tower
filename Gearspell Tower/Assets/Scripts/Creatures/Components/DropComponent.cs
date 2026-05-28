using System;
using UnityEngine;

public class DropComponent : MonoBehaviour
{
    [SerializeField] private GameObject gearsDropPrefab;
    [SerializeField] private HealthComponent health;

    private int gears;

    private void Awake()
    {
        health = GetComponent<HealthComponent>();
        if (health != null)
            health.OnDeath += OnDeath;
    }

    private void OnDestroy()
    {
        if (health != null)
            health.OnDeath -= OnDeath;
    }

    public void SetDrop(int minGears, int maxGears)
    {
        gears = UnityEngine.Random.Range(minGears, maxGears + 1);
    }

    public void SetDrop(int gears)
    {
        this.gears = gears;
    }

    private void OnDeath()
    {
        Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * 0.5f;
        Vector3 dropPosition = transform.position + new Vector3(randomOffset.x, randomOffset.y);

        GameObject gear = Instantiate(gearsDropPrefab, dropPosition, Quaternion.identity);
        gear.GetComponent<GearsDrop>().GearsNumber = gears;
    }
}
