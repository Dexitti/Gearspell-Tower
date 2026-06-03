using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Mine : MonoBehaviour
{
    [SerializeField] private int damage = 10;
    [SerializeField] private Animator animator;
    [SerializeField] private ParticleSystem effect;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            HealthComponent enemyHP = other.GetComponent<HealthComponent>();
            if (enemyHP != null)
            {
                enemyHP.TakeDamage(damage);
                G.AudioManager.PlaySFXAtPosition("explosion", other.transform.position, 10f);
            }

            StartCoroutine(Explode());
        }
    }

    private IEnumerator Explode()
    {
        Collider2D col = GetComponent<Collider2D>();
        col.enabled = false;

        animator.Play("Explosion");
        effect.Play();

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(stateInfo.length);
        Destroy(gameObject);
    }
}
