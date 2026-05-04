using System.Collections;
using UnityEngine;

public class WindProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    Vector3 direction;
    int damage;

    public Vector3 Direction { get => direction; set => direction = value; }
    public int Damage { get => damage; set => damage = value; }

    private void Start()
    {
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f + Random.Range(-5f, 5f));
    }

    void Update()
    {
        transform.position += IsometricExtension.IsoMovement(direction, speed);
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
        yield return null;
        Destroy(gameObject);
    }
}
