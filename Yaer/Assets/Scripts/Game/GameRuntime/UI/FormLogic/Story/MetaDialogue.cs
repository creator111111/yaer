using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.GameMgr;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic.Story
{
    public class MetaDialogue : MonoBehaviour
    {
        [SerializeField]
        private List<GameObject> Dialogues = new List<GameObject>();
        [SerializeField]
        private List<GameObject> Dialogues_en = new List<GameObject>();
        [SerializeField]
        private List<GameObject> Dialogues_jp = new List<GameObject>();

        private int currentIndex;

        public GameObject imgBg;

        private void Start()
        {
            imgBg.SetActive(false);
            foreach (var dialogue in Dialogues)
            {
                dialogue.GetComponent<CanvasGroup>().DOKill();
                dialogue.GetComponent<CanvasGroup>().alpha = 0;
                dialogue.SetActive(false);
            }
            foreach (var dialogue in Dialogues_en)
            {
                dialogue.GetComponent<CanvasGroup>().DOKill();
                dialogue.GetComponent<CanvasGroup>().alpha = 0;
                dialogue.SetActive(false);
            }
            foreach (var dialogue in Dialogues_jp)
            {
                dialogue.GetComponent<CanvasGroup>().DOKill();
                dialogue.GetComponent<CanvasGroup>().alpha = 0;
                dialogue.SetActive(false);
            }
        }

        private GameObject CurrentShowGraphic
        {
            get
            {
                Dictionary<LanguageEnumType, List<GameObject>> dictDatas = new Dictionary<LanguageEnumType, List<GameObject>>() {
                    { LanguageEnumType.Chinese, Dialogues }, { LanguageEnumType.English, Dialogues_en }, {LanguageEnumType.Japanese, Dialogues_jp },
                };
                var curLanguage = GameManager.Instance.language;
                List<GameObject> myDialogues;
                if (dictDatas.ContainsKey(curLanguage))
                {
                    myDialogues = dictDatas[curLanguage];
                }
                else
                {
                    myDialogues = dictDatas[LanguageEnumType.Chinese];
                }
                
                if (myDialogues == null || currentIndex < 0 || currentIndex >= myDialogues.Count) return null;
                return myDialogues[currentIndex];
            }
        }

        private Sequence currentSeq;

        private void OnValidate()
        {
            //Dialogues = new List<Image>();
            //for (int i = 0; i < transform.childCount; i++) 
            //{
            //    var graphic = transform.GetChild(i).GetComponent<UnityEngine.UI.Graphic>();
            //    if (graphic != null)
            //    {
            //        Dialogues.Add(graphic);
            //    }
            //}
        }

        public void Reset()
        {
            foreach (var dialogue in Dialogues)
            {
                dialogue.GetComponent<CanvasGroup>().DOKill();
                dialogue.GetComponent<CanvasGroup>().alpha = 0;
                dialogue.SetActive(false);
            }
            foreach (var dialogue in Dialogues_en)
            {
                dialogue.GetComponent<CanvasGroup>().DOKill();
                dialogue.GetComponent<CanvasGroup>().alpha = 0;
                dialogue.SetActive(false);
            }
            foreach (var dialogue in Dialogues_jp)
            {
                dialogue.GetComponent<CanvasGroup>().DOKill();
                dialogue.GetComponent<CanvasGroup>().alpha = 0;
                dialogue.SetActive(false);
            }
            currentIndex = -1;
        }

        public void Next(float fadeTime, float stayTime)
        {
            imgBg.SetActive(true);
            DOTween.Kill(imgBg);
            imgBg.GetComponent<CanvasGroup>().alpha = 0f;
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }
            if (currentSeq != null)
            {
                currentSeq.Kill();
            }
            if (CurrentShowGraphic != null)
            {
                //CurrentShowGraphic.GetComponent<CanvasGroup>().alpha = 0;
                GameActionMgr.runFadeAction(CurrentShowGraphic, 0, fadeTime);
            }
            currentIndex++;
            if (CurrentShowGraphic != null)
            {
                CurrentShowGraphic.SetActive(true);
                //GameActionMgr.runFadeAction(CurrentShowGraphic, 1, fadeTime);
                currentSeq = DOTween.Sequence();
                List<Tween> tweens = new List<Tween>() {
                    GameActionMgr.runFadeAction(CurrentShowGraphic, 1, fadeTime),
                    GameActionMgr.runFadeAction(CurrentShowGraphic, 0, fadeTime, stayTime),
                };
                currentSeq = GameActionMgr.runSequenceAction(CurrentShowGraphic, tweens);
                List<Tween> tweens2 = new List<Tween>() {
                    GameActionMgr.runFadeAction(imgBg, 1, fadeTime),
                    GameActionMgr.runFadeAction(imgBg, 0, fadeTime, stayTime),
                };
                GameActionMgr.runSequenceAction(imgBg, tweens2);
                //currentSeq.Append(CurrentShowGraphic.GetComponent<CanvasGroup>().DOFade(1, fadeTime));
                //currentSeq.AppendInterval(stayTime);
                //currentSeq.Append(CurrentShowGraphic.GetComponent<CanvasGroup>().DOFade(0, fadeTime));
            }
        }
    }
}