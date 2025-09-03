using Game.GameRuntime.Entities.Effect.Slime;
using Game.GameRuntime.GameSceneManager.SubManager.Buff;
using UnityEngine;

namespace Game.GameRuntime.Entities.Buff.Monster.Angry
{
    public class SlimeAngryBuff : BaseBuff
    {
        public SlimeAngryBuffEffect CreateEffect(Transform parent)
        {
            var cpn = UnityEngine.Object.Instantiate(BuffManager.GetPrefabsAsset("Effect", "Effect_MonsterState_Angry"), parent)?.GetComponent<SlimeAngryBuffEffect>();
            if (cpn == null)
            {
                Debug.LogError("特效组件获取失败");
                return null;
            }

            cpn.transform.localPosition = Vector3.zero;

            return cpn;
        }
    }
}