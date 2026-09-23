using System;
using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.PureMVC;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.GameSceneManager.Component;
using Game.GameRuntime.UI.Component;
using GameFramework.UnityRuntime.UI;
using GameFramework.UnityRuntime.Utility;
using UnityEngine;

namespace Game.GameRuntime.UI.FormLogic.Base
{
    public class BaseUIFormLogic : UIFormLogic
    {
        [SerializeField] protected Canvas canvas;
        [SerializeField] protected ComponentSystemUI componentSystemUI;
        [SerializeField] private bool allowEscapeClose; // 是否允许esc关闭

        protected int curLoadAtlasCount = 0;
        protected int targetAtlasCount = 0;
        public bool AllowEscapeClose => allowEscapeClose;

        protected bool canStartUpdateUI = false; // 是否能开始刷新界面
        protected bool hasUpdateUI = false; // 是否刷新了界面
        public void SetAllowEscapeClose(bool canClose) { 
            allowEscapeClose = canClose; 
            if (allowEscapeClose)
            {
                SubscribeEscapeClose();
            }
            else
            {
                UnsubscribeEscapeClose();
            }
        }
        public Canvas GetCanvas() { return canvas; }

        private void OnValidate()
        {
            componentSystemUI = GetComponent<ComponentSystemUI>() ?? gameObject.AddComponent<ComponentSystemUI>();

            canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                Debug.LogWarning($"canvas引用丢失=>{transform.root}", gameObject);
            }
        }

        /// <summary>
        /// 在OnInit前调用
        /// </summary>
        protected virtual void Awake()
        {
            if (canvas == null)
            {
                return;
            }

            var uiGm = GameManager.GetGMComponent<UIComponentGM>();
            if (uiGm == null || uiGm.UICamera == null)
            {
                // DialogDebug 等未走 GF UI 初始化的场景：Overlay 模式避免 NRE
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.worldCamera = null;
                return;
            }

            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = uiGm.UICamera;
        }

        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);
            componentSystemUI.OnInit();
            LoadAtlas(0);
        }

        protected virtual void LoadAtlas(int targetAtlasCount)
        {
            this.targetAtlasCount = targetAtlasCount;
            canStartUpdateUI = false;
        }

        protected virtual void loadAtlasCallFunc()
        {
            curLoadAtlasCount++;
            if (curLoadAtlasCount > targetAtlasCount) { return; } // 超过加载的图片数量就不用处理了
            if (curLoadAtlasCount == targetAtlasCount)
            {
                // 加载好需要的图集后刷新界面
                canStartUpdateUI = true;
            }
        }

        public virtual void UpdateUI()
        {
            hasUpdateUI = true;
        }

        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            PlayerOpenAudio(); // 打开界面自动播放一个打开界面音效
            hasUpdateUI = false; // 每次打开界面时都设置需要刷新界面
            // 每次打开界面设置为当前界面组的最高级
            var forms = UIForm.UIGroup.GetAllUIForms();
            if (forms != null)
            {
                int maxOrder = UIForm.UIGroup.Depth;
                foreach (var form in forms)
                {
                    if (((UIForm)form).Logic is BaseUIFormLogic baseUIFormLogic)
                    {
                        if (baseUIFormLogic.canvas.sortingOrder > maxOrder)
                        {
                            maxOrder = baseUIFormLogic.canvas.sortingOrder;
                        }
                    }
                }
                
                canvas.sortingOrder = maxOrder + 1;
            }
        }

        protected internal override void OnClose(bool isShutdown, object userData)
        {
            // 0922 背包→地图→再 ESC：OnClose 必须退订 ESC。
            // 原因：仅 OnCover 退订时，CloseUIForm 关菜单/地图不会走 OnCover，CloseFormOnEsc 残留。
            // 池化同步 Open 后同一次 ESC 多播会打到已重新激活的实例 → 刚 Open 又被 Close（菜单「打不开」）。
            UnsubscribeEscapeClose();
            base.OnClose(isShutdown, userData);
            hasUpdateUI = false;
        }

        public virtual void PlayerOpenAudio()
        {
            UIUtils.PlayTapExChangeSfx(this);
        }

        /// <summary>
        /// 在OnOpen后调用
        /// </summary>
        protected virtual void Start()
        {
            
        }

        protected internal override void OnReveal()
        {
            base.OnReveal();

            // esc 事件绑定：先 -= 再 +=，避免重复订阅（池化重开 / 漏 OnClose 退订时）
            if (allowEscapeClose)
            {
                SubscribeEscapeClose();
            }
        }

        protected internal override void OnCover()
        {
            base.OnCover();
            UnsubscribeEscapeClose();
        }

        /// <summary>订阅 ESC 关本界面（幂等）。</summary>
        private void SubscribeEscapeClose()
        {
            var input = GameManager.GetGMComponent<InputComponentGM>();
            if (input == null)
            {
                return;
            }

            input.onEscPressed -= CloseFormOnEsc;
            input.onEscPressed += CloseFormOnEsc;
        }

        /// <summary>退订 ESC 关本界面（幂等）。OnClose / OnCover 必须成对调用。</summary>
        private void UnsubscribeEscapeClose()
        {
            var input = GameManager.GetGMComponent<InputComponentGM>();
            if (input == null)
            {
                return;
            }

            input.onEscPressed -= CloseFormOnEsc;
        }

        protected internal override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(elapseSeconds, realElapseSeconds);

            componentSystemUI.OnUpdate();

            if (canStartUpdateUI && !hasUpdateUI)
            {
                UpdateUI();
            }
        }

        /// <summary>
        /// esc关闭
        /// </summary>
        public virtual void CloseFormOnEsc()
        {
            if (this == null)
            {
                Debug.LogError("============FormLogic Has Be Desotry,But call it???");
                return;
            }
            if (isActiveAndEnabled)
            {
                GameManager.GetGMComponent<UIComponentGM>().CloseUIForm(UIForm);
            }
        }

        /// <summary>
        /// 直接关闭
        /// </summary>
        public void CloseForm()
        {
            if (isActiveAndEnabled)
            {
                GameManager.GetGMComponent<UIComponentGM>().CloseUIForm(UIForm);
            }
        }

        public T GetProxy<T>() where T : BaseFormProxy, new()
        {
            return GameManager.GetGMComponent<MVCComponentGM>().GetProxy<T>();
        }

        /// <summary>
        /// 直接停止
        /// </summary>
        public virtual void OnShutDown()
        {
            CloseForm();
        }

        protected virtual void AllowOpenMenu(bool allow)
        {
            var sceneMgr = GameManager.GetGameSceneManager();
            if (sceneMgr != null)
            {
                var inputMgr = sceneMgr.GetModule<InputComponentGSM>();
                if (inputMgr != null)
                {
                    inputMgr.SetAllowOpenMenu(allow);
                }
            }
        }
    }
}