using System.Collections.Generic;
using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive;
using Game.GameMgr.Component.Archive.ArchiveDataClass;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.GameSceneManager.Component;
using Game.GameRuntime.UI.Component.BlackFade;
using Game.GameRuntime.UI.FormLogic.Archive.Control;
using Game.GameRuntime.UI.FormLogic.Base;
using Game.GameRuntime.UI.FormLogic.SystemTips;
using Game.Static.Path;
using UnityEngine;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic.Archive.LoadGamePanel
{
    public class LoadGameFormLogic : BaseUIFormLogic
    {
        [SerializeField] private Button btnDelete;
        [SerializeField] private Button btnBack;
        [SerializeField] private Transform contentTransform;
        [SerializeField] private GameObject archiveBtnPrefab;

        private string selectedArchiveGuid; // 当前选中的存档guid

        private ArchiveInfo nowArchiveInfo;
        private List<ButtonArchive> btnList = new List<ButtonArchive>(); // 储存显示的存档按钮

        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);
            
            // 获取存档数据
            GetProxy<LoadGameFormProxy>();
            // 提前初始化系统提示
            GetProxy<SystemTipsFormProxy>();
            

            btnDelete.onClick.AddListener(OnBtnDelete);
            btnBack.onClick.AddListener(OnBtnBack);
            
            archiveBtnPrefab.gameObject.SetActive(false);
        }

        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            componentSystemUI.GetComponent<BlackFadeComponent>().HideFade();
            
            UpdateInfo();

            AllowOpenMenu(false);
        }

        protected internal override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);

            AllowOpenMenu(true);
        }

        private void OnBtnDelete()
        {
            UIUtils.PlayBtnAudio(this);
            // 未选择存档或者选择正在使用的存档禁止删除
            if (string.IsNullOrEmpty(selectedArchiveGuid) || nowArchiveInfo != null && nowArchiveInfo.guid == selectedArchiveGuid) return;
                
            GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(UIPrefabPath.GetUIPrefabPath("SystemTipsPanel"), EUIGroup.Top, new OpenFormArgs()
            {
                userData = ESystemTipsType.Delete,
                callBack = logic =>
                {
                    if (logic is SystemTipsFormLogic systemTipsFormLogic)
                    {
                        systemTipsFormLogic.GetProxy<SystemTipsFormProxy>().onSureEvent = () =>
                        {
                            GetProxy<LoadGameFormProxy>().DeleteArchive(selectedArchiveGuid);
                            UpdateInfo();
                        };

                        systemTipsFormLogic.GetProxy<SystemTipsFormProxy>().onCancelEvent = SetNowArchiveHighLight;
                    }
                }
            });
        }

        private void OnBtnBack()
        {
            componentSystemUI.GetComponent<BlackFadeComponent>().CloseFormShowFade(UIForm);
            UIUtils.PlayBtnAudio(this);
        }

        /// <summary>
        /// 根据存档信息创建存档按钮
        /// </summary>
        private void UpdateArchiveButton(List<ArchiveDirectoryInfo> infos)
        {
            // 清空旧按钮
            for (var i = 0; i < btnList.Count; i++) Destroy(btnList[i].gameObject);

            btnList.Clear();

            // 存档信息创建有存档的按钮
            for (var i = 0; i < infos.Count; i++)
            {
                var button = Instantiate(archiveBtnPrefab).GetComponent<ButtonArchive>();
                button.transform.SetParent(contentTransform);
                button.transform.localScale = Vector3.one;
                button.OnInit();
                button.onClickDelete = guid =>
                {
                    selectedArchiveGuid = guid;
                    OnBtnDelete();
                };
                button.onClickOnce = OnBtnSelect;
                var info = infos[i].info;
                button.UpdateInfo(i + 1, info.guid, info.currentSceneName, info.createTime, info.playTime);

                // 添加双击后的事件
                button.onClickTwice += (btn) =>
                {
                    OnBtnSelect(btn);

                    GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(UIPrefabPath.GetUIPrefabPath("SystemTipsPanel"), UIForm.UIGroup.Name, new OpenFormArgs()
                    {
                        userData = ESystemTipsType.Load,
                        callBack = logic =>
                        {
                            if (logic is SystemTipsFormLogic systemTipsFormLogic)
                            {
                                systemTipsFormLogic.proxy.onSureEvent = () =>
                                {
                                    // 加载存档
                                    GetProxy<LoadGameFormProxy>().LoadArchive(selectedArchiveGuid);
                                };

                                systemTipsFormLogic.proxy.onCancelEvent = SetNowArchiveHighLight;
                            }
                        }
                    });
                };
                button.gameObject.SetActive(true);
                btnList.Add(button);

                // 默认当前使用的存档为高亮
                SetNowArchiveHighLight();
            }

            // 创建多余的无存档信息的按钮
            for (var i = 0; i < 20 - infos.Count; i++)
            {
                var button = Instantiate(archiveBtnPrefab).GetComponent<ButtonArchive>();
                button.transform.SetParent(contentTransform);
                button.transform.localScale = Vector3.one;
                button.OnInit();
                button.onClickOnce = OnBtnSelect;
                button.UpdateInfo(infos.Count + 1 + i, "");
                button.gameObject.SetActive(true);
                btnList.Add(button);
            }
        }

        private void OnBtnSelect(ButtonArchive btn)
        {
            if (!string.IsNullOrEmpty(btn.Guid))
            {
                // 记录选中的存档ID
                selectedArchiveGuid = btn.Guid;
            }

            for (var i = 0; i < btnList.Count; i++)
            {
                if (btnList[i] != btn)
                    // 未选中的不高亮
                    btnList[i].IsSelect = false;
            }
            UIUtils.PlayTapExChangeSfx(this);
            SetNowArchiveHighLight();
        }
        
        /// <summary>
        /// 设置当前使用的存档为高亮
        /// </summary>
        private void SetNowArchiveHighLight()
        {
            if (nowArchiveInfo is null)
            {
                return;
            }
            
            for (var i = 0; i < btnList.Count; i++)
            {
                if (btnList[i].Guid == nowArchiveInfo.guid)
                {
                    btnList[i].SetUsing(true);
                }
                else
                {
                    btnList[i].SetUsing(false);
                }
            }
        }
        
        private void UpdateInfo()
        {
            nowArchiveInfo = GetProxy<LoadGameFormProxy>().GetNowArchiveInfo();
            UpdateArchiveButton( GetProxy<LoadGameFormProxy>().GetAllArchiveInfos());
            SetNowArchiveHighLight();
        }

        public override void CloseFormOnEsc()
        {
            if (componentSystemUI.GetComponent<BlackFadeComponent>().IsBusy)
            {
                return;
            }

            componentSystemUI.GetComponent<BlackFadeComponent>().CloseFormShowFade(UIForm);
        }
    }
}