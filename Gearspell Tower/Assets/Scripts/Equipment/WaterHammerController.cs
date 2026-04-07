using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

namespace Assets.Scripts.Equipment
{
    public class WaterHammerController : EquipmentController
    {

        protected override IEnumerator Attack()
        {
            Transform target = GetStrongestEnemy();
            if (target == null || Vector3.Distance(towerTransform.position, target.position) > currentRange) yield break;

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
            GameObject[] enemyArray = GameObject.FindGameObjectsWithTag("Enemy");
            if (enemyArray.Length == 0) return null;
            return enemyArray
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
