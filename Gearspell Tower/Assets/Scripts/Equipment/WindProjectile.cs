using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    Vector3 direction;
    int damage;
    float range;

    private bool hasPiercing;
    private int pierceCount;
    private float armorPenetration;
    private HashSet<GameObject> hitEnemies = new HashSet<GameObject>();

    public Vector3 Direction { get => direction; set => direction = value; }
    public int Damage { get => damage; set => damage = value; }
    public float Range { get => range; set => range = value; }

    public void SetPiercing(int count)
    {
        hasPiercing = true;
        pierceCount = count;
    }

    public void SetArmorPenetration(float penMult)
    {
        armorPenetration = penMult;
    }

    private void Start()
    {
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f);
    }

    void Update()
    {
        transform.position += IsometricExtension.IsoMovement(direction, speed);

        if (IsometricExtension.IsoDistance(G.Tower.transform.position, transform.position) >= range)
        {
            StartCoroutine(PlayHitAnimationAndDestroy());
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") || other.CompareTag("FlyingEnemy"))
        {
            GameObject enemy = other.gameObject;

            if (hasPiercing && hitEnemies.Contains(enemy))
                return;

            HealthComponent enemyHP = other.GetComponent<HealthComponent>();
            if (enemyHP != null)
            {
                enemyHP.TakeDamage(Damage);
                hitEnemies.Add(enemy);
            }

            if (!hasPiercing || hitEnemies.Count >= pierceCount)
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
