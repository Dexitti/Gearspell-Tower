using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Equipment
{
    public class AntennaController : EquipmentController
    {
        Vector3 firePoint = Vector3.zero;

        protected override void OnEnable()
        {
            base.OnEnable();
            firePoint = towerTransform.position + new Vector3(0, 0.28f, 0);
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
