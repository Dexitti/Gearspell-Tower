using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class WaterHammerDecor : MonoBehaviour
{
    public void PlayIdleSound()
    {
        G.AudioManager.PlaySFX("pump thud", 0.6f);
    }
}
