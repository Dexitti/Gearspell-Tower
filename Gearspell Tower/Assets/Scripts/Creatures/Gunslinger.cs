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
    [SerializeField] private float orbitalSpeed = 6f;

    private enum State { Moving, Orbiting, Shooting }
    private State currentState = State.Moving;
    private Vector3 dirToTower;
    private Coroutine attackCoroutine;
    private bool isAttacking = false;

    private float orbitAngle;

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
                    currentState = State.Shooting;
                    Vector3 offset = transform.position - towerPosition;
                    orbitAngle = Mathf.Atan2(offset.y / IsometricExtension.isoRatio, offset.x) * Mathf.Rad2Deg;
                }
                break;

            case State.Shooting:
                Attack(G.Tower.gameObject);
                break;

            case State.Orbiting:
                if (!isAttacking)
                {
                    Vector3 towerPos = G.Tower.Position;
                    orbitAngle += orbitalSpeed * Time.deltaTime * 50f;
                    float rad = orbitAngle * Mathf.Deg2Rad;
                    Vector3 offsetPos = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad) * 0.5f, 0) * fireDistance;
                    Vector3 targetPos = towerPos + offsetPos;
                    transform.position = Vector3.MoveTowards(transform.position, targetPos, currentSpeed * Time.deltaTime);

                    Vector3 lookDir = towerPosition - transform.position;
                    if (lookDir.x < 0) sprite.flipX = !sprite.flipX;
                    currentState = State.Shooting;
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
        StopCoroutine(attackCoroutine);
        currentState = State.Orbiting;
    }
}
