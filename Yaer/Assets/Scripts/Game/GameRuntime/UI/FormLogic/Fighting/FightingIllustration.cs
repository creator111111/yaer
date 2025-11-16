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
            // 初始化相机
            if (!camera)
            {
                camera = Camera.main;
            }
        }

        private void SetEffect()
        {
            
        }

        private Rect GetScreenRect(RectTransform rectTransform)
        {

            Vector2 screenPointMin = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 screenPointMax = new Vector2(float.MinValue, float.MinValue);
            
            // 获取UI元素的四个角点
            Vector3[] worldCorners = new Vector3[4];
            rectTransform.GetWorldCorners(worldCorners);
            
            // 对于每个角点，转换为屏幕坐标
            for (int i = 0; i < 4; i++)
            {
                // 使用Canvas的相机（如果有）或主相机进行转换
                Camera uiCamera = rectTransform.GetComponentInParent<Canvas>().worldCamera;
                if (uiCamera == null)
                {
                    uiCamera = camera; // 使用主相机
                }
                
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, worldCorners[i]);
                
                // 更新最小和最大屏幕坐标
                screenPointMin.x = Mathf.Min(screenPointMin.x, screenPoint.x);
                screenPointMin.y = Mathf.Min(screenPointMin.y, screenPoint.y);
                screenPointMax.x = Mathf.Max(screenPointMax.x, screenPoint.x);
                screenPointMax.y = Mathf.Max(screenPointMax.y, screenPoint.y);
            }
            
            // 创建屏幕空间中的矩形
            float width = screenPointMax.x - screenPointMin.x;
            float height = screenPointMax.y - screenPointMin.y;
            
            // 计算中心位置
            float centerX = screenPointMin.x + width / 2;
            float centerY = screenPointMin.y + height / 2;
            
            // 将矩形扩大10%
            float scaleFactor = 1.2f; // 扩大10%
            float newWidth = width * scaleFactor;
            float newHeight = height * scaleFactor;
            
            // 基于中心位置重新计算矩形
            return new Rect(centerX - newWidth / 2, 
                           centerY - newHeight / 2, 
                           newWidth, 
                           newHeight);
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
            rec = GetScreenRect(transform as RectTransform);
            Vector2 playerScreenPoint = camera.WorldToScreenPoint(playerLogic.transform.position);
            bool currentIsIn = rec.Contains(playerScreenPoint);
            if (currentIsIn)
            {
                // 确保矩形有合理的尺寸
                if (rec.width > 0 && rec.height > 0)
                {
                    if (!isIn)
                    {
                        // 玩家进入UI区域，隐藏UI
                        SetAlpha(0);
                    }
                }
            }
            else if (isIn)
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