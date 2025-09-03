using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Static.Path.Sound
{
    public enum SoundType
    {
        BGM,
        SFX
    }

    public class SoundPath
    {
        public static string GetSoundPath(SoundType soundType, string resName)
        {
            return $"Assets/GameRes/Audio/{soundType}/{resName}";
        }
    }
}