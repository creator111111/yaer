using DG.Tweening;
using Game.GameRuntime.Entities.Player;
using Game.Static.Name.Clothes;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Game.GameRuntime.UI.FormLogic.Fighting
{
    public class FightingIllustration : MonoBehaviour
    {
        public enum IllustrationState
        {
            Normal,
            Damaged,
            DamagedAndWounded
        }

        [SerializeField] private Sprite[] Parts;
        [SerializeField] private Image Body;
        [SerializeField] private Image Face;
        [SerializeField] private Image Part;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float MinBlinkTime;
        [SerializeField] private float MaxBlinkTime;
        [SerializeField] private CanvasScaler CanvasScaler;
        private Animator Animator;
        private new Camera camera;

        private float mul;
        private Rect rec;
        private bool isIn = false;

        private PlayerLogic playerLogic;
        private IllustrationState State = IllustrationState.Normal;

        private bool IllustrationShowSign = false;

        private void Awake()
        {
            Animator = GetComponent<Animator>();
            Body = transform.Find("Body").GetComponent<Image>();
            Face = transform.Find("Face").GetComponent<Image>();
            Part = transform.Find("Part").GetComponent<Image>();
        }

        private void Start()
        {
            camera = Camera.main;
        }

        private void Update()
        {
            CheckPlayerPosition();
            RandomBlink();
        }

        private void OnEnable()
        {
            mul = Screen.width / CanvasScaler.referenceResolution.x;
            rec = GetWorldRect(transform as RectTransform);
        }

        private void SetEffect()
        {
            
        }

        private Rect GetWorldRect(RectTransform rectTransform)
        {
            var corners = new Vector3[4];
            rectTransform.GetLocalCorners(corners);
            var width = Mathf.Abs(Vector2.Distance(corners[0], corners[3])) * mul;
            var height = Mathf.Abs(Vector2.Distance(corners[0], corners[1])) * mul;
            var s = CanvasScaler.referenceResolution;
            var a = transform.localPosition + corners[0] + new Vector3(s.x / 2, s.y / 2);
            return new Rect(a * mul, new Vector2(width, height));
        }

        private void GetPlayer()
        {
            if (!playerLogic)
            {
                playerLogic = FindObjectOfType<PlayerLogic>();
            }
        }

        private void CheckPlayerPosition()
        {
            GetPlayer();
            if (!playerLogic) return;

            if (!camera)
            {
                camera = Camera.main;
                return;
            }

            Vector2 playerScreenPoint = camera.WorldToScreenPoint(playerLogic.transform.position);

            bool currentIsIn = rec.Contains(playerScreenPoint);

            if (currentIsIn && !isIn)
            {
                SetAlpha(0);
            }
            else if (!currentIsIn && isIn)
            {
                SetAlpha(1f);
            }

            isIn = currentIsIn;
        }

        /// <summary>
        ///     part 0:无，1:王冠，2:护头
        /// </summary>
        /// <param name="state"></param>
        /// <param name="part"></param>
        public void Initialize(IllustrationState state, string headWear)
        {
            SetState(State);
            Part.gameObject.SetActive(true);
            switch (headWear)
            {
                case ClothesName.HeadWear.NoHeadWear:
                    Part.gameObject.SetActive(false);
                    break;
                case ClothesName.HeadWear.Crown:
                    Part.sprite = Parts[0];
                    break;
                case ClothesName.HeadWear.ArmorHead:
                    Part.sprite = Parts[1];
                    break;
            }
        }

        private float BlinkCD;
        private void RandomBlink()
        {
            if (gameObject.activeInHierarchy && State == IllustrationState.Normal)
            {
                BlinkCD -= Time.deltaTime;
                if (BlinkCD < 0)
                {
                    Animator.SetTrigger("Blink");
                    BlinkCD = Random.Range(MinBlinkTime, MaxBlinkTime);
                }
            }
        }

        public void SetState(IllustrationState state)
        {
            State = state;
            switch (State)
            {
                 case IllustrationState.Normal:
                     Animator.SetTrigger("Normal");
                     break;
                 case IllustrationState.Damaged:
                     Animator.SetTrigger("Damaged");
                     break;
                 case IllustrationState.DamagedAndWounded:
                     Animator.SetTrigger("DamagedAndWounded");
                     break;
                 default:
                    Debug.LogError("未知状态！");
                     break;
            }
        }

        public void Attacked(bool attacked)
        {
            if (attacked)
            {
                Animator.SetTrigger("Attacked");
            }
            //SetFace();
        }
        public void SetAlpha(float alpha)
        {
            canvasGroup.DOKill();
            canvasGroup.DOFade(alpha, 0.5f);
        }
    }
}