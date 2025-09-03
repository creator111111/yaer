using Game.GameMgr;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.GameSceneManager.Base;
using GameFramework.UnityRuntimeExtend.Component;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Physics
{
    public class KnockBackComponent : BaseGFComponentMono
    {
        public float knockBackDuration = 0.5f; // 后退的持续时间
        public float bounceHeight = 0.5f; // 抛物线抖动的高度
        public float bounceFrequency = 2f; // 抖动频率
        private float elapsedTime;

        private bool isInit;
        private bool isKinematic;
        private bool isKnockedBack;

        private Vector2 knockBackDirection; // 受击方向
        private float knockBackDistance; // 后退的距离

        private Rigidbody2D rg;
        private Vector2 startPos; // 起始位置
        BaseGameSceneManager sceneMgr;
        private void FixedUpdate()
        {
            if (isKnockedBack)
            {
                
                if (sceneMgr != null && sceneMgr.GetSceneObjAniIsPause())
                {
                    return;
                }
                elapsedTime += Time.fixedDeltaTime;

                // 计算后退的进度
                var t = elapsedTime / knockBackDuration;
                t = Mathf.Clamp01(t);

                // 计算后退的水平位移
                var knockBackOffset = knockBackDirection * (knockBackDistance * t);

                // 添加垂直抛物线抖动
                var bounceOffset = Mathf.Sin(t * Mathf.PI * bounceFrequency) * bounceHeight * (1 - t);

                // 计算新位置
                var newPosition = startPos + knockBackOffset + new Vector2(0, bounceOffset);

                // 使用 MovePosition 移动
                rg.MovePosition(newPosition);

                // 检查是否完成后退
                if (t >= 1f) isKnockedBack = false;
            }
        }

        public void Init(Rigidbody2D rg)
        {
            this.rg = rg;
            if (this.rg == null)
            {
                Debug.LogWarning(name + "的KnockBackEffect未初始化成功");
                return;
            }
            
            isInit = true;
        }

        public void ApplyKnockBack(Vector2 direction, float backDistance = 0f)
        {
            if (isInit == false) return;

            knockBackDirection = direction;
            knockBackDistance = backDistance;
            startPos = transform.position;
            elapsedTime = 0f;
            isKnockedBack = true;
        }
        // 设置击退效果
        public void SetKnockBaseData(float breakHeight, float breakTime)
        {
            bounceHeight = breakHeight;
            knockBackDuration = breakTime;
        }

        // 停止击退动作
        public void StopKnockBackEffect()
        {
            isKnockedBack = false;
        }

        public void SetSceneMgr(BaseGameSceneManager mgr)
        {
            sceneMgr = mgr;
        }

        protected override void OnInit()
        {
            
        }
    }
}