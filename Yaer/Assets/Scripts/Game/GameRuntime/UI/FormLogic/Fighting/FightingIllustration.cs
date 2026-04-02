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
        [Tooltip("屏幕像素：在立绘矩形基础上四边外扩，用于更早触发淡出，减轻与角色重叠")]
        [SerializeField] private float hideOverlapPaddingPixels = 48f;

        private Animator Animator;
        private new Camera camera;

        private Rect rec;
        private bool isIn = false;

        private Canvas rootCanvas;

        private PlayerLogic playerLogic;
        private IllustrationState State = IllustrationState.Normal;

        private bool IllustrationShowSign = false;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        [SerializeField] private bool showRuntimeTuner;
        [SerializeField] private KeyCode runtimeTunerToggleKey = KeyCode.F9;
        [SerializeField] private float runtimeTunerPaddingMax = 300f;
        [SerializeField] private bool drawHideZoneDebugRect;
        private static Texture2D s_DebugWhite;
#endif

        private void Awake()
        {
            Animator = GetComponent<Animator>();
            Body = transform.Find("Body").GetComponent<Image>();
            Face = transform.Find("Face").GetComponent<Image>();
            Part = transform.Find("Part").GetComponent<Image>();
            rootCanvas = GetComponentInParent<Canvas>();
        }

        private void Start()
        {
            camera = Camera.main;
        }

        private void Update()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (Input.GetKeyDown(runtimeTunerToggleKey))
            {
                showRuntimeTuner = !showRuntimeTuner;
            }
#endif
            CheckPlayerPosition();
            RandomBlink();
        }

        private void OnEnable()
        {
            isIn = false;
        }

        private void CacheCanvas()
        {
            if (rootCanvas == null)
            {
                rootCanvas = GetComponentInParent<Canvas>();
            }
        }

        /// <summary>
        /// 用立绘 RectTransform 四角投影到屏幕，再外扩 padding；每帧调用以便 Play 中改 padding / 布局变化立即生效。
        /// </summary>
        private void RefreshHideRect()
        {
            CacheCanvas();
            var rt = transform as RectTransform;
            if (rt == null)
            {
                return;
            }

            if (rootCanvas == null)
            {
                FallbackRecFromLegacyMath(rt);
                return;
            }

            var corners = new Vector3[4];
            rt.GetWorldCorners(corners);
            var uiCam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera;

            var minX = float.MaxValue;
            var minY = float.MaxValue;
            var maxX = float.MinValue;
            var maxY = float.MinValue;
            for (var i = 0; i < 4; i++)
            {
                var sp = RectTransformUtility.WorldToScreenPoint(uiCam, corners[i]);
                if (sp.x < minX) minX = sp.x;
                if (sp.x > maxX) maxX = sp.x;
                if (sp.y < minY) minY = sp.y;
                if (sp.y > maxY) maxY = sp.y;
            }

            var baseRect = Rect.MinMaxRect(minX, minY, maxX, maxY);
            rec = ExpandRectScreenPixels(baseRect, hideOverlapPaddingPixels);
        }

        private void FallbackRecFromLegacyMath(RectTransform rectTransform)
        {
            if (CanvasScaler == null)
            {
                return;
            }

            var mul = Screen.width / CanvasScaler.referenceResolution.x;
            var lc = new Vector3[4];
            rectTransform.GetLocalCorners(lc);
            var width = Mathf.Abs(Vector2.Distance(lc[0], lc[3])) * mul;
            var height = Mathf.Abs(Vector2.Distance(lc[0], lc[1])) * mul;
            var s = CanvasScaler.referenceResolution;
            var a = transform.localPosition + lc[0] + new Vector3(s.x / 2, s.y / 2);
            var legacy = new Rect(a * mul, new Vector2(width, height));
            rec = ExpandRectScreenPixels(legacy, hideOverlapPaddingPixels);
        }

        private static Rect ExpandRectScreenPixels(Rect r, float padding)
        {
            if (padding <= 0f) return r;
            return new Rect(r.xMin - padding, r.yMin - padding, r.width + 2f * padding, r.height + 2f * padding);
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
            RefreshHideRect();

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

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnGUI()
        {
            if (!showRuntimeTuner && !drawHideZoneDebugRect)
            {
                return;
            }

            if (s_DebugWhite == null)
            {
                s_DebugWhite = Texture2D.whiteTexture;
            }

            if (showRuntimeTuner)
            {
                GUILayout.BeginArea(new Rect(10, 10, 320, 120), GUI.skin.box);
                GUILayout.Label("FightingIllustration Hide Padding (F9 toggle)");
                hideOverlapPaddingPixels = GUILayout.HorizontalSlider(
                    hideOverlapPaddingPixels,
                    0f,
                    runtimeTunerPaddingMax,
                    GUILayout.Width(280));
                GUILayout.Label($"hideOverlapPaddingPixels: {hideOverlapPaddingPixels:F1}");
                drawHideZoneDebugRect = GUILayout.Toggle(drawHideZoneDebugRect, "Draw hide zone (screen)");
                GUILayout.EndArea();
            }

            if (drawHideZoneDebugRect && Event.current.type == EventType.Repaint)
            {
                var c = new Color(1f, 1f, 0f, 0.25f);
                DrawScreenRectFilled(rec, c);
                DrawScreenRectBorder(rec, new Color(1f, 0.92f, 0.016f, 0.9f), 2f);
            }
        }

        private static void DrawScreenRectFilled(Rect screenRectBottomLeft, Color color)
        {
            var gui = ScreenToGuiRect(screenRectBottomLeft);
            var prev = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(gui, s_DebugWhite, ScaleMode.StretchToFill);
            GUI.color = prev;
        }

        private static void DrawScreenRectBorder(Rect screenRectBottomLeft, Color color, float thickness)
        {
            var g = ScreenToGuiRect(screenRectBottomLeft);
            var prev = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(new Rect(g.xMin, g.yMin, g.width, thickness), s_DebugWhite, ScaleMode.StretchToFill);
            GUI.DrawTexture(new Rect(g.xMin, g.yMax - thickness, g.width, thickness), s_DebugWhite, ScaleMode.StretchToFill);
            GUI.DrawTexture(new Rect(g.xMin, g.yMin, thickness, g.height), s_DebugWhite, ScaleMode.StretchToFill);
            GUI.DrawTexture(new Rect(g.xMax - thickness, g.yMin, thickness, g.height), s_DebugWhite, ScaleMode.StretchToFill);
            GUI.color = prev;
        }

        private static Rect ScreenToGuiRect(Rect screenRectBottomLeft)
        {
            return new Rect(
                screenRectBottomLeft.x,
                Screen.height - screenRectBottomLeft.yMax,
                screenRectBottomLeft.width,
                screenRectBottomLeft.height);
        }
#endif

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
