using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private int regeneration = 1;
    private HealthComponent healthComponent;

    public Vector3 Position => transform.position;

    // Модификаторы из систем
    //private float globalDamageMultiplier = 1f;
    //private float globalAttackSpeedMultiplier = 1f;
    //private float globalActiveCooldownReduction = 0f;
    //private float globalProjSize = 1f;
    //private float globalConstructCooldownReduction = 0f;

    private void Start()
    {
        healthComponent = GetComponent<HealthComponent>();
        if (healthComponent != null)
        {
            healthComponent.MaxHealth = 1000;
            healthComponent.OnHealthChanged += OnHealthChanged;
            healthComponent.OnDeath += OnDeath;
        }

        StartCoroutine(Regenerate());
    }

    private void OnHealthChanged(float current, float max)
    {
        G.EventManager?.TriggerTowerHealthChanged(current, max);
    }

    private void OnDeath()
    {
        G.EventManager?.TriggerTowerDestroyed();
        G.GameManager?.GameOver();
    }

    IEnumerator Regenerate()
    {
        while (true)
        {
            if (healthComponent != null && healthComponent.isAlive)
                healthComponent.Heal(regeneration);
            yield return new WaitForSeconds(1f);
        }
    }


    private void OnDestroy()
    {
        if (healthComponent != null)
        {
            healthComponent.OnHealthChanged -= OnHealthChanged;
            healthComponent.OnDeath -= OnDeath;
        }
    }
}
