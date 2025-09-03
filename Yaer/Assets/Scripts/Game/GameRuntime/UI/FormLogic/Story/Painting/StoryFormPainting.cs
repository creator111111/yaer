using DG.Tweening;
using Game.GameRuntime.Story.NodeCanvasExtend;
using NodeCanvas.DialogueTrees;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic.Story.Base
{
    public class StoryFormPainting : MonoBehaviour
    {
        [SerializeField] private Image defaultFace;
        [SerializeField] private Transform clothes;
        [SerializeField] private Transform faces;
        private Dictionary<string, GameObject> facesDic = new Dictionary<string, GameObject>();
        private Dictionary<string, Transform> clothesDic = new Dictionary<string, Transform>();

        protected virtual void Awake()
        {
            faces = transform.Find("Faces").transform;

            // get all faces obj, 0 is self
            for (var i = 0; i < faces.childCount; i++)
            {
                var face = faces.GetChild(i);
                facesDic.Add(face.name, face.gameObject);
                face.gameObject.SetActive(defaultFace == null ? false : face.name == defaultFace.name);
            }
            
            // get all clothes obj
            for (var i = 0; i < clothes.childCount; i++) clothesDic.Add(clothes.GetChild(i).name, clothes.GetChild(i));
        }

        private void Start()
        {
            var dialogueActor = GetComponent<DialogueActorEx>();
            if (dialogueActor == null)
            {
                dialogueActor = transform.parent.GetComponent<DialogueActorEx>();
            }
            if (dialogueActor != null)
            {
                RegisterRefreshAvatarEvent(dialogueActor);
            }

            SetDefaultPainting();
        }

        protected virtual void SetDefaultPainting()
        {

        }

        protected virtual void RegisterRefreshAvatarEvent(DialogueActorEx dialogueActor)
        {
            dialogueActor.OnRefreshAvatarEvent += (roleName, faceType, sprite) =>
            {
                UpdateFace(faceType.ToString());
            };
        }

        public virtual void UpdateFace(string faceName)
        {
            foreach (var item in facesDic) item.Value.SetActive(false);
            if (facesDic.ContainsKey(faceName))
            {
                facesDic[faceName].SetActive(true);
            }
        }

        public virtual void UpdateClothes(string clothesName)
        {
            foreach (var p in clothesDic) p.Value.gameObject.SetActive(false);
            
            if (clothesDic.ContainsKey(clothesName)) clothesDic[clothesName].gameObject.SetActive(true);
        }

        public void Fade(float endValue=0, float duration=0.7f)
        {
            if (this.TryGetComponent<CanvasGroup>(out var canvasGroup))
            {
                canvasGroup.DOFade(endValue, duration);
            }
        }
    }
}