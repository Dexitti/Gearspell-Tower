using System;
using System.Collections;
using UnityEngine;
using UnityEngine.U2D;

public abstract class CreatureController : MonoBehaviour
{
    [SerializeField] protected CreatureData data;
    [SerializeField] protected float attackCooldown = 1f;

    protected SpriteRenderer sprite;
    protected HealthComponent health;
    protected int currentDamage;
    protected float currentSpeed;
    protected Vector3 towerPosition;
    protected bool isStunned = false;
    protected float slowMultiplier = 1f;

    private Coroutine slowCoroutine;

    public CreatureData Data => data;

    protected virtual void Awake()
    {
        currentDamage = data.damage;
        currentSpeed = data.speed;
        sprite = GetComponent<SpriteRenderer>();
        health = GetComponent<HealthComponent>();
        health.SetMaxHealth(data.health);

        var drop = GetComponent<DropComponent>();
        if (drop != null)
            drop.SetDrop(data.minGearsDrop, data.maxGearsDrop);
    }

    protected virtual void OnEnable()
    {
        if (G.Tower != null) towerPosition = G.Tower.Position;
        sprite.flipX = (transform.position - towerPosition).x > 0; // flip sprite

        health.OnHealthChanged += PlayDamageReceivedAnimation;
        health.OnDeath += PlayDeathAnimation;
    }

    protected virtual void OnDisable()
    {
        health.OnHealthChanged -= PlayDamageReceivedAnimation;
        health.OnDeath -= PlayDeathAnimation;
    }

    protected virtual void Update()
    {
        if (G.Tower == null) return;
        if (!isStunned) Move();
    }

    protected abstract void Move();
    protected abstract void Attack(GameObject target);

    public void ApplyStun(float duration)
    {
        if (!isStunned)
            StartCoroutine(StunCoroutine(duration));
    }

    private IEnumerator StunCoroutine(float duration)
    {
        isStunned = true;
        currentSpeed = 0;

        // Визуальный эффект стана
        yield return new WaitForSeconds(duration);

        isStunned = false;
        currentSpeed = data.speed;
    }

    public void ApplySlow(float multiplier, float duration)
    {
        if (slowCoroutine != null) StopCoroutine(slowCoroutine);
        slowCoroutine = StartCoroutine(SlowCoroutine(multiplier, duration));
    }

    private IEnumerator SlowCoroutine(float multiplier, float duration)
    {
        slowMultiplier = multiplier;
        currentSpeed *= slowMultiplier;

        // Визуальный эффект замедления
        yield return new WaitForSeconds(duration);
        slowMultiplier = 1f;
        currentSpeed = data.speed;
    }

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
        currentSpeed = 0;
        //animator.Play("die");
        //AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(0.1f);
        G.EventManager?.TriggerEnemyKilled(this);
        Destroy(gameObject);
    }
}
