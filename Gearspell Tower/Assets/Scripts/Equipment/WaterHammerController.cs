using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Equipment
{
    public class WaterHammerController : EquipmentController
    {
        private bool hasStun = false;
        private float stunRadius = 1f;
        private float stunDuration = 1f;
        private bool hasArmorBreak = false;
        private int bonusDamage = 0;
        private int abilityHitCount = 3;

        protected override IEnumerator Attack()
        {
            Transform target = GetStrongestEnemy();
            if (target == null || IsometricExtension.IsoDistance(towerTransform.position, target.position) > currentRange) yield break;

            bool flipX = UnityEngine.Random.value > 0.5f;

            for (int i = 0; i < currentProjectileCount; i++)
            {
                GameObject hammer = Instantiate(data.projectilesPrefabs[0], GetSpawnPosition(target, flipX), Quaternion.identity, transform);

                SpriteRenderer sprite = hammer.GetComponent<SpriteRenderer>();
                CircleCollider2D spriteCollider = hammer.GetComponent<CircleCollider2D>();
                sprite.flipX = flipX;
                if (spriteCollider != null && flipX)
                    spriteCollider.offset *= new Vector2(-1f, 1);

                WaterHammerProjectile projScript = hammer.GetComponent<WaterHammerProjectile>();
                projScript.Target = target;
                projScript.Damage = currentDamage;
            }

            yield break;
        }

        private Transform GetStrongestEnemy()
        {
            List<GameObject> enemyList = GameObject.FindGameObjectsWithTag("Enemy").ToList();
            enemyList.AddRange(GameObject.FindGameObjectsWithTag("FlyingEnemy"));
            if (enemyList.Count == 0) return null;
            return enemyList
                .OrderByDescending(enemy => enemy.GetComponent<HealthComponent>().CurrentHealth)
                .FirstOrDefault().transform;
        }

        private Vector3 GetSpawnPosition(Transform target, bool flip)
        {
            float direction = flip ? 1 : -1;

            Collider2D enemyCollider = target.GetComponent<Collider2D>();
            Vector3 spawnOffset = new Vector3(
                 direction * target.GetComponent<Collider2D>().bounds.extents.x * 2f,
                -target.GetComponent<Collider2D>().bounds.extents.y * 0.5f,
                0
            );

            return target.position + spawnOffset;
        }

        protected override void ApplyEffect(string upgradeId)
        {
            switch (upgradeId)
            {
                case "WaterHammer_1":
                    currentDamage = Mathf.RoundToInt(currentDamage * 1.6f);
                    break;

                case "WaterHammer_2":
                    currentAttackCooldown *= 0.65f;
                    currentProjectileCount += 1;
                    break;

                case "WaterHammer_3": // fork A
                    hasStun = true;
                    stunRadius = 1f;
                    stunDuration = 1f;
                    break;

                case "WaterHammer_4": // fork B
                    hasArmorBreak = true;
                    bonusDamage = Mathf.RoundToInt(currentDamage * 0.33f);
                    break;

                case "WaterHammer_5":
                    HasActiveAbility = true;
                    abilityHitCount = 3;
                    break;

                default:
                    Debug.LogWarning($"[WaterHammer] Unknown upgradeId: {upgradeId}");
                    break;
            }
        }

        protected override void ActivateAbility()
        {
            if (!HasActiveAbility) return;
            StartCoroutine(Avalanche());
        }

        private IEnumerator Avalanche()
        {
            Transform target = GetStrongestEnemy();
            if (target == null) yield break;

            float stunDurMult = 0.5f;
            for (int hit = 0; hit < abilityHitCount; hit++)
            {
                // Каждый следующий удар сильнее
                int hitDamage = Mathf.RoundToInt(currentDamage * (1f + hit * 0.5f));
                float hitRadius = 1.5f + hit * 0.5f;

                bool flipX = hit % 2 == 0;
                Vector3 spawnPos = GetSpawnPosition(target, flipX) + Vector3.up * 1.5f;

                GameObject hammer = Instantiate(data.projectilesPrefabs[0], spawnPos, Quaternion.identity, transform);

                SpriteRenderer sprite = hammer.GetComponent<SpriteRenderer>();
                sprite.flipX = flipX;

                WaterHammerProjectile projScript = hammer.GetComponent<WaterHammerProjectile>();
                projScript.Target = target;
                projScript.Damage = hitDamage;
                projScript.SetStunParameters(true, hitRadius, 0.1f + hit * stunDurMult);
                projScript.HasArmorBreak = hasArmorBreak;
                projScript.BonusDamage = bonusDamage;

                yield return new WaitForSeconds(0.4f);
            }
        }
    }
}
