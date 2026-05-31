using System.Collections;
using UnityEngine;

public class FlyingCreature : CreatureController
{
    private bool isAttacking = false;
    private Coroutine attackCoroutine;
    private float bobOffset;
    Vector3 direction;

    private void Start()
    {
        bobOffset = Random.Range(0f, 2f * Mathf.PI);
        gameObject.tag = "FlyingEnemy";
    }

    protected override void Move()
    {
        // Прямо к башне с покачиванием
        direction = IsometricExtension.IsoDirection(transform.position, towerPosition);
        Vector3 movement = IsometricExtension.IsoMovement(direction, currentSpeed);
        transform.position += movement;
        float yBob = Mathf.Sin((Time.time + bobOffset) * 7f) * 0.001f;
        transform.position += new Vector3(0, yBob);


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
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            var projEffect = Instantiate(data.attackPrefabs[0], transform.position + direction * 0.3f, Quaternion.Euler(0, 0, angle), transform);
            G.AudioManager?.PlaySFX("shot");

            yield return new WaitForSeconds(attackCooldown);
            Destroy(projEffect, 0.6f);
        }

        isAttacking = false;
        attackCoroutine = null;
    }
}