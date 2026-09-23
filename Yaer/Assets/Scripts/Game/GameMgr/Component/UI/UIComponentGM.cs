using System;
using System.Collections.Generic;
using System.Linq;
using Game.GameMgr.Component.Base;
using Game.Static.Path;
using GameFramework.Event;
using GameFramework.UnityRuntime.Event;
using GameFramework.UnityRuntime.UI;
using GameFramework.UnityRuntime.Utility;
using UnityEngine;

namespace Game.GameMgr.Component.UI
{
    public class UIComponentGM : BaseComponentGM
    {
        [SerializeField] private Camera uiCamera;
        private UIComponent uiComponent;
        private Dictionary<int, CallBackAction> callBackActions = new Dictionary<int, CallBackAction>();

        public Camera UICamera => uiCamera;
        
        public override void OnInit()
        {
            base.OnInit();

            uiCamera = transform.GetComponentInChildren<Camera>();
            if (uiCamera == null)
            {
                Log.Error("UICamera引用丢失");
            }

            uiComponent = GameManager.GetGFComponent<UIComponent>();

            GameManager.GetGFComponent<EventComponent>().Subscribe(OpenUIFormSuccessEventArgs.EventId, OpenUIFormSuccessHandler);
            GameManager.GetGFComponent<EventComponent>().Subscribe(OpenUIFormFailureEventArgs.EventId, OpenUIFormFailureHandler);
        }

        private void OpenUIFormFailureHandler(object sender, GameEventArgs e)
        {
            if (e is OpenUIFormFailureEventArgs args)
            {
                Log.Error(args.UIFormAssetName + "打开界面失败");
            }
        }

        private void OpenUIFormSuccessHandler(object sender, GameEventArgs e)
        {
            if (e is OpenUIFormSuccessEventArgs args && callBackActions.TryGetValue(args.UIForm.SerialId, out var callBackAction))
            {
                callBackAction.action?.Invoke(args.UIForm.Logic);
                callBackActions.Remove(args.UIForm.SerialId);
            }
        }


        /// <summary>
        /// 打开界面
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称</param>
        /// <param name="group">界面所在的组</param>
        /// <param name="args">参数</param>
        /// <returns>返回界面ID</returns>
        public int OpenUIForm(string uiFormAssetName, EUIGroup group, OpenFormArgs args)
        {
            return OpenUIForm(uiFormAssetName, group.ToString(), args);
        }
        
        public int OpenUIForm(string uiFormAssetName, string group, OpenFormArgs args)
        {
            if (args == null)
            {
                Log.Error("OpenFormArgs不能为空");
                return -1;
            }

            args.callBack += logic =>
            {
                // 设置uiCamera
                var canvas = logic.GetComponent<Canvas>();
                if (canvas)
                {
                    canvas.worldCamera = uiCamera;
                }
                else
                {
                    Log.Error(logic.GetType().Name + "Canvas组件丢失");
                }
            };

            int id = args.userData == null ? uiComponent.OpenUIForm(uiFormAssetName, group) : uiComponent.OpenUIForm(uiFormAssetName, group, args.userData);

            if (args.callBack != null)
            {
                callBackActions[id] = new CallBackAction(args.callBack);
            }

            return id;
        }

        /// <summary>
        /// 关闭界面
        /// </summary>
        /// <param name="uiForm">要关闭的界面</param>
        public void CloseUIForm(UIForm uiForm)
        {
            uiComponent.CloseUIForm(uiForm);
        }

        public void CloseUIForm(string uiFormAssetName)
        {
            var uiForm = uiComponent.GetUIForm(uiFormAssetName);
            if (uiForm is null)
            {
                Log.Warning("UIForm不存在");
                return;
            }

            uiComponent.CloseUIForm(uiForm);
        }

        /// <summary>
        /// 幂等打开左下角版本号。0924：仅主菜单调用；进局后随 CloseAll 关掉，不再补开。
        /// </summary>
        public void EnsureVersionForm()
        {
            string path = UIPrefabPath.VersionPanel;
            if (GetUIForm(path) != null)
            {
                return;
            }

            OpenUIForm(path, EUIGroup.System, new OpenFormArgs());
        }

        /// <summary>
        /// 关闭所有界面
        /// </summary>
        /// <param name="filter">过滤不关闭的界面（如换场黑幕）</param>
        /// <remarks>
        /// 0924：版本号仅主界面显示——CloseAll <strong>不再</strong>跳过/末尾 Ensure VersionPanel；
        /// 进局、换场、读档清屏后左下不应再有版本号。回主菜单由 <c>OpenMainMenu</c>→Ensure 再开。
        /// 替代（否决）：0922 常驻保活；M2 进局 SetActive(false)。
        /// </remarks>
        public void CloseAllUIForm(params UIForm[] filter)
        {
           var forms =  uiComponent.GetAllLoadedUIForms();
           foreach (var form in forms)
           {
               if (filter != null && filter.Contains(form))
               {
                   continue;
               }

               uiComponent.CloseUIForm(form);
           }
        }

        /// <summary>
        /// 获取界面
        /// </summary>
        public UIForm GetUIForm(string uiFormAssetName)
        {
            return uiComponent.GetUIForm(uiFormAssetName);
        }


        #region 回调封装

        private class CallBackAction
        {
            public readonly Action<UIFormLogic> action;

            public CallBackAction(Action<UIFormLogic> action)
            {
                this.action = action;
            }
        }

        #endregion
    }
}