using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Equipment
{
    public class LightningCogsController : EquipmentController
    {
        [Header("Cogs Settings")]
        [SerializeField] private int cogsAmount = 4;
        [SerializeField] private int lightningTickTimes = 1; // Увеличится с прокачкой
        [SerializeField] private float cogsSpread = 0.2f;
        [SerializeField] private float cogArcHeight = 1.3f;
        [SerializeField] private float attackDuration = 1f;
        [SerializeField] private float lightningDamageWidth = 1.2f;

        [Header("Visual Settings")]
        [SerializeField] private Color lightningColor = new Color(0.66f, 0.44f, 0.85f, 1f);
        [SerializeField] private float lightningWidth = 0.15f;

        private Vector3 firePoint;
        private float cogSpacing;
        private Queue<GameObject> cogsPool = new Queue<GameObject>();

        private class CogChain
        {
            public List<GameObject> cogs = new List<GameObject>();
            public List<Vector3> positions = new List<Vector3>();
            public LineRenderer lightningLine;
            public Vector3 direction;
        }

        private List<CogChain> activeChains = new List<CogChain>();
        private Coroutine lightningCoroutine;

        protected override void OnEnable()
        {
            base.OnEnable();
            firePoint = towerTransform.position + new Vector3(0.526f, 0.211f, 0);
            cogSpacing = currentRange / cogsAmount;

            for (int i = 0; i < currentProjectileCount * cogsAmount * 2; i++)
            {
                GameObject cog = Instantiate(data.projectilesPrefabs[0], transform);
                cog.SetActive(false);
                cogsPool.Enqueue(cog);
            }

        }

        protected override IEnumerator Attack()
        {
            List<Vector3> directions = GetAttackDirections();
            if (directions == null) yield break;

            FireCogs(directions);
            yield return new WaitForSeconds(0.5f); // Lightning delay

            if (lightningCoroutine != null) StopCoroutine(lightningCoroutine);
            lightningCoroutine = StartCoroutine(ActivateLightning());
        }

        private List<Vector3> GetAttackDirections()
        {
            GameObject[] enemyArray = GameObject.FindGameObjectsWithTag("Enemy");
            if (enemyArray.Length == 0) return null;

            List<Transform> enemiesInRange = enemyArray
                .Where(e => Vector3.Distance(towerTransform.position, e.transform.position) <= currentRange)
                .Select(e => e.transform)
                .ToList();
            if (enemiesInRange.Count == 0) return null;

            // Группируем врагов по направлениям
            Dictionary<float, List<Transform>> directionGroups = new Dictionary<float, List<Transform>>();
            float angleStep = 30f;

            foreach (var enemy in enemiesInRange)
            {
                Vector3 directionToEnemy = (enemy.position - towerTransform.position).normalized;
                float angle = Mathf.Atan2(directionToEnemy.y, directionToEnemy.x) * Mathf.Rad2Deg;
                if (angle < 0) angle += 360f;

                float groupAngle = Mathf.Floor(angle / angleStep) * angleStep;

                if (!directionGroups.ContainsKey(groupAngle))
                    directionGroups[groupAngle] = new List<Transform>();

                directionGroups[groupAngle].Add(enemy);
            }

            // Среднее направление из группы
            var sortedGroups = directionGroups.OrderByDescending(g => g.Value.Count).Take(currentProjectileCount);
            List<Vector3> result = new List<Vector3>();
            foreach (var group in sortedGroups)
            {
                Vector3 avgDirection = Vector3.zero;
                foreach (var enemy in group.Value)
                {
                    avgDirection += (enemy.position - towerTransform.position).normalized;
                }
                avgDirection.Normalize();
                result.Add(avgDirection);
            }

            return result;
        }

        private void FireCogs(List<Vector3> directions)
        {
            activeChains.Clear();

            foreach (var direction in directions)
            {
                CogChain chain = new CogChain();
                chain.direction = direction;

                for (int i = 1; i <= cogsAmount; i++)
                {
                    // Нормализованное расстояние (%)
                    float t = (float)i / cogsAmount;
                    float distanceFromTower = Mathf.Lerp(currentRange * 0.2f, currentRange, t);

                    Vector3 targetPos = firePoint + direction * distanceFromTower;

                    // Разброс
                    float randomAngle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
                    Vector3 randomOffset = new Vector3(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle), 0) * cogsSpread;
                    targetPos += randomOffset;

                    chain.positions.Add(targetPos);

                    GameObject cog;
                    if (cogsPool.Count > 0)
                    {
                        cog = cogsPool.Dequeue();
                        cog.transform.position = firePoint;
                        cog.SetActive(true);
                    }
                    else cog = Instantiate(data.projectilesPrefabs[0], firePoint, Quaternion.identity, transform);
                    chain.cogs.Add(cog);

                    float flightAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    StartCoroutine(AnimateCogFlight(cog, firePoint, targetPos, flightAngle, (float)i / cogsAmount));
                }

                activeChains.Add(chain);
            }
        }

        private IEnumerator AnimateCogFlight(GameObject cog, Vector3 start, Vector3 end, float flightAngle, float normalizedT)
        {
            float elapsed = 0f;
            SpriteRenderer renderer = cog.GetComponent<SpriteRenderer>();
            float duration = 0.4f;

            if (renderer != null)
            {
                Color color = renderer.color;
                renderer.color = new Color(color.r, color.g, color.b, 0f);
            }

            Vector3 flatEnd = end;
            flatEnd.y = start.y; // Убираем влияние дуги из конечной позиции!

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Прямолинейное движение
                Vector3 currentPos = Vector3.Lerp(start, flatEnd, t);

                // Дуга: максимальная высота в середине пути, затухает к концу
                // Чем дальше шестерня, тем выше дуга (нормализовано)
                float arcMultiplier = Mathf.Lerp(0.5f, 1f, normalizedT);
                float arc = Mathf.Sin(t * Mathf.PI) * cogArcHeight * arcMultiplier * (1f - t * 0.5f);
                currentPos.y += arc;

                cog.transform.position = currentPos;

                if (renderer != null)
                {
                    float alpha = Mathf.Lerp(0f, 1f, t);
                    Color color = renderer.color;
                    renderer.color = new Color(color.r, color.g, color.b, alpha);
                }

                yield return null;
            }

            cog.transform.position = end;
            if (renderer != null)
            {
                Color color = renderer.color;
                renderer.color = new Color(color.r, color.g, color.b, 1f);
            }
        }

        private IEnumerator ActivateLightning()
        {
            yield return new WaitForSeconds(0.2f);
            if (activeChains.Count == 0) yield break;

            // Отдельные цепи
            foreach (var chain in activeChains)
            {
                if (chain.positions.Count >= 1)
                {
                    CreateLightningForChain(chain);
                }
            }

            for (int tick = 0; tick < lightningTickTimes; tick++)
            {
                foreach (var chain in activeChains)
                {
                    if (chain.lightningLine != null)
                    {
                        DrawLightningForChain(chain);
                    }
                }

                DealDamageAlongLightning();
                yield return new WaitForSeconds(attackDuration);
            }

            // Убираем все молнии
            foreach (var chain in activeChains)
            {
                if (chain.lightningLine != null)
                {
                    chain.lightningLine.enabled = false;
                }
            }

            yield return StartCoroutine(ReturnCogsToPool());
        }

        private void CreateLightningForChain(CogChain chain)
        {
            if (chain.lightningLine != null) return;

            GameObject lineObj = new GameObject($"LightningLine_{activeChains.IndexOf(chain)}");
            lineObj.transform.SetParent(transform);
            lineObj.transform.localPosition = Vector3.zero;

            chain.lightningLine = lineObj.AddComponent<LineRenderer>();
            chain.lightningLine.startWidth = lightningWidth;
            chain.lightningLine.endWidth = lightningWidth;
            chain.lightningLine.sortingLayerName = "Effects";
            chain.lightningLine.sortingOrder = 1;
            chain.lightningLine.useWorldSpace = true;

            chain.lightningLine.startColor = lightningColor;
            chain.lightningLine.endColor = lightningColor;
            chain.lightningLine.enabled = true;
        }

        private void DrawLightningForChain(CogChain chain)
        {
            if (chain.lightningLine == null || chain.positions.Count == 0) return;

            // Башня + все шестерни в цепи
            List<Vector3> points = new List<Vector3>();
            points.Add(firePoint);
            points.AddRange(chain.positions);

            chain.lightningLine.positionCount = points.Count;

            for (int i = 0; i < points.Count; i++)
            {
                chain.lightningLine.SetPosition(i, points[i]);
            }
        }

        private void DealDamageAlongLightning()
        {
            Collider2D[] allEnemies = Physics2D.OverlapCircleAll(towerTransform.position, currentRange);
            HashSet<HealthComponent> damagedEnemies = new HashSet<HealthComponent>(); // Чтобы не дамажить одного врага несколько раз за тик

            foreach (var enemyCollider in allEnemies)
            {
                if (!enemyCollider.CompareTag("Enemy")) continue;

                Vector3 enemyPos = enemyCollider.transform.position;
                bool damaged = false;

                foreach (var chain in activeChains)
                {
                    if (damaged) break;

                    // Проверяем урон вокруг каждой шестерни
                    foreach (var cogPos in chain.positions)
                    {
                        if (Vector3.Distance(enemyPos, cogPos) <= 1.5f)
                        {
                            HealthComponent health = enemyCollider.GetComponent<HealthComponent>();
                            if (health != null && !damagedEnemies.Contains(health))
                            {
                                health.TakeDamage(currentDamage);
                                damagedEnemies.Add(health);
                                damaged = true;
                            }
                            break;
                        }
                    }

                    if (damaged) break;

                    // Проверяем урон вдоль линии молнии (между башней и шестернями)
                    List<Vector3> allPoints = new List<Vector3>();
                    allPoints.Add(firePoint);
                    allPoints.AddRange(chain.positions);

                    for (int i = 0; i < allPoints.Count - 1; i++)
                    {
                        Vector3 start = allPoints[i];
                        Vector3 end = allPoints[i + 1];

                        // Проверяем расстояние от врага до отрезка
                        float distance = PointToSegmentDistance(enemyPos, start, end);

                        if (distance <= lightningDamageWidth)
                        {
                            HealthComponent health = enemyCollider.GetComponent<HealthComponent>();
                            if (health != null && !damagedEnemies.Contains(health))
                            {
                                health.TakeDamage(currentDamage);
                                damagedEnemies.Add(health);
                                damaged = true;
                            }
                            break;
                        }
                    }
                }
            }
        }

        private float PointToSegmentDistance(Vector3 point, Vector3 a, Vector3 b)
        {
            Vector2 p = new Vector2(point.x, point.y);
            Vector2 a2 = new Vector2(a.x, a.y);
            Vector2 b2 = new Vector2(b.x, b.y);

            Vector2 ab = b2 - a2;
            Vector2 ap = p - a2;

            float t = Vector2.Dot(ap, ab) / Vector2.Dot(ab, ab);
            t = Mathf.Clamp01(t);

            Vector2 closestPoint = a2 + t * ab;
            return Vector2.Distance(p, closestPoint);
        }

        private IEnumerator ReturnCogsToPool()
        {
            foreach (var chain in activeChains)
            {
                foreach (var cog in chain.cogs)
                {
                    if (cog != null)
                    {
                        StartCoroutine(AnimateCogDisappearance(cog));
                    }
                }
            }

            yield return new WaitForSeconds(0.3f);

            foreach (var chain in activeChains)
            {
                foreach (var cog in chain.cogs)
                {
                    if (cog != null)
                    {
                        cog.SetActive(false);
                        cogsPool.Enqueue(cog);
                    }
                }

                if (chain.lightningLine != null)
                {
                    Destroy(chain.lightningLine.gameObject);
                }
            }

            activeChains.Clear();
        }

        private IEnumerator AnimateCogDisappearance(GameObject cog)
        {
            SpriteRenderer renderer = cog.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                float duration = 0.2f;
                Color originalColor = renderer.color;

                for (float t = 0; t < duration; t += Time.deltaTime)
                {
                    float alpha = 1f - (t / duration);
                    renderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                    yield return null;
                }
            }
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
