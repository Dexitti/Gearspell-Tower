using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Equipment
{
    public class PoisonRotorProjectile : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private CircleCollider2D attackCollider;
        [SerializeField] private ParticleSystem groundTrail;
        [SerializeField] private ParticleSystem poisonCloud;
        [SerializeField] private SpriteMask spriteMask;

        [Header("Poison Rotors Settings")]
        [SerializeField] private float rotorSpeed = 2f;
        [SerializeField] private float wobbleAmount = 0.5f; // Сила петляние
        [SerializeField] private float wobbleFrequency = 2f; // Частота петляние

        [Header("Cloud Settings")]
        [SerializeField] private float damageTickInterval = 1f;
        [SerializeField] private float cloudDuration = 3f;
        [SerializeField] private float cloudRadius = 1f;

        private Vector3 targetPosition;
        private bool isMoving = true;
        private int damage;
        private Coroutine damageCoroutine;
        private Coroutine moveCoroutine;

        public void Initialize(Vector3 spawnPoint, int damage)
        {
            targetPosition = spawnPoint;
            this.damage = damage;

            // Настройка начального состояния
            SetUndergroundState(true);

            // Запуск движения
            if (moveCoroutine != null) StopCoroutine(moveCoroutine);
            moveCoroutine = StartCoroutine(MoveToTarget());
        }

        private void SetUndergroundState(bool isUnderground)
        {
            if (spriteRenderer != null) spriteRenderer.enabled = !isUnderground;
            if (spriteMask != null) spriteMask.enabled = isUnderground;
            if (groundTrail != null)
            {
                if (isUnderground) groundTrail.Play();
                else groundTrail.Stop();
            }
            if (attackCollider != null) attackCollider.enabled = false;
            if (poisonCloud != null)
            {
                if (isUnderground) poisonCloud.Stop();
                else poisonCloud.Play();
            }
        }

        private IEnumerator MoveToTarget()
        {
            Vector3 startPos = transform.position;
            float journeyLength = Vector3.Distance(startPos, targetPosition);
            float startTime = Time.time;

            while (isMoving && IsometricExtension.IsoDistance(transform.position, targetPosition) > 0.2f)
            {
                float fraction = (Time.time - startTime) * rotorSpeed / journeyLength;
                Vector3 basePos = Vector3.Lerp(startPos, targetPosition, fraction);

                // Петляние
                float wobbleX = Mathf.Sin(Time.time * wobbleFrequency) * wobbleAmount;
                float wobbleY = Mathf.Cos(Time.time * wobbleFrequency * 0.7f) * wobbleAmount * 0.5f;

                transform.position = basePos + new Vector3(wobbleX, wobbleY, 0);
                yield return null;
            }

            transform.position = targetPosition;
            isMoving = false;

            yield return StartCoroutine(EmergeAndDeploy());
        }

        private IEnumerator EmergeAndDeploy()
        {
            // Выключаем след
            if (groundTrail != null) groundTrail.Stop();

            // Анимация появления
            float duration = 0.3f;
            Vector3 startPos = transform.position;
            Vector3 endPos = startPos + Vector3.up * 0.2f;

            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                float progress = t / duration;
                transform.position = Vector3.Lerp(startPos, endPos, progress);

                if (spriteRenderer != null)
                {
                    Color c = spriteRenderer.color;
                    c.a = progress;
                    spriteRenderer.color = c;
                    spriteRenderer.enabled = true;
                }
                yield return null;
            }

            transform.position = endPos;

            if (spriteRenderer != null)
            {
                Color c = spriteRenderer.color;
                c.a = 1f;
                spriteRenderer.color = c;
            }

            ActivatePoisonCloud();
        }

        private void ActivatePoisonCloud()
        {
            if (poisonCloud != null)
            {
                var shape = poisonCloud.shape;
                shape.radius = cloudRadius;
                poisonCloud.Play();
            }

            if (attackCollider != null)
            {
                attackCollider.radius = cloudRadius;
                attackCollider.enabled = true;
            }

            damageCoroutine = StartCoroutine(DealDamageOverTime());
            StartCoroutine(DeactivateAfterDelay());
        }

        private IEnumerator DealDamageOverTime()
        {
            float timer = 0f;

            while (timer < cloudDuration)
            {
                timer += Time.deltaTime;

                if (timer >= damageTickInterval)
                {
                    timer = 0f;
                    Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, cloudRadius);

                    foreach (var enemy in enemies)
                    {
                        if (enemy.CompareTag("Enemy") && IsometricExtension.IsoDistance(transform.position, enemy.transform.position) <= cloudRadius)
                        {
                            var health = enemy.GetComponent<HealthComponent>();
                            health.TakeDamage(damage);
                        }
                    }
                }
                yield return null;
            }
        }

        private IEnumerator DeactivateAfterDelay()
        {
            yield return new WaitForSeconds(cloudDuration);

            if (damageCoroutine != null)
                StopCoroutine(damageCoroutine);

            if (poisonCloud != null)
            {
                poisonCloud.Stop();
                poisonCloud.Clear();
            }

            yield return new WaitForSeconds(0.3f);

            // Уход под землю
            float duration = 0.3f;
            Vector3 startPos = transform.position;
            Vector3 endPos = startPos - Vector3.up * 0.5f;

            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                float progress = t / duration;
                transform.position = Vector3.Lerp(startPos, endPos, progress);

                if (spriteRenderer != null)
                {
                    Color c = spriteRenderer.color;
                    c.a = 1f - progress;
                    spriteRenderer.color = c;
                }
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
