using GameFramework.UnityRuntime.Entity;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Effect
{
    public class AnimaEffectComponent : EntityLogic, IAnimaEffectComponent
    {
        [SerializeField] protected SpriteRenderer sr;
        [SerializeField] protected Animator animator;
        private bool up;
        private int times;
        private bool isFollowSr;
        private bool setTimes;
        private int targetTimes;

        private AnimatorStateInfo stateInfo;
        private SpriteRenderer followSr;
        public GameObject GameObject => gameObject;

        protected virtual void Awake()
        {
            Find();
            animator.speed = 0;
        }

        protected virtual void Update()
        {
            if (isFollowSr)
            {
                sr.sortingLayerName = followSr.sortingLayerName;
                if (up)
                    sr.sortingOrder = followSr.sortingOrder + 1;
                else
                    sr.sortingOrder = followSr.sortingOrder - 1;
            }

            if (setTimes)
            {
                stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if (stateInfo.normalizedTime >= targetTimes + 1) Debug.LogWarning(name + "没有设置次数结束动画事件");
            }
        }

        private void OnValidate()
        {
            Find();
        }


        public virtual void Play(int times)
        {
            if (times <= 0)
            {
                Debug.LogError("播放次数不能小于等于0");
                return;
            }

            targetTimes = times;
            setTimes = true;
            Play();
        }

        public virtual void Play()
        {
            animator.speed = 1;
        }

        public void FollowSrSortLayer(SpriteRenderer sr, bool up)
        {
            isFollowSr = true;
            followSr = sr;
            this.up = up;
            sr.sortingLayerName = followSr.sortingLayerName;
            sr.sortingOrder = followSr.sortingOrder;
        }

        protected virtual void Find()
        {
            animator = GetComponent<Animator>();
            sr = GetComponent<SpriteRenderer>();
        }

        public void SetSrSortLayer(string layerName, int order)
        {
            sr.sortingLayerName = layerName;
            sr.sortingOrder = order;
        }

        private void AnimaEventTimes()
        {
            times++;

            if (setTimes && times >= targetTimes)
            {
                sr.color = Color.clear;
                Destroy(gameObject);
            }
        }

        public virtual void onAnimaEventTimesRun()
        {

        }

        public void SetRight()
        {
            throw new System.NotImplementedException();
        }

        public void SetLeft()
        {
            throw new System.NotImplementedException();
        }
    }
}