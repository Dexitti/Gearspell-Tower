using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class WalkingCreature : CreatureController
{
    private bool isAttacking = false;
    private Coroutine attackCoroutine;

    protected override void Move()
    {
        if (isAttacking) return;
        // Прямо к башне
        Vector3 dirToTower = IsometricExtension.IsoDirection(transform.position, towerPosition);
        Vector3 movement = IsometricExtension.IsoMovement(dirToTower, currentSpeed);
        transform.position += movement;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Tower"))
        {
            Attack(other.gameObject);
            G.EventManager?.TriggerEnemyReachedTower(this);
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
            G.AudioManager?.PlaySFX("hit", 0.5f);

            yield return new WaitForSeconds(attackCooldown);
        }

        isAttacking = false;
        attackCoroutine = null;
    }
}
