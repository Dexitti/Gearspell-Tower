using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Equipment
{
    public class PoisonRotorsController : EquipmentController
    {
        [SerializeField] private Animator rotorHangarsAnimator;

        private Queue<GameObject> rotorsPool = new Queue<GameObject>();
        private Queue<GameObject> rotorSpikesPool = new Queue<GameObject>();
        Vector3 firePoint = Vector3.zero;
        private float clusterRadius = 5f;

        private float rotorSpeedMultiplier = 1f;
        private float damageTickMultiplier = 1f;
        private float cloudRadiusMultiplier = 1f;
        private float cloudDurationMultiplier = 1f;
        private bool shootsSpikes = false;
        private int spikesCount = 3;
        private bool poisonExplodes = false;
        private float explosionRadius = 2f;

        protected override void OnEnable()
        {
            base.OnEnable();
            firePoint = towerTransform.position + new Vector3(0, -0.75f, 0);
            rotorHangarsAnimator = decorationInstance.GetComponentInChildren<Animator>();

            RebuildPools();
                //if (isUpgradeSpikes)
                //{
                //    for (int s = 0; s < spikesCount; s++)
                //    {
                //        GameObject spike = Instantiate(data.projectilesPrefabs[1], proj.transform);
                //        spike.SetActive(false);
                //        rotorSpikesPool.Enqueue(spike);
                //    }
                //}
        }

        private void RebuildPools()
        {
            foreach (var rotor in rotorsPool)
                if (rotor != null) Destroy(rotor);
            foreach (var spike in rotorSpikesPool)
                if (spike != null) Destroy(spike);
            rotorsPool.Clear();
            rotorSpikesPool.Clear();

            int poolSize = Mathf.Max(currentProjectileCount, 4);

            for (int i = 0; i < poolSize; i++)
            {
                GameObject proj = Instantiate(data.projectilesPrefabs[0], firePoint, Quaternion.identity, transform);
                proj.SetActive(false);
                rotorsPool.Enqueue(proj);
            }

            if (shootsSpikes)
            {
                for (int s = 0; s < poolSize * spikesCount; s++)
                {
                    GameObject spike = Instantiate(data.projectilesPrefabs[1], transform);
                    spike.SetActive(false);
                    rotorSpikesPool.Enqueue(spike);
                }
            }
        }

        protected override IEnumerator Attack()
        {
            List<Transform> targets = GetTargets();
            if (targets == null || targets.Count == 0) yield break;

            rotorHangarsAnimator.SetTrigger("Attack");

            for (int i = 0; i < Mathf.Min(currentProjectileCount, targets.Count); i++)
            {
                Transform target = targets[i];
                if (IsometricExtension.IsoDistance(towerTransform.position, target.position) > currentRange) continue;

                Vector3 spawnPoint = CalculateSpawnPoint(target);
                SpawnRotor(spawnPoint);
            }
        }

        private void SpawnRotor(Vector3 spawnPoint)
        {
            GameObject rotor;
            if (rotorsPool.Count > 0)
            {
                rotor = rotorsPool.Dequeue();
                rotor.SetActive(true);
            }
            else rotor = Instantiate(data.projectilesPrefabs[0], firePoint, Quaternion.identity, transform);

            PoisonRotorProjectile projScript = rotor.GetComponent<PoisonRotorProjectile>();
            projScript.Initialize(spawnPoint, currentDamage, 3, 1f / damageTickMultiplier);
            projScript.SetParameters(cloudRadiusMultiplier, cloudDurationMultiplier, shootsSpikes, spikesCount, data.projectilesPrefabs[1], rotorSpikesPool, poisonExplodes, explosionRadius);

            StartCoroutine(ReturnToPoolAfterDestroy(rotor, projScript));

            //yield return new WaitForSeconds(0.05f);
        }

        private IEnumerator ReturnToPoolAfterDestroy(GameObject rotor, PoisonRotorProjectile projScript)
        {
            // Ждем пока ротор не будет уничтожен (проверяем каждые 0.5 секунды)
            while (rotor != null && rotor.activeSelf)
            {
                yield return new WaitForSeconds(0.5f);
            }

            if (rotor != null)
            {
                rotor.SetActive(false);
                rotorsPool.Enqueue(rotor);
            }
        }

        private List<Transform> GetTargets()
        {
            GameObject[] enemyArray = GameObject.FindGameObjectsWithTag("Enemy");
            if (enemyArray.Length == 0) return null;

            List<Transform> enemies = enemyArray.Select(e => e.transform).ToList();
            // Если врагов <= выбираем их
            if (enemies.Count <= currentProjectileCount) return enemies;

            List<List<Transform>> clusters = IdentifyEnemyClusters(enemies);
            var sortedClusters = clusters.OrderByDescending(c => c.Count);

            List<Transform> selectedTargets = new List<Transform>();
            foreach (var cluster in sortedClusters)
            {
                if (selectedTargets.Count >= currentProjectileCount) break;
                // Выбираем центр скопления
                selectedTargets.Add(GetClusterCenter(cluster));
            }

            if (selectedTargets.Count < currentProjectileCount)
            {
                var remaining = enemies.Where(e => !selectedTargets.Contains(e)).ToList();
                var additional = remaining.Take(currentProjectileCount - selectedTargets.Count);
                selectedTargets.AddRange(additional);
            }

            return selectedTargets;
        }

        private List<List<Transform>> IdentifyEnemyClusters(List<Transform> enemies)
        {
            List<List<Transform>> clusters = new List<List<Transform>>();
            HashSet<Transform> processed = new HashSet<Transform>();

            foreach (var enemy in enemies)
            {
                if (processed.Contains(enemy)) continue;

                List<Transform> cluster = new List<Transform>();
                Queue<Transform> toProcess = new Queue<Transform>();
                toProcess.Enqueue(enemy);

                while (toProcess.Count > 0)
                {
                    var current = toProcess.Dequeue();
                    if (processed.Contains(current)) continue;

                    processed.Add(current);
                    cluster.Add(current);

                    // Находим врагов поблизости
                    foreach (var other in enemies)
                    {
                        if (!processed.Contains(other) &&
                            IsometricExtension.IsoDistance(current.position, other.position) <= clusterRadius)
                        {
                            toProcess.Enqueue(other);
                        }
                    }
                }

                if (cluster.Count > 0)
                    clusters.Add(cluster);
            }

            return clusters;
        }

        private Transform GetClusterCenter(List<Transform> cluster)
        {
            Vector3 center = Vector3.zero;
            foreach (var enemy in cluster)
            {
                center += enemy.position;
            }
            center /= cluster.Count;

            return cluster.OrderBy(enemy => IsometricExtension.IsoDistance(enemy.position, center)).FirstOrDefault();
        }

        private Vector3 CalculateSpawnPoint(Transform target)
        {
            Vector3 direction = (target.position - towerTransform.position).normalized;
            float distance = IsometricExtension.IsoDistance(towerTransform.position, target.position);

            return towerTransform.position + direction * (distance * 0.85f); // Опережение 15%
        }

        protected override void ApplyEffect(string upgradeId)
        {
            switch (upgradeId)
            {
                case "PoisonRotors_1":
                    rotorSpeedMultiplier = 1.5f;
                    damageTickMultiplier = 1.6f;
                    break;

                case "PoisonRotors_2":
                    currentDamage = Mathf.RoundToInt(currentDamage * 1.4f);
                    break;

                case "PoisonRotors_3": // fork A
                    cloudRadiusMultiplier = 1.8f;
                    cloudDurationMultiplier = 1.7f;
                    break;

                case "PoisonRotors_4": // fork B
                    shootsSpikes = true;
                    spikesCount = 4;
                    break;

                case "PoisonRotors_5":
                    HasActiveAbility = true;
                    poisonExplodes = true;
                    explosionRadius = 2.5f;
                    break;

                default:
                    Debug.LogWarning($"[PoisonRotors] Unknown upgradeId: {upgradeId}");
                    break;
            }

            RebuildPools();
        }

        protected override void ActivateAbility()
        {
            if (!HasActiveAbility) return;

            // Чума активируется пассивно через poisonExplodes флаг в роторах
            // Но можно добавить активную часть: мгновенный взрыв всех отравленных врагов
            StartCoroutine(PlagueActive());
        }

        private IEnumerator PlagueActive()
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var enemy in enemies)
            {
                // Взрываем всех врагов в радиусе (условно — всех отравленных)
                if (IsometricExtension.IsoDistance(towerTransform.position, enemy.transform.position) <= currentRange)
                {
                    // Наносим урон вокруг
                    Collider2D[] nearby = Physics2D.OverlapCircleAll(enemy.transform.position, explosionRadius);
                    foreach (var hit in nearby)
                    {
                        if (hit.CompareTag("Enemy") && hit.gameObject != enemy)
                        {
                            HealthComponent hp = hit.GetComponent<HealthComponent>();
                            if (hp != null)
                                hp.TakeDamage(Mathf.RoundToInt(currentDamage * 2f));
                        }
                    }

                    // Визуальный эффект взрыва
                    HealthComponent enemyHp = enemy.GetComponent<HealthComponent>();
                    if (enemyHp != null)
                        enemyHp.TakeDamage(Mathf.RoundToInt(currentDamage * 1.5f));
                }
            }

            yield return new WaitForSeconds(0.1f);
        }
    }
}
