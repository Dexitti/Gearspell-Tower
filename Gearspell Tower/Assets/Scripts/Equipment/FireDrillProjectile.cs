using System.Collections;
using UnityEngine;

public class FireDrillProjectile : MonoBehaviour
{
    [SerializeField] private float fallSpeed = 15f;
    [SerializeField] private Animator animator;

    int damage;
    private bool hasStun = false;
    private float stunChance = 0f;
    private float stunDuration = 1f;

    public int Damage { get => damage; set => damage = value; }

    public void SetStun(bool isStun, float chance, float duration)
    {
        hasStun = isStun;
        stunChance = chance;
        stunDuration = duration;
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        Destroy(gameObject, 2f);
    }

    private void Update()
    {
        transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("FlyingEnemy"))
        {
            HealthComponent enemyHP = other.GetComponent<HealthComponent>();
            if (enemyHP != null)
            {
                enemyHP.TakeDamage(Damage);

                if (hasStun && Random.value < stunChance)
                {
                    CreatureController creature = other.GetComponent<CreatureController>();
                    if (creature != null)
                        creature.ApplyStun(stunDuration);
                }
            }

            StartCoroutine(PlayHitAnimationAndDestroy());
        }
    }

    IEnumerator PlayHitAnimationAndDestroy()
    {
        GetComponent<Collider2D>().enabled = false;
        fallSpeed = 0;
        animator.Play("hit");

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(stateInfo.length);
        Destroy(gameObject);
    }
}
