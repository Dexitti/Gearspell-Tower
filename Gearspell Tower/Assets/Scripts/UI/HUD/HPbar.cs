using UnityEngine;
using UnityEngine.UI;

public class HPbar : MonoBehaviour
{
    private HealthComponent towerHealthComponent;
    private Scrollbar healthBar;

    void Start()
    {
        towerHealthComponent = GameObject.Find("Tower").GetComponent<HealthComponent>();
        if (healthBar == null)
            healthBar = GetComponent<Scrollbar>();

        towerHealthComponent.OnHealthChanged += UpdateHealthBar;
    }

    private void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        healthBar.size = currentHealth / maxHealth;
        healthBar.value = 1f;
    }

    private void OnDestroy()
    {
        if (towerHealthComponent != null)
        {
            towerHealthComponent.OnHealthChanged -= UpdateHealthBar;
        }
    }
}
