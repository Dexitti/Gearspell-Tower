using System.Collections;
using UnityEngine;

public class FireDrillProjectile : MonoBehaviour
{
    [SerializeField] private float fallSpeed = 15f;
    int damage;

    [SerializeField] private Animator animator;

    public int Damage { get => damage; set => damage = value; }

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
        if (other.CompareTag("Enemy"))
        {
            HealthComponent enemyHP = other.GetComponent<HealthComponent>();
            if (enemyHP != null)
            {
                enemyHP.TakeDamage(Damage);
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
