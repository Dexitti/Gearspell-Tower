using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
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

        protected override void OnEnable()
        {
            base.OnEnable();
            firePoint = towerTransform.position + new Vector3(0, -0.75f, 0);
            rotorHangarsAnimator = decorationInstance.GetComponentInChildren<Animator>();

            for (int i = 0; i < currentProjectileCount; i++)
            {
                GameObject proj = Instantiate(data.projectilesPrefabs[0], firePoint, Quaternion.identity, transform);
                proj.SetActive(false);
                rotorsPool.Enqueue(proj);

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
        }

        protected override IEnumerator Attack()
        {
            List<Transform> targets = GetTargets();
            if (targets == null || targets.Count == 0) yield break;

            rotorHangarsAnimator.SetTrigger("Attack");

            for (int i = 0; i < Mathf.Min(currentProjectileCount, targets.Count); i++)
            {
                Transform target = targets[i];
                if (Vector3.Distance(towerTransform.position, target.position) > currentRange) continue;

                Vector3 spawnPoint = CalculateSpawnPoint(target);

                GameObject rotor;
                if (rotorsPool.Count > 0)
                {
                    rotor = rotorsPool.Dequeue();
                    rotor.SetActive(true);
                }
                else rotor = Instantiate(data.projectilesPrefabs[0], firePoint, Quaternion.identity, transform);

                PoisonRotorProjectile projScript = rotor.GetComponent<PoisonRotorProjectile>();
                projScript.Initialize(spawnPoint, currentDamage);
                StartCoroutine(ReturnToPoolAfterDestroy(rotor, projScript));

                //yield return new WaitForSeconds(0.05f);
            }
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
                            Vector3.Distance(current.position, other.position) <= clusterRadius)
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

            return cluster.OrderBy(enemy => Vector3.Distance(enemy.position, center)).FirstOrDefault();
        }

        private Vector3 CalculateSpawnPoint(Transform target)
        {
            Vector3 direction = (target.position - towerTransform.position).normalized;
            float distance = Vector3.Distance(towerTransform.position, target.position);

            return towerTransform.position + direction * (distance * 0.85f); // Опережение 15%
        }

        protected override void ApplyEffect(string upgradeId)
        {
            throw new NotImplementedException();
        }

        protected override void Upgrade(int upgradeIndex)
        {
            throw new NotImplementedException();
        }

        protected override void ActivateAbility()
        {
            throw new NotImplementedException();
        }
    }
}
