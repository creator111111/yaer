using Game.Static.Enum;
using UnityEngine;

namespace Game.GameMgr.Manager.Effect
{
    public class EffectManagerSettings : MonoBehaviour
    {
        public bool usePool = true;
        public EResLoadType defaultLoadType = EResLoadType.Addressable;
    }
}