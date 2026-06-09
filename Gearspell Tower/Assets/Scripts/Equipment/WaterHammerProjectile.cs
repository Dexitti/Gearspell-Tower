using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class WaterHammerProjectile : MonoBehaviour
{
    private int damage;
    private bool hasHit = false;

    private bool hasStun;
    private float stunRadius;
    private float stunDuration;
    private bool hasArmorBreak;
    private int bonusDamage;

    public int Damage { get => damage; set => damage = value; }
    public Transform Target { get; set; }
    public bool HasArmorBreak { get => hasArmorBreak; set => hasArmorBreak = value; }
    public int BonusDamage { get => bonusDamage; set => bonusDamage = value; }

    public void SetStunParameters(bool hasStun, float stunRadius, float stunDuration)
    {
        this.hasStun = hasStun;
        this.stunRadius = stunRadius;
        this.stunDuration = stunDuration;
    }

    public void ColliderOn()
    {
        GetComponent<CircleCollider2D>().enabled = true;
    }

    public void PlayWaterSound()
    {
        G.AudioManager.PlaySFX("water explosion", 0.35f);
    }

    private void Start()
    {
        Destroy(gameObject, 3f);
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

            if (hasStun)
            {
                Collider2D[] nearby = Physics2D.OverlapCircleAll(Target.position, stunRadius);
                foreach (var col in nearby)
                {
                    if (col.CompareTag("Enemy") || col.CompareTag("FlyingEnemy"))
                    {
                        CreatureController creature = col.GetComponent<CreatureController>();
                        if (creature != null)
                            creature.ApplyStun(stunDuration);
                    }
                }
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
