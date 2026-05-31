using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class MageShot : MonoBehaviour
{
    [SerializeField] private float speed = 7f;
    Vector3 direction;
    int damage;

    public Vector3 Direction { get => direction; set => direction = value; }
    public int Damage { get => damage; set => damage = value; }

    void Update()
    {
        transform.position += IsometricExtension.IsoMovement(direction, speed);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Tower"))
        {
            var towerHealth = other.GetComponent<HealthComponent>();
            if (towerHealth != null && towerHealth.isAlive)
            {
                towerHealth.TakeDamage(damage);
                G.AudioManager?.PlaySFX("hit");
            }
        }
        Destroy(gameObject, 0.1f);
    }
}
