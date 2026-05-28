using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Equipment
{
    public class CryogenicBeam : MonoBehaviour
    {
        private CryogenicStabilizerController controller;

        public void SetController(CryogenicStabilizerController ctrl)
        {
            controller = ctrl;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Enemy") || other.CompareTag("FlyingEnemy"))
            {
                controller.AddEnemyToBeam(other.gameObject);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Enemy") || other.CompareTag("FlyingEnemy"))
            {
                controller.RemoveEnemyFromBeam(other.gameObject);
            }
        }
    }
}
