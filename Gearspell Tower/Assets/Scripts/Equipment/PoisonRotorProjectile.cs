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

        [Header("Move Settings")]
        [SerializeField] private float rotorSpeed = 2f;
        [SerializeField] private float wobbleAmount = 0.5f; // Сила петляние
        [SerializeField] private float wobbleFrequency = 2f; // Частота петляние

        private Vector3 targetPosition;
        private bool isMoving = true;
        private float cloudDuration = 3f;
        private float cloudRadius = 1f;
        private int damage;
        private int tickCount = 3;
        private float tickInterval = 1f;

        private Coroutine moveCoroutine;
        private Coroutine damageCoroutine;

        private bool shootsSpikes;
        private int spikeCount;
        private GameObject spikePrefab;
        private Queue<GameObject> spikesPool;
        private Coroutine spikeCoroutine;

        private bool poisonExplodes;
        private float explosionRadius;

        public void Initialize(Vector3 spawnPoint, int damage, int tickCount, float tickInterval)
        {
            targetPosition = spawnPoint;
            this.damage = damage;
            this.tickCount = tickCount;
            this.tickInterval = tickInterval;

            // Настройка начального состояния
            SetUndergroundState(true);

            // Запуск движения
            if (moveCoroutine != null) StopCoroutine(moveCoroutine);
            moveCoroutine = StartCoroutine(MoveToTarget());
        }

        public void SetParameters(
            float cloudRadius, float cloudDuration,
            bool shootsSpikes, int spikeCount,
            GameObject spikePrefab, Queue<GameObject> spikesPool,
            bool poisonExplodes, float explosionRadius)
        {
            this.cloudRadius = cloudRadius;
            this.cloudDuration = Mathf.Max(cloudDuration, tickCount * tickInterval);
            this.shootsSpikes = shootsSpikes;
            this.spikeCount = spikeCount;
            this.spikePrefab = spikePrefab;
            this.spikesPool = spikesPool;
            this.poisonExplodes = poisonExplodes;
            this.explosionRadius = explosionRadius;
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
            float journeyLength = IsometricExtension.IsoDistance(startPos, targetPosition);
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

            damageCoroutine = StartCoroutine(MultiTickDamage());
            if (shootsSpikes)
                spikeCoroutine = StartCoroutine(ShootSpikesRoutine());
            StartCoroutine(DeactivateAfterDelay());
        }

        private IEnumerator MultiTickDamage()
        {
            for (int tick = 0; tick < tickCount; tick++)
            {
                Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, cloudRadius);
                foreach (var collider in enemies)
                {
                    if (collider.CompareTag("Enemy") &&
                        IsometricExtension.IsoDistance(transform.position, collider.transform.position) <= cloudRadius)
                    {
                        collider.GetComponent<HealthComponent>()?.TakeDamage(damage);
                    }
                }
                yield return new WaitForSeconds(tickInterval);
            }
        }

        private IEnumerator ShootSpikesRoutine()
        {
            yield return new WaitForSeconds(0.1f);
            ShootSpikes();
        }

        private void ShootSpikes()
        {
            float randomAngle = UnityEngine.Random.Range(0f, 360f);

            for (int i = 0; i < spikeCount; i++)
            {
                float angle = randomAngle + (360f / spikeCount) * i * Mathf.Deg2Rad;
                Vector3 dir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle));

                GameObject spike;
                if (spikesPool != null && spikesPool.Count > 0)
                {
                    spike = spikesPool.Dequeue();
                    spike.transform.position = transform.position;
                    spike.transform.rotation = Quaternion.LookRotation(Vector3.forward, dir);
                    spike.SetActive(true);
                }
                else spike = Instantiate(spikePrefab, transform.position, Quaternion.LookRotation(Vector3.forward, dir));

                PoisonSpike spikeScript = spike.GetComponent<PoisonSpike>();
                spikeScript.Direction = dir;
                spikeScript.Damage = damage;
                spikeScript.Range = cloudRadius * 3f;
                spikeScript.Pool = spikesPool;
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
