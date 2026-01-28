using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class Creature : MonoBehaviour
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private int damage = 5;
    [SerializeField] private float attackCooldown = 1f;

    private Tower tower;
    private Vector3 _towerPosition;

    [Header("Коллайдеры")]
    [SerializeField] private Collider2D bodyCollider;     // Для физики и получения урона
    [SerializeField] private Collider2D hitbox;     // Trigger для атаки башни

    public Action<Creature> OnReachedTower;

    public void Awake()
    {
        tower = GameObject.Find("Tower").GetComponent<Tower>();
    }

    private void OnEnable()
    {
        _towerPosition = tower.Position;
    }

    private void Update()
    {
        //Move
        transform.position = Vector3.MoveTowards(transform.position, _towerPosition, speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Tower")
        {
            StartCoroutine(AttackTower());
        }
    }

    IEnumerator AttackTower()
    {
        while (true)
        {
            HealthComponent towerHealthComponent = GameObject.Find("Tower").GetComponent<HealthComponent>();
            if (towerHealthComponent == null || !towerHealthComponent.isAlive) yield break;
            towerHealthComponent.TakeDamage(damage);
            Debug.Log($"Trigger: Tower take {damage} damage!");

            // Анимация атаки

            yield return new WaitForSeconds(attackCooldown);
        }
    }

    
}
