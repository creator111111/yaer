using UnityEngine;

namespace Game.GameRuntime.Entities.Monster.Slime
{
    public class SlimeResHelper : MonoBehaviour
    {
        public string weakBuffEffectPath;

        private void Start()
        {
            if (weakBuffEffectPath == null) Debug.LogError(name + ":没有设置WeakBuff预制体路径");
        }
    }
}