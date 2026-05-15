using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace Assets.Scripts.Equipment
{
    public class AntennaController : EquipmentController
    {
        Vector3 firePoint = Vector3.zero;
        bool hasEnemies = false;
        bool isAttacking = false;

        private ParticleSystem wavePS;
        private ParticleSystem.MainModule mainModule;
        private ParticleSystem.EmissionModule emissionModule;
        private ParticleSystem.ShapeModule shapeModule;

        protected override void OnEnable()
        {
            base.OnEnable();
            firePoint = towerTransform.position + new Vector3(0, 1.25f, 0);
            wavePS = GetComponent<ParticleSystem>();
            mainModule = wavePS.main;
            emissionModule = wavePS.emission;
            shapeModule = wavePS.shape;
            SetupVisualParameters();
            wavePS.Emit(0);
        }

        private void SetupVisualParameters()
        {
            Transform rangeBoundary = transform.Find("RangeBoundary");
            rangeBoundary.position = towerTransform.position;
            rangeBoundary.localScale = new Vector3(currentRange, currentRange);

            shapeModule.radius = currentRange * 0.5f;
            mainModule.startSpeed = currentRange * 1.5f;
            mainModule.startLifetime = currentRange / (currentRange * 1.5f);
            emissionModule.SetBurst(0, new ParticleSystem.Burst(
                _time: 0,
                _count: currentProjectileCount,
                _cycleCount: 1,
                _repeatInterval: 0.5f
            ));
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            Transform mortar = towerTransform.Find("Mortars and Parapet(Clone)");
            if (mortar != null)
            {
                Transform antennaColumn = towerTransform.Find("Antenna column(Clone)");
                if (antennaColumn != null)
                    Destroy(antennaColumn.gameObject);
            }
        }

        protected override IEnumerator Attack()
        {
            if (isAttacking) yield break;
            isAttacking = true;
            HaveAndDamageEnemiesInRange();

            if (hasEnemies)
                wavePS.Emit(currentProjectileCount);
            SetupVisualParameters();

            yield return new WaitForSeconds(currentAttackCooldown);
            isAttacking = false;
        }

        private void HaveAndDamageEnemiesInRange()
        {
            GameObject[] enemyArray = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (var enemy in enemyArray)
            {
                if (IsometricExtension.IsoDistance(towerTransform.position, enemy.transform.position) <= currentRange)
                {
                    hasEnemies = true;
                    HealthComponent enemyHP = enemy.GetComponent<HealthComponent>();
                    if (enemyHP != null)
                    {
                        enemyHP.TakeDamage(currentDamage);
                    }
                }
                else hasEnemies = false;
            }
        }

        protected override void ApplyEffect(string upgradeId)
        {
            throw new NotImplementedException();
        }

        protected override void ActivateAbility()
        {
            throw new NotImplementedException();
        }
    }
}
