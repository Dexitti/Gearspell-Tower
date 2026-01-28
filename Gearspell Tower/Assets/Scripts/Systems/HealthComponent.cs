using System;
using UnityEditor;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;

    private int currentHealth;
    private bool isDied = false;

    // События
    public Action<float, float> OnHealthChanged; // текущее, максимальное
    public Action OnDeath;

    public bool isAlive => !isDied;
    public int MaxHealth => maxHealth;

    private void OnEnable()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        //hitDelay -= Time.fixedDeltaTime;
    }

    public float GetHealthPercentage() => Mathf.Clamp((float)currentHealth / maxHealth, 0, 1);

    public void TakeDamage(int damage)
    {
        if (isDied) return;
        Debug.Log($"{gameObject} получил {damage} урона");
        currentHealth = Math.Max(0, currentHealth - damage);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        if (currentHealth <= 0 && !isDied)
        {
            Die();
        }
    }

    public void Heal(int hitpoints)
    {
        if (isDied) return;
        Debug.Log($"{gameObject} восстановил {hitpoints} hp");
        currentHealth = Math.Min(currentHealth + hitpoints, maxHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    private void Die()
    {
        if (isDied) return;
        isDied = true;

        OnDeath?.Invoke();
        Debug.Log($"{gameObject} уничтожен!");
    }

    void OnDrawGizmos()
    {
        GUIStyle style = new GUIStyle();
        style.normal.textColor = Color.white;
        style.alignment = TextAnchor.MiddleCenter;

        Vector3 textPosition = transform.position + Vector3.up * 0.85f;
        Handles.Label(textPosition, currentHealth.ToString(), style);

    }
}