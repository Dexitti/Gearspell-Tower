using UnityEngine;

public class WindProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    public int Damage { get; private set; } // Разместить data?!

    void Update()
    {
        transform.position += (Vector3)transform.right * speed * Time.deltaTime;
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

            Destroy(gameObject);
        }
    }
}
