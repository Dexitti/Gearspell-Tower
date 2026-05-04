using System;
using System.Collections;
using UnityEngine;

public abstract class Creature : MonoBehaviour
{
    public Action<Creature> OnReachedTower;

    [SerializeField] protected CreatureData data;

    [Header("Коллайдеры")]
    [SerializeField] private Collider2D bodyCollider;     // Для физики и получения урона
    //[SerializeField] private Collider2D hitbox;     // -> Trigger для атаки башни

    protected int baseDamage;
    protected float baseSpeed;

    protected HealthComponent healthComponent;
    protected Vector3 towerPosition;

    public CreatureData Data => data;

    protected virtual void Awake()
    {
        baseDamage = data.damage;
        baseSpeed = data.speed;

        healthComponent = GetComponent<HealthComponent>();
        healthComponent.MaxHealth = data.health;

        var drop = GetComponent<DropComponent>();
        if (drop != null)
            drop.SetDrop(data.minGearsDrop, data.maxGearsDrop);
    }

    protected virtual void OnEnable()
    {
        if (G.Tower != null) towerPosition = G.Tower.Position;
        healthComponent.OnHealthChanged += PlayDamageReceivedAnimation;
        healthComponent.OnDeath += PlayDeathAnimation;
    }

    protected virtual void OnDisable()
    {
        healthComponent.OnHealthChanged -= PlayDamageReceivedAnimation;
        healthComponent.OnDeath -= PlayDeathAnimation;
    }

    private void Update()
    {
        if (G.Tower == null) return;
        Move();
    }

    protected abstract void Move();

    private void PlayDamageReceivedAnimation(float currentHealth, float maxHealth)
    {
        
    }

    private void PlayDeathAnimation()
    {
        StartCoroutine(DeathCoroutine());
    }

    IEnumerator DeathCoroutine()
    {
        GetComponent<Collider2D>().enabled = false;
        baseSpeed = 0;
        //animator.Play("die");
        //AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(0.1f);
        G.EventManager?.TriggerEnemyKilled(this);
        gameObject.SetActive(false);
    }
}
