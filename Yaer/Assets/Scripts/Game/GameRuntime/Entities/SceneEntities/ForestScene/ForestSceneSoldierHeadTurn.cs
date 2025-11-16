using Game.GameRuntime.Entities.Base.BaseSceneObj;
using UnityEngine;

namespace Game.GameRuntime.Entities.SceneEntities.ForestScene
{
    /// <summary>
    /// 森林场景士兵头部转向控制器
    /// 管理士兵身体和两个头部状态的切换
    /// </summary>
    public class ForestSceneSoldierHeadTurn : BaseSceneEntityLogic
    {
        [Header("身体和头部组件")]
        [SerializeField] private GameObject soldierBody;
        [SerializeField] private GameObject normalHead;  // 正常头部
        [SerializeField] private GameObject turnedHead;  // 转向头部

        [Header("转向设置")]
        [SerializeField] private float turnDuration = 1.0f;  // 转向动画时长
        [SerializeField] private bool isHeadTurned = false;  // 当前头部状态


        /// <summary>
        /// 头部状态枚举
        /// </summary>
        public enum HeadState
        {
            Normal,  // 正常状态
            Turned   // 转向状态
        }

        private HeadState currentHeadState = HeadState.Normal;
        private Animator bodyAnimator;

        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);
   
            // 初始化组件
            InitializeComponents();
            
            // 设置初始状态
            SetHeadState(currentHeadState, false);
        }

        /// <summary>
        /// 初始化组件
        /// </summary>
        private void InitializeComponents()
        {
            // 获取身体动画器
            if (soldierBody != null)
            {
                bodyAnimator = soldierBody.GetComponent<Animator>();
            }

            // 验证必要组件
            if (soldierBody == null)
                Debug.LogWarning("士兵身体组件未设置", gameObject);
            
            if (normalHead == null)
                Debug.LogWarning("正常头部组件未设置", gameObject);
            
            if (turnedHead == null)
                Debug.LogWarning("转向头部组件未设置", gameObject);
        }

        /// <summary>
        /// 切换头部状态
        /// </summary>
        /// <param name="targetState">目标状态</param>
        /// <param name="playAnimation">是否播放动画</param>
        public void SetHeadState(HeadState targetState, bool playAnimation = true)
        {
            if (currentHeadState == targetState)
                return;

            currentHeadState = targetState;
            isHeadTurned = (targetState == HeadState.Turned);

            if (playAnimation)
            {
                PlayTurnAnimation(targetState);
            }
            else
            {
                UpdateHeadVisibility();
            }
        }

        /// <summary>
        /// 播放转向动画
        /// </summary>
        /// <param name="targetState">目标状态</param>
        private void PlayTurnAnimation(HeadState targetState)
        {
            // 播放身体动画
            if (bodyAnimator != null)
            {
                string animationName = targetState == HeadState.Turned ? "TurnHead" : "TurnBackHead";
                bodyAnimator.Play(animationName);
            }

            // 更新头部显示
            UpdateHeadVisibility();
        }

        /// <summary>
        /// 更新头部可见性
        /// </summary>
        private void UpdateHeadVisibility()
        {
            if (normalHead != null)
                normalHead.SetActive(currentHeadState == HeadState.Normal);
            
            if (turnedHead != null)
                turnedHead.SetActive(currentHeadState == HeadState.Turned);
        }

        /// <summary>
        /// 切换到正常头部
        /// </summary>
        public void SetNormalHead()
        {
            SetHeadState(HeadState.Normal);
        }

        /// <summary>
        /// 切换到转向头部
        /// </summary>
        public void SetTurnedHead()
        {
            SetHeadState(HeadState.Turned);
        }

        /// <summary>
        /// 切换头部状态（切换到相反状态）
        /// </summary>
        public void ToggleHeadState()
        {
            HeadState newState = currentHeadState == HeadState.Normal ? HeadState.Turned : HeadState.Normal;
            SetHeadState(newState);
        }

        /// <summary>
        /// 获取当前头部状态
        /// </summary>
        /// <returns>当前头部状态</returns>
        public HeadState GetCurrentHeadState()
        {
            return currentHeadState;
        }

        /// <summary>
        /// 检查头部是否已转向
        /// </summary>
        /// <returns>true表示已转向，false表示正常状态</returns>
        public bool IsHeadTurned()
        {
            return isHeadTurned;
        }
        protected override void InitComponentSystem()
        {
            base.InitComponentSystem();        
        }
    }
}