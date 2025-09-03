using Game.GameRuntime.Entities.Component.Effect;
using Game.GameRuntime.GameSceneManager.SubManager.Buff;
using UnityEngine;

namespace Game.GameRuntime.Entities.Buff.Monster.Tired
{
    public class SlimeTiredBuff : BaseBuff
    {
        public void CreateEffect(SpriteRenderer parentSr, Transform parent)
        {
            var cpn = UnityEngine.Object.Instantiate(BuffManager.GetPrefabsAsset("Effect", "Effect_MonsterState_Tired"), parent)
                ?.GetComponent<AnimaEffectComponent>();
            if (cpn == null)
            {
                Debug.LogError("特效组件获取失败");
                return;
            }

            cpn.transform.localPosition = Vector3.zero;
            cpn.transform.localScale = Vector3.one * 2f;
            cpn.FollowSrSortLayer(parentSr, true);
            cpn.Play(5);
        }
    }
}