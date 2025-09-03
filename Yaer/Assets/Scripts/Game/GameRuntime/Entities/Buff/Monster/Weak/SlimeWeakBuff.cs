using Game.GameRuntime.Entities.Effect.Slime;
using Game.GameRuntime.GameSceneManager.SubManager.Buff;
using UnityEngine;

namespace Game.GameRuntime.Entities.Buff.Monster.Weak
{
    public class SlimeWeakBuff : BaseBuff
    {
        private SlimeWeakBuffEffect effect;
        private int index;
        private float slowMoveSpeed;

        public override void Init(IBuffManager buffManager)
        {
            base.Init(buffManager);

            index = 0;
        }

        public override void Apply()
        {
            base.Apply();

            ApplyState(index);
        }

        public void SetPos(Vector2 pos)
        {
            effect.transform.position = pos;
        }

        public void AddIndex()
        {
            index++;
            ApplyState(index);
        }

        private void ApplyState(int i)
        {
            // if (prefabsIsCreated == false)
            // {
            //      effect = UnityEngine.Object.Instantiate(prefab).GetComponent<SlimeWeakBuffEffect>();
            // }

            switch (i)
            {
                case 0:

                    break;
                case 1:

                    break;
                case 2:

                    break;
            }
        }

        public SlimeWeakBuffEffect CreateEffect(Transform parent)
        {
            var cpn = UnityEngine.Object.Instantiate(BuffManager.GetPrefabsAsset("Effect", "Effect_MonsterState_Weak"), parent)
                ?.GetComponent<SlimeWeakBuffEffect>();

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