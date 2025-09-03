using System;
using System.Collections;
using System.Collections.Generic;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.UI.Component.BlackFade;
using Game.GameRuntime.UI.FormLogic.Base;
using UnityEngine;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic.Loading
{
    /// <summary>
    /// 打开时需要传入加载完成后的回调
    /// </summary>
    public class LoadingFormLogic: BaseUIFormLogic
    {
        [SerializeField]
        private GameObject MainGO;
        [SerializeField]
        private RectTransform PicturesRtf;
        [SerializeField]
        private RectTransform TipsRtf;
        [SerializeField]
        private Slider ProgressSlider;

        private BlackFadeComponent blackFade;

        private List<GameObject> PicturesGos;
        private List<GameObject> TipsGos;

        private Coroutine progressCoroutine = null;

        private Action OnFinishLoading;

        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);
            PicturesGos = GetChildsToList(PicturesRtf);
            TipsGos = GetChildsToList(TipsRtf);
            blackFade = componentSystemUI.GetComponent<BlackFadeComponent>();

        }

        private List<GameObject> GetChildsToList(RectTransform rtf)
        {
            var list = new List<GameObject>(rtf.childCount);
            for (int i = 0; i < rtf.childCount; i++)
            {
                list.Add(rtf.GetChild(i).gameObject);
            }
            return list;
        }

        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            MainGO.SetActive(true);
            blackFade.HideFade();
            OnFinishLoading = userData as Action;
            foreach (var go in PicturesGos)
            {
                go.SetActive(false);
            }
            foreach (var go in TipsGos)
            {
                go.SetActive(false);
            }
            int rd = UnityEngine.Random.Range(0, PicturesGos.Count);
            PicturesGos[rd].gameObject.SetActive(true);
            rd = UnityEngine.Random.Range(0, TipsGos.Count);
            TipsGos[rd].gameObject.SetActive(true);

            if (progressCoroutine != null)
            {
                StopCoroutine(progressCoroutine);
                progressCoroutine = null;
            }

            float progressTotalTime = UnityEngine.Random.Range(2f, 3f);
            progressCoroutine = StartCoroutine(Progress(progressTotalTime));
        }

        private IEnumerator Progress(float totalTime)
        {
            float time = 0;
            ProgressSlider.value = time / totalTime;

            int pause = 2;
            List<float> pauseMoments = new List<float>(pause);
            for (int i=0; i<pause; i++)
            {
                pauseMoments.Add(UnityEngine.Random.Range(0f, 1f));
            }
            pauseMoments.Sort();

            for (int i = 0; i < pause; i++)
            {
                while (time < totalTime)
                {
                    yield return null;
                    time += Time.deltaTime;
                    float percent = time / totalTime;
                    ProgressSlider.value = percent;
                    if (percent >= pauseMoments[i]) break;
                }
                yield return new WaitForSeconds(0.5f);
            }

            while (time < totalTime)
            {
                yield return null;
                time += Time.deltaTime;
                float percent = time / totalTime;
                ProgressSlider.value = percent;
            }

            blackFade.ShowFade(() =>
            {
                OnSliderEnd();
                MainGO.SetActive(false);
                blackFade.CloseFormHideFade(this.UIForm);
            });
        }

        private void OnSliderEnd()
        {
            OnFinishLoading?.Invoke();
            OnFinishLoading = null;
        }

        public override void PlayerOpenAudio()
        {

        }
    }
}