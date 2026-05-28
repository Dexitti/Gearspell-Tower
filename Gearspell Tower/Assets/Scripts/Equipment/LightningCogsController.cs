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

        private int extraBranches = 0;
        private bool hasLightningPillar = false;
        private List<GameObject> activePillars = new List<GameObject>();

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
                .Where(e => IsometricExtension.IsoDistance(towerTransform.position, e.transform.position) <= currentRange)
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

                Vector3 xOffset = direction.x < 0 ? new Vector3(1, 0) : Vector3.zero; // Из-за расположения декорации на башне

                for (int i = 1; i <= cogsAmount; i++)
                {
                    // Нормализованное расстояние (%)
                    float t = (float)i / cogsAmount;
                    float distanceFromTower = Mathf.Lerp(currentRange * 0.2f, currentRange, t);

                    Vector3 targetPos = firePoint + direction * distanceFromTower + xOffset;

                    // Разброс
                    float randomAngle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
                    Vector3 randomOffset = IsometricExtension.IsoVector(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle), 0) * cogsSpread;
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
                        DrawBranchesForChain(chain);
                    }
                }

                if (tick == 0 && hasLightningPillar)
                {
                    foreach (var chain in activeChains)
                        CreatePillarsForChain(chain);
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

        private void DrawBranchesForChain(CogChain chain)
        {
            if (extraBranches <= 0 || chain.positions.Count == 0) return;

            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

            var nearbyEnemies = enemies
                .Where(e => IsometricExtension.IsoDistance(towerTransform.position, e.transform.position) <= currentRange)
                .OrderBy(e => IsometricExtension.IsoDistance(towerTransform.position, e.transform.position))
                .Take(extraBranches * activeChains.Count) // всего ответвлений = цепей × extraBranches
                .ToList();

            HashSet<GameObject> usedEnemies = new HashSet<GameObject>();

            foreach (var enemy in nearbyEnemies)
            {
                if (usedEnemies.Contains(enemy)) continue;

                // Находим ближайшую шестерню к этому врагу
                Vector3 enemyPos = enemy.transform.position;
                Vector3 closestCog = chain.positions[0];
                float closestDist = IsometricExtension.IsoDistance(enemyPos, closestCog);

                foreach (var cogPos in chain.positions)
                {
                    float dist = IsometricExtension.IsoDistance(enemyPos, cogPos);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestCog = cogPos;
                    }
                }

                // Только если враг достаточно близко к шестерне
                if (closestDist > cogSpacing) continue;

                // Создаём ответвление
                GameObject branchObj = new GameObject($"Branch_{chain.positions.IndexOf(closestCog)}_{usedEnemies.Count}");
                branchObj.transform.SetParent(transform);
                LineRenderer branchLine = branchObj.AddComponent<LineRenderer>();
                branchLine.startWidth = lightningWidth * 0.6f;
                branchLine.endWidth = lightningWidth * 0.2f;
                branchLine.positionCount = 2;
                branchLine.SetPosition(0, closestCog);
                branchLine.SetPosition(1, enemyPos);
                branchLine.startColor = lightningColor;
                branchLine.endColor = new Color(lightningColor.r, lightningColor.g, lightningColor.b, 0.3f);
                branchLine.sortingLayerName = "Effects";
                branchLine.sortingOrder = 1;
                branchLine.useWorldSpace = true;

                HealthComponent hp = enemy.GetComponent<HealthComponent>();
                if (hp != null)
                    hp.TakeDamage(Mathf.RoundToInt(currentDamage * 0.75f));

                usedEnemies.Add(enemy);
                Destroy(branchObj, attackDuration);

                if (usedEnemies.Count >= extraBranches) break;
            }
        }

        private void CreatePillarsForChain(CogChain chain)
        {
            if (!hasLightningPillar) return;

            foreach (var cogPos in chain.positions)
            {
                GameObject pillar = null;

                if (data.projectilesPrefabs[2] != null)
                {
                    pillar = Instantiate(data.projectilesPrefabs[2], cogPos, Quaternion.identity, transform);
                }
                // Fallback: LineRenderer
                else
                {
                    pillar = new GameObject($"Pillar_{chain.positions.IndexOf(cogPos)}");
                    pillar.transform.SetParent(transform);
                    pillar.transform.position = cogPos;
                    LineRenderer lr = pillar.AddComponent<LineRenderer>();
                    lr.startWidth = lightningWidth * 1.5f;
                    lr.endWidth = lightningWidth * 0.8f;
                    lr.positionCount = 2;
                    lr.SetPosition(0, cogPos);
                    lr.SetPosition(1, cogPos + Vector3.up * 3f);
                    lr.startColor = lightningColor;
                    lr.endColor = new Color(lightningColor.r, lightningColor.g, lightningColor.b, 0.5f);
                    lr.sortingLayerName = "Effects";
                    lr.sortingOrder = 2;
                    lr.useWorldSpace = true;
                }

                if (pillar != null)
                    activePillars.Add(pillar);
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
                        if (IsometricExtension.IsoDistance(enemyPos, cogPos) <= 1.5f)
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

            if (hasLightningPillar)
            {
                foreach (var chain in activeChains)
                {
                    foreach (var cogPos in chain.positions)
                    {
                        // Урон по вертикали над каждой шестернёй
                        Collider2D[] pillarTargets = Physics2D.OverlapCircleAll(cogPos, lightningDamageWidth * 1.5f);
                        foreach (var col in pillarTargets)
                        {
                            if (!(col.CompareTag("Enemy") || col.CompareTag("FlyingEnemy"))) continue;
                            HealthComponent health = col.GetComponent<HealthComponent>();
                            if (health != null && !damagedEnemies.Contains(health))
                            {
                                health.TakeDamage(Mathf.RoundToInt(currentDamage * 0.5f));
                                damagedEnemies.Add(health);
                            }
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
            return Vector3.Distance(p, closestPoint);
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

            foreach (var pillar in activePillars)
                if (pillar != null) Destroy(pillar);
            activePillars.Clear();

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

        protected override void ApplyEffect(string upgradeId)
        {
            switch (upgradeId)
            {
                case "LightningCogs_1":
                    currentRange += 1.5f;
                    cogsAmount += 2;
                    cogSpacing = currentRange / cogsAmount;
                    break;

                case "LightningCogs_2":
                    currentRange += 1.5f;
                    extraBranches = 2;
                    lightningDamageWidth *= 1.15f;
                    break;

                case "LightningCogs_3": // fork A
                    lightningTickTimes = 2;
                    lightningDamageWidth *= 1.2f;
                    break;

                case "LightningCogs_4": // fork B
                    // Будет обрабатываться в DrawLightningForChain + отдельный урон
                    hasLightningPillar = true;
                    currentDamage = Mathf.RoundToInt(currentDamage * 1.5f);
                    lightningTickTimes = Mathf.Max(1, lightningTickTimes - 1);
                    break;

                case "LightningCogs_5": // Магнитный резонанс (Active)
                    HasActiveAbility = true;
                    break;

                default:
                    Debug.LogWarning($"[LightningCogs] Unknown upgradeId: {upgradeId}");
                    break;
            }

            // Перестройка пула
            foreach (var cog in cogsPool)
                if (cog != null) Destroy(cog);
            cogsPool.Clear();

            for (int i = 0; i < currentProjectileCount * cogsAmount * 2; i++)
            {
                GameObject cog = Instantiate(data.projectilesPrefabs[0], transform);
                cog.SetActive(false);
                cogsPool.Enqueue(cog);
            }
        }

        protected override void ActivateAbility()
        {
            if (!HasActiveAbility) return;
            StartCoroutine(MagneticResonance());
        }

        private IEnumerator MagneticResonance()
        {
            // Притягиваем врагов к шестерням перед ударом молнии
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

            float pullDuration = 0.5f;
            float elapsed = 0f;

            while (elapsed < pullDuration)
            {
                foreach (var chain in activeChains)
                {
                    foreach (var cogPos in chain.positions)
                    {
                        foreach (var enemy in enemies)
                        {
                            if (enemy == null) continue;
                            float dist = IsometricExtension.IsoDistance(enemy.transform.position, cogPos);
                            if (dist <= currentRange * 0.5f)
                            {
                                Vector3 pullDir = (cogPos - enemy.transform.position).normalized;
                                enemy.transform.position += pullDir * 3f * Time.deltaTime;
                            }
                        }
                    }
                }
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
    }
}
