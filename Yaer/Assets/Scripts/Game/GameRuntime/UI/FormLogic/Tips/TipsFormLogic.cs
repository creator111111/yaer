using System;
using System.Collections;
using System.Collections.Generic;
using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.UI.FormLogic.Base;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic.Tips
{
    public class TipsFormLogic : BaseUIFormLogic
    {
        [SerializeField] private float textSpeed = 1.5f;
        [SerializeField] private float textBossSpeed = 0.5f;
        [SerializeField] private Image imgChar;
        [SerializeField] private Animator animator;
        public GameObject normalImgBg; // 普通提示背景
        public GameObject bossTipsBg; // BOSS提示背景
        public SoundToggleComponent soundSfxCpn;
        
        private Coroutine coroutine;
        private Queue<Sprite> tipsQueue = new Queue<Sprite>();

        private TipsFormProxy proxy;

        float waitSecond = 1f;
        ETipsType tipsType;
        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);

            animator = GetComponent<Animator>();
            proxy = GetProxy<TipsFormProxy>();
            
            var ae = GetComponent<AnimationEventComponent>();
            ae.RegisterEvent("HideEnd", s => CloseForm());
        }

        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            animator.Rebind();
            imgChar.fillAmount = 0;
            if (userData is TipsFormArgs args)
            {
                // 部分类型的标题持续时间不一样
                tipsType = args.type;
                textSpeed = tipsType == ETipsType.Boss ? textBossSpeed: textSpeed;
                UpdateImgBg(tipsType);
                UpdateInfo(proxy.GetTipsSprite(args.info));
            }
        }

        protected internal override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                coroutine = null;
            }
            tipsQueue.Clear();
        }

        public void AddTipsInfo(string info, ETipsType type = ETipsType.Item)
        {
            UpdateImgBg(type);
            UpdateInfo(proxy.GetTipsSprite(info));  
        }

        private void UpdateImgBg(ETipsType type)
        {
            //normalImgBg.SetActive(type != ETipsType.Boss);
            //bossTipsBg.SetActive(type == ETipsType.Boss);
        }

        private void UpdateInfo(Sprite sprite)
        {
            if (sprite != null)
            {
                tipsQueue.Enqueue(sprite);
                if (coroutine == null) coroutine = StartCoroutine(ShowCharCoroutine());
            }
        }

        private IEnumerator ShowCharCoroutine()
        {
            while (tipsQueue.Count > 0)
            {
                // 切换下一条
                imgChar.fillAmount = 0;
                imgChar.sprite = tipsQueue.Dequeue();
                imgChar.SetNativeSize();
                if (tipsType == ETipsType.Item)
                {
                    soundSfxCpn.PlaySound();
                }

                while (imgChar.fillAmount < 1)
                {
                    imgChar.fillAmount += textSpeed * Time.deltaTime;
                    if (imgChar.fillAmount >= 1)
                    {
                        imgChar.fillAmount = 1;
                        break;
                    }

                    yield return null;
                }

                yield return null;
                if (tipsQueue.Count > 0)
                {
                    yield return new WaitForSeconds(waitSecond);
                }
            }
            HideCloseForm();
            StopCoroutine(coroutine);
            coroutine = null;
        }

        private void HideCloseForm()
        {
            animator.SetTrigger("Hide");
        }

        public override void PlayerOpenAudio()
        {

        }
    }
}