using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Equipment
{
    public class KineticArmorController : EquipmentController
    {
        [Header("Kinetic Armor Settings")]
        [SerializeField] private float reflectAngleWidth = 45f;
        [SerializeField] private ParticleSystem reflectEffect;

        private HealthComponent towerHealth;
        private int currentShieldCharges;
        private bool isShieldActive = false;
        private Coroutine rechargeCoroutine;

        protected override void OnEnable()
        {
            base.OnEnable();
            reflectEffect = data.projectilesPrefabs[0].GetComponent<ParticleSystem>();

            if (G.Tower != null)
            {
                towerHealth = G.Tower.GetComponent<HealthComponent>();
                if (towerHealth != null)
                {
                    towerHealth.OnBeforeTakeDamage += IsEnemyHit;
                }
            }
        }

        private void Start()
        {
            currentShieldCharges = currentProjectileCount;
            isShieldActive = true;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (towerHealth != null)
            {
                towerHealth.OnBeforeTakeDamage -= IsEnemyHit;
            }
        }

        private void IsEnemyHit(int damage, Action<int> setDamage)
        {
            Debug.Log($"IsEnemyHit ВЫЗВАН! damage={damage}, isShieldActive={isShieldActive}, charges={currentShieldCharges}");
            if (isShieldActive && currentShieldCharges > 0)
            {
                currentShieldCharges--;
                ReflectAttack(damage);
                setDamage(0);
                Debug.Log($"Урон ОТМЕНЕН! Осталось зарядов: {currentShieldCharges}");

                if (currentShieldCharges <= 0)
                {
                    isShieldActive = false;
                    if (decorationInstance != null)
                        decorationInstance.SetActive(false);
                    ParticleSystem breakFX = GetComponentInChildren<ParticleSystem>();
                    if (breakFX != null)
                        breakFX.Play();
                }
                else StartCoroutine(ReloadCharge());
            }
            else Debug.Log($"Урон НЕ отменен! isShieldActive={isShieldActive}, charges={currentShieldCharges}");
        }

        private void ReflectAttack(int damage)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            GameObject closestEnemy = null;
            float closestDistance = currentRange;

            foreach (GameObject enemy in enemies)
            {
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = enemy;
                }
            }

            if (closestEnemy != null)
            {
                Vector3 direction = (closestEnemy.transform.position - towerTransform.position).normalized;
                Vector3 spawnPosition = towerTransform.position + direction * 0.25f;

                GameObject reflect = Instantiate(data.projectilesPrefabs[0], spawnPosition, Quaternion.LookRotation(Vector3.forward, direction));

                int reflectDamage = Mathf.RoundToInt(damage * 0.5f); // 50% от полученного урона

                Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(closestEnemy.transform.position, currentSize);
                foreach (Collider2D hit in hitEnemies)
                {
                    if (hit.CompareTag("Enemy"))
                    {
                        HealthComponent enemyHealth = hit.GetComponent<HealthComponent>();
                        if (enemyHealth != null)
                        {
                            enemyHealth.TakeDamage(reflectDamage);
                        }
                    }
                }

                if (reflectEffect != null)
                {
                    reflectEffect.transform.position = towerTransform.position;
                    reflectEffect.Play();
                }
            }
        }

        private IEnumerator ReloadCharge()
        {
            yield return new WaitForSeconds(currentAttackCooldown);
            if (currentShieldCharges < currentProjectileCount)
            {
                currentShieldCharges++;
                if (!isShieldActive && currentShieldCharges > 0)
                {
                    isShieldActive = true;
                    if (decorationInstance != null)
                        decorationInstance.SetActive(true);
                }
            }
        }

        protected override IEnumerator Attack()
        {
            yield return null;
        }

        protected override void ApplyEffect(string upgradeId)
        {
            throw new NotImplementedException();
        }

        protected override void Upgrade(int upgradeIndex)
        {
            throw new NotImplementedException();
        }
        
        protected override void ActivateAbility()
        {
            throw new NotImplementedException();
        }
    }
}
