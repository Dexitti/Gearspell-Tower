using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
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

        private float reflectDamageMultiplier = 0.5f;
        private bool hasKnockback = false;
        private float knockbackForce;
        private bool convertToShield = false;
        private bool isFullCircle = false;
        private float noHitTimer = 0f;
        private float maxBonusRange = 2f;
        private bool provokeOtherEquipment = false;

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

        private void Update()
        {
            if (isFullCircle && isShieldActive)
            {
                noHitTimer += Time.deltaTime;
            }
        }

        private void IsEnemyHit(int damage, Action<int> setDamage)
        {
            Debug.Log($"IsEnemyHit ВЫЗВАН! damage={damage}, isShieldActive={isShieldActive}, charges={currentShieldCharges}");
            if (isShieldActive && currentShieldCharges > 0)
            {
                currentShieldCharges--;
                ReflectAttack(damage);
                G.AudioManager.PlaySFX("shield breach", 0.4f);
                setDamage(0);
                noHitTimer = 0f;
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
            float bonusRange = isFullCircle ? Mathf.Min(noHitTimer * 0.5f, maxBonusRange) : 0f;
            float effectiveRange = currentRange + bonusRange;

            int reflectDamage = Mathf.RoundToInt(damage * reflectDamageMultiplier);

            List<GameObject> allEnemies = GameObject.FindGameObjectsWithTag("Enemy").ToList();
            allEnemies.AddRange(GameObject.FindGameObjectsWithTag("FlyingEnemy"));
            if (isFullCircle)
            {
                // Атака по всем врагам вокруг
                foreach (var hit in allEnemies)
                {
                    if (hit.CompareTag("Enemy") || hit.CompareTag("FlyingEnemy"))
                    {
                        HealthComponent enemyHealth = hit.GetComponent<HealthComponent>();
                        if (enemyHealth != null)
                        {
                            enemyHealth.TakeDamage(reflectDamage);
                            if (hasKnockback)
                                ApplyKnockback(hit.gameObject);
                        }
                    }
                }
            }
            else
            {
                GameObject closestEnemy = null;
                float closestDistance = effectiveRange;

                foreach (GameObject enemy in allEnemies)
                {
                    float distance = IsometricExtension.IsoDistance(transform.position, enemy.transform.position);
                    if (distance < closestDistance)
                    {
                        Vector3 toEnemy = (enemy.transform.position - towerTransform.position).normalized;
                        float angle = Vector3.Angle(towerTransform.right, toEnemy);
                        if (angle <= reflectAngleWidth * 0.5f)
                        {
                            closestDistance = distance;
                            closestEnemy = enemy;
                        }
                    }
                }

                if (closestEnemy != null)
                {
                    HealthComponent enemyHealth = closestEnemy.GetComponent<HealthComponent>();
                    if (enemyHealth != null)
                    {
                        enemyHealth.TakeDamage(reflectDamage);
                        if (hasKnockback)
                            ApplyKnockback(closestEnemy);
                    }
                }

                // Конвертор: прибавляем заряд щита
                if (convertToShield)
                {
                    currentShieldCharges = Mathf.Min(currentShieldCharges + 1, currentProjectileCount);
                    if (!isShieldActive && currentShieldCharges > 0)
                    {
                        isShieldActive = true;
                        if (decorationInstance != null)
                            decorationInstance.SetActive(true);
                    }
                }

                // Резонанс: провоцируем атаку других снаряжений
                if (provokeOtherEquipment)
                {
                    var controllers = G.EquipmentManager?.GetActiveControllers();
                    if (controllers != null)
                    {
                        foreach (var ctrl in controllers)
                        {
                            //if (ctrl != this)
                            //    ctrl.ActivateAttack();
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

        private void ApplyKnockback(GameObject enemy)
        {
            Vector3 knockDir = (enemy.transform.position - detectionOrigin).normalized;
            enemy.transform.position += knockDir * knockbackForce * 0.5f;

            CreatureController creature = enemy.GetComponent<CreatureController>();
            if (creature != null)
                creature.ApplyStun(0.1f);
        }

        private IEnumerator ReloadCharge()
        {
            yield return new WaitForSeconds(currentAttackCooldown);
            if (currentShieldCharges < currentProjectileCount)
            {
                G.AudioManager.PlaySFX("shine", 0.6f);
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
            switch (upgradeId)
            {
                case "KineticArmor_1":
                    currentAttackCooldown *= 0.66f;
                    break;

                case "KineticArmor_2":
                    reflectDamageMultiplier = 1.5f;
                    hasKnockback = true;
                    knockbackForce = 5f;
                    break;

                case "KineticArmor_3": // fork A
                    currentProjectileCount += 1;
                    convertToShield = true;
                    break;

                case "KineticArmor_4": // fork B
                    currentProjectileCount += 1;
                    isFullCircle = true;
                    currentRange *= 0.8f;
                    break;

                case "KineticArmor_5":
                    currentProjectileCount += 1;
                    provokeOtherEquipment = true;
                    HasActiveAbility = true;
                    break;

                default:
                    Debug.LogWarning($"[KineticArmor] Unknown upgradeId: {upgradeId}");
                    break;
            }
        }
        
        protected override void ActivateAbility()
        {
            if (!HasActiveAbility) return;

            // Мгновенно провоцируем все снаряжения
            var controllers = G.EquipmentManager?.GetActiveControllers();
            if (controllers != null)
            {
                foreach (var ctrl in controllers)
                {
                    //if (ctrl != this)
                    //    ctrl.ActivateAttack();
                }
            }
        }
    }
}
