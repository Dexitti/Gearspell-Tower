using System.Collections;
using UnityEngine;

public class FlyingCreature : CreatureController
{
    private bool isAttacking = false;
    private Coroutine attackCoroutine;
    private float bobOffset;

    private void Start()
    {
        bobOffset = Random.Range(0f, Mathf.PI * 2f);
        gameObject.tag = "FlyingEnemy";
    }

    protected override void Move()
    {
        // Прямо к башне с покачиванием
        Vector3 direction = (towerPosition - transform.position).normalized;
        Vector3 movement = direction * currentSpeed * slowMultiplier * Time.deltaTime;
        float yBob = Mathf.Sin((Time.time + bobOffset) * 2f) * 0.1f;
        movement.y = yBob;

        transform.position += movement;

        // Летающие враги могут проходить сквозь препятствия
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Tower"))
        {
            Attack(other.gameObject);
        }
    }

    protected override void Attack(GameObject target)
    {
        if (attackCoroutine != null) StopCoroutine(attackCoroutine);
        attackCoroutine = StartCoroutine(AttackCoroutine(target));
    }

    private IEnumerator AttackCoroutine(GameObject target)
    {
        isAttacking = true;

        var towerHealth = target.GetComponent<HealthComponent>();

        while (towerHealth != null && towerHealth.isAlive)
        {
            towerHealth.TakeDamage(currentDamage);
            G.AudioManager?.PlaySFX("shot");

            yield return new WaitForSeconds(attackCooldown);
        }

        isAttacking = false;
        attackCoroutine = null;
    }
}