using System;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    public Action<int, Action<int>> OnBeforeTakeDamage;
    public Action<float, float> OnHealthChanged; // текущее, максимальное
    public Action OnDeath;
    
    [SerializeField] private int maxHealth = 100;
    public int MaxHealth
    {
        get => maxHealth;
        set
        {
            maxHealth = value;
            CurrentHealth = value;
            Debug.Log($"{gameObject}: maxHealth set to {value}");
        }
    }

    private bool isDied = false;
    public bool isAlive => !isDied;

    private int currentHealth;
    public int CurrentHealth
    {
        get => currentHealth;
        private set
        {
            currentHealth = value;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }

    private void OnEnable()
    {
        CurrentHealth = maxHealth;
    }

    public float GetHealthPercentage() => Mathf.Clamp((float)currentHealth / maxHealth, 0, 1);

    public void SetHealth(int value) => CurrentHealth = Mathf.Clamp(value, 0, maxHealth);

    public void TakeDamage(int damage)
    {
        if (isDied) return;

        int finalDamage = damage;
        OnBeforeTakeDamage?.Invoke(damage, (newDamage) => finalDamage = newDamage);

        if (finalDamage <= 0) return;

        CurrentHealth = Math.Max(0, currentHealth - finalDamage);
        Debug.Log($"{gameObject.name} получил {finalDamage} урона");

        if (CurrentHealth <= 0 && !isDied)
        {
            Die();
        }
    }

    public void Heal(int hitpoints)
    {
        if (isDied) return;
        Debug.Log($"{gameObject.name} восстановил {hitpoints} hp");
        CurrentHealth = Math.Min(currentHealth + hitpoints, maxHealth);
    }

    private void Die()
    {
        if (isDied) return;
        isDied = true;

        OnDeath?.Invoke();
        Debug.Log($"{gameObject.name} уничтожен!");
    }

    void OnDrawGizmos()
    {
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.alignment = TextAnchor.MiddleCenter;

        Vector3 textPosition = transform.position + Vector3.up * 0.85f;
        //Handles.Label(textPosition, CurrentHealth.ToString(), style);
    }
}