using System;
using System.Collections.Generic;
using UnityEngine;

public class PoisonSpike : MonoBehaviour
{
    [SerializeField] private float speed = 13f;
    Vector3 direction;
    int damage;
    float range;

    public Vector3 Direction { get => direction; set => direction = value; }
    public int Damage { get => damage; set => damage = value; }
    public float Range { get => range; set => range = value; }
    public Queue<GameObject> Pool { get; set; }

    void Update()
    {
        transform.position += IsometricExtension.IsoMovement(direction, speed);

        if (IsometricExtension.IsoDistance(G.Tower.transform.position, transform.position) >= range)
        {
            ReturnToPool();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            other.GetComponent<HealthComponent>()?.TakeDamage(Damage);
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        gameObject.SetActive(false);
        Pool?.Enqueue(gameObject);
    }
}
