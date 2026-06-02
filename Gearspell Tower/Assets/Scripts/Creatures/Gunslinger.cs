using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Gunslinger : CreatureController
{
    [SerializeField] private float fireDistance = 3f;
    [SerializeField] private int fireTimes = 2;

    [SerializeField] private float orbitalSpeed = 1.2f;

    private enum State { Moving, Orbiting, Shooting }
    private State currentState = State.Moving;

    private Vector3 dirToTower;
    private Coroutine attackCoroutine;
    private bool isAttacking = false;

    private float currentAngle;
    private float currentOrbitDuration;
    private float orbitTimer = 0f;
    private int orbitDirection = 1;

    protected override void Move()
    {
        float distance = IsometricExtension.IsoDistance(transform.position, towerPosition);
        switch (currentState)
        {
            case State.Moving:
                dirToTower = IsometricExtension.IsoDirection(transform.position, towerPosition);
                Vector3 movement = IsometricExtension.IsoMovement(dirToTower, currentSpeed);
                transform.position += movement;

                if (distance <= fireDistance)
                {
                    Vector3 offset = transform.position - towerPosition;
                    currentAngle = Mathf.Atan2(offset.y / IsometricExtension.isoRatio, offset.x);

                    currentState = State.Shooting;
                    Attack(G.Tower.gameObject);
                }
                break;

            case State.Shooting:
                break;

            case State.Orbiting:
                if (!isAttacking)
                {
                    currentAngle += orbitDirection * orbitalSpeed * Time.deltaTime;
                    currentAngle = Mathf.Repeat(currentAngle, Mathf.PI * 2f);
                    Vector3 offsetPos = new Vector3(fireDistance * Mathf.Cos(currentAngle), fireDistance * Mathf.Sin(currentAngle) * IsometricExtension.isoRatio);
                    transform.position = towerPosition + offsetPos;

                    // Поворот спрайта
                    Vector3 lookDir = towerPosition - transform.position;
                    if (lookDir.x < 0 && !sprite.flipX) sprite.flipX = true;
                    else if (lookDir.x > 0 && sprite.flipX) sprite.flipX = false;

                    orbitTimer += Time.deltaTime;
                    if (orbitTimer >= currentOrbitDuration)
                    {
                        orbitTimer = 0f;
                        currentState = State.Shooting;
                        Attack(G.Tower.gameObject);
                    }
                }
                break;
        }
    }

    protected override void Attack(GameObject target)
    {
        if (isAttacking) return;
        fireTimes = UnityEngine.Random.Range(2, 4);
        isAttacking = true;
        attackCoroutine = StartCoroutine(Shoot());
        
    }

    private IEnumerator Shoot()
    {
        dirToTower = IsometricExtension.IsoDirection(transform.position, towerPosition);
        for (int i = 0; i < fireTimes; i++)
        {
            float angle = Mathf.Atan2(dirToTower.y, dirToTower.x) * Mathf.Rad2Deg;
            var shot = Instantiate(data.attackPrefabs[0], transform.position, Quaternion.Euler(0, 0, angle), transform);
            var mageShot = shot.GetComponent<MageShot>();
            mageShot.Direction = dirToTower;
            mageShot.Damage = currentDamage;
            G.AudioManager?.PlaySFX("zap");
            yield return new WaitForSeconds(attackCooldown);
        }

        isAttacking = false;
        attackCoroutine = null;
        orbitTimer = 0f;
        orbitDirection = UnityEngine.Random.value > 0.5f ? 1 : -1;
        currentOrbitDuration = UnityEngine.Random.Range(25f, 55f) * Mathf.Deg2Rad / orbitalSpeed;
        currentState = State.Orbiting;
    }
}
