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
        //Move
        transform.position = Vector3.MoveTowards(transform.position, towerPosition, baseSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Tower"))
        {
            baseSpeed = 0;
            OnReachedTower?.Invoke(this);
            G.EventManager?.TriggerEnemyReachedTower(this);
            StartCoroutine(AttackTower(other));
        }
    }

    IEnumerator AttackTower(Collider2D other)
    {
        while (true)
        {
            HealthComponent towerHealthComponent = other.GetComponent<HealthComponent>();
            if (towerHealthComponent == null || !towerHealthComponent.isAlive) yield break;
            towerHealthComponent.TakeDamage(baseDamage);

            // Анимация атаки

            yield return new WaitForSeconds(1f);
        }
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
        baseSpeed = 0;
        //animator.Play("die");
        //AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(0.1f);
        G.EventManager?.TriggerEnemyKilled(this);
        Destroy(gameObject);
    }
}
