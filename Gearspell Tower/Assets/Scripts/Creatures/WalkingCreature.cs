using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class WalkingCreature : Creature
{
    protected override void Move()
    {
        // Прямо к башне
        Vector3 isoDirection = IsometricExtension.IsoDirection(transform.position, towerPosition);
        Vector3 movement = IsometricExtension.IsoMovement(isoDirection, baseSpeed);
        transform.position += movement;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Tower"))
        {
            baseSpeed = 0;
            OnReachedTower?.Invoke(this);
            G.EventManager?.TriggerEnemyReachedTower(this);
            StartCoroutine(Attack(other));
        }
    }

    IEnumerator Attack(Collider2D other)
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
}
