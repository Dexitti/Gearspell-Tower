using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class LightningCogsDecor : MonoBehaviour
{
    public void PlayIdleSound()
    {
        G.AudioManager.PlaySFX("electrical double", 0.18f);
    }
}
