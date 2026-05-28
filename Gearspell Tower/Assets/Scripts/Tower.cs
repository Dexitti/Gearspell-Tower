using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private int regeneration = 1;
    private HealthComponent healthComponent;
    private bool regenerationStarted;

    public Vector3 Position => transform.position;
    public void SetRegeneration(int value) => regeneration = value;

    private void Awake()
    {
        healthComponent = GetComponent<HealthComponent>();
        if (healthComponent == null) return;

        healthComponent.OnHealthChanged += OnHealthChanged;
        healthComponent.OnDeath += OnDeath;
    }

    public void Initialize()
    {
        if (healthComponent == null) return;

        healthComponent.SetMaxHealth(1000);
        OnHealthChanged(healthComponent.CurrentHealth, healthComponent.MaxHealth);

        if (!regenerationStarted)
        {
            regenerationStarted = true;
            StartCoroutine(Regenerate());
        }
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
