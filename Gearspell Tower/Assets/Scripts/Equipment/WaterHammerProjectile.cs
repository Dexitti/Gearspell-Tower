using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Assets.Scripts.Equipment
{
    public class WaterHammerProjectile : MonoBehaviour
    {
        private int damage;
        private bool hasHit = false;

        public int Damage { get => damage; set => damage = value; }
        public Transform Target {  get; set; }

        public void ColliderOn()
        {
            GetComponent<CircleCollider2D>().enabled = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (hasHit) return;

            if (other.transform == Target)
            {
                hasHit = true;
                HealthComponent enemyHP = Target.GetComponent<HealthComponent>();
                if (enemyHP != null)
                {
                    enemyHP.TakeDamage(Damage);
                }
            }

            StartCoroutine(PlayHitAnimationAndDestroy());
        }

        IEnumerator PlayHitAnimationAndDestroy()
        {
            GetComponent<CircleCollider2D>().enabled = false;

            Animator animator = GetComponent<Animator>();
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            yield return new WaitForSeconds(stateInfo.length);
            Destroy(gameObject);
        }
    }
}
