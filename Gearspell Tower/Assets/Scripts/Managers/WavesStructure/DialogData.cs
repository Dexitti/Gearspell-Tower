using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class DialogData
{
    [Header("Parameters")]
    public string speakerName;
    //public Sprite speakerPortrait;
    public string text;
    //public AudioClip voiceClip;

    public float duration = 3f;
    public bool autoAdvance = true;
    public int appearance = 1; // -1 - до, 0 - во время, 1 - после волны
}
