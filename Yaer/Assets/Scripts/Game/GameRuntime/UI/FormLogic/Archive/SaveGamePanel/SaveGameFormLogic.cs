using System;
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

namespace Game.GameRuntime.UI.FormLogic.Archive.SaveGamePanel
{
    public class SaveGameFormLogic : BaseUIFormLogic
    {
        public Button btnDelete;
        public Button btnBack;
        public GameObject btnPrefab;
        public Transform contentTransform;
        private ArchiveInfo nowArchiveInfo;
        private List<ArchiveDirectoryInfo> archiveInfosList;
        private List<ButtonArchive> btnList = new List<ButtonArchive>(); // 储存显示的存档按钮

        private bool allowEscClose;
        private string selectedArchiveGuid; // 当前选中的存档id

        private SaveGameFormProxy proxy;

        public GameObject textGameTime;
        public GameObject textSaveDate;
        Dictionary<LanguageEnumType, string> textConfig_1 = new Dictionary<LanguageEnumType, string>() {
            { LanguageEnumType.Chinese, "保存日期:" }, { LanguageEnumType.English, "Save Data:"}, { LanguageEnumType.Japanese, "ほぞんび"},
        };
        Dictionary<LanguageEnumType, string> textConfig_2 = new Dictionary<LanguageEnumType, string>() {
            { LanguageEnumType.Chinese, "游戏时长:" }, { LanguageEnumType.English, "Play Time"}, { LanguageEnumType.Japanese, "プレイ時間"},
        };
        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);

            proxy = GetProxy<SaveGameFormProxy>();
            // 提前初始化系统提示
            GetProxy<SystemTipsFormProxy>();
            
            btnBack.onClick.AddListener(OnClickBtnBack);
            btnDelete.onClick.AddListener(OnClickBtnDelete);
            
            btnPrefab.gameObject.SetActive(false);
        }

        protected internal override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            componentSystemUI.GetComponent<BlackFadeComponent>().HideFade();

            UpdateInfo();

            AllowOpenMenu(false);

            UpdateUI();
        }

        public override void UpdateUI()
        {
            base.UpdateUI();

            var languageTyppe = GameManager.Instance.language;
            var dateText = textConfig_1.ContainsKey(languageTyppe) ? textConfig_1[languageTyppe] : "error";
            GameTools.setText(textSaveDate, dateText);
            var timeText = textConfig_2.ContainsKey(languageTyppe) ? textConfig_2[languageTyppe] : "error";
            GameTools.setText(textGameTime, dateText);
        }

        protected internal override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);
            AllowOpenMenu(true);
        }

        private void OnClickBtnBack()
        {
            UIUtils.PlayBtnAudio(this);
            CloseFormOnEsc();
        }

        private void OnClickBtnDelete()
        {
            UIUtils.PlayBtnAudio(this);
            // 未选择存档或者选择正在使用的存档禁止删除
            if (string.IsNullOrEmpty(selectedArchiveGuid) || nowArchiveInfo.guid == selectedArchiveGuid) return;

            GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(UIPrefabPath.GetUIPrefabPath("SystemTipsPanel"), EUIGroup.Top, new OpenFormArgs()
            {
                userData = ESystemTipsType.Delete,
                callBack = logic =>
                {
                    if (logic is SystemTipsFormLogic systemTipsFormLogic)
                    {
                        systemTipsFormLogic.GetProxy<SystemTipsFormProxy>().onSureEvent = () =>
                        {
                            proxy.DeleteArchive(selectedArchiveGuid);
                            UpdateInfo();
                        };
                        systemTipsFormLogic.GetProxy<SystemTipsFormProxy>().onCancelEvent = SetNowArchiveHighLight;
                    }
                }
            });
        }

        private void UpdateInfo()
        {
            nowArchiveInfo = proxy.GetNowArchiveInfo();
            UpdateArchiveButton(proxy.GetAllArchiveInfos());
            SetNowArchiveHighLight();
        }

        /// <summary>
        ///     根据存档信息创建存档按钮
        /// </summary>
        private void UpdateArchiveButton(List<ArchiveDirectoryInfo> infos)
        {
            // 获取所有存档信息
            archiveInfosList = infos;
            // 清空旧按钮
            for (var i = 0; i < btnList.Count; i++) Destroy(btnList[i].gameObject);

            btnList.Clear();
            for (var i = 0; i < archiveInfosList.Count; i++)
            {
                var button = Instantiate(btnPrefab).GetComponent<ButtonArchive>();
                button.transform.SetParent(contentTransform);
                button.transform.localScale = Vector3.one;
                button.OnInit();
                var info = archiveInfosList[i].info;
                button.UpdateInfo(i + 1, info.guid, info.currentSceneName, info.createTime, info.playTime);

                button.onClickOnce = OnBtnSelected;
                button.onClickDelete = guid =>
                {
                    selectedArchiveGuid = guid;
                    OnClickBtnDelete();
                };
                // 添加双击后的事件
                button.onClickTwice = (btn) =>
                {
                    OnBtnSelected(btn);
                    // 显示提示面板
                    if (selectedArchiveGuid != nowArchiveInfo.guid)
                    {
                        // 保存的存档与选择的存档不一致提示覆盖存档
                        GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(UIPrefabPath.GetUIPrefabPath("SystemTipsPanel"), EUIGroup.Top, new OpenFormArgs()
                        {
                            userData = ESystemTipsType.Cover,
                            callBack = logic =>
                            {
                                if (logic is SystemTipsFormLogic systemTipsFormLogic)
                                {
                                    systemTipsFormLogic.GetProxy<SystemTipsFormProxy>().onSureEvent = () =>
                                    {
                                        CoverArchive();
                                        // 刷新
                                        UpdateInfo();
                                    };

                                    systemTipsFormLogic.GetProxy<SystemTipsFormProxy>().onCancelEvent = SetNowArchiveHighLight;
                                }
                            }
                        });
                    }
                    else
                    {
                        // 继续存档
                        GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(UIPrefabPath.GetUIPrefabPath("SystemTipsPanel"), EUIGroup.Top, new OpenFormArgs()
                        {
                            userData = ESystemTipsType.Save,
                            callBack = logic =>
                            {
                                if (logic is SystemTipsFormLogic systemTipsFormLogic)
                                {
                                    systemTipsFormLogic.GetProxy<SystemTipsFormProxy>().onSureEvent = () =>
                                    {
                                        SaveOldArchive();
                                        // 刷新
                                        UpdateInfo();
                                    };

                                    systemTipsFormLogic.GetProxy<SystemTipsFormProxy>().onCancelEvent = SetNowArchiveHighLight;
                                }
                            }
                        });
                    }
                };
                btnList.Add(button);
                // 默认当前使用的存档为高亮
                SetNowArchiveHighLight();

                button.gameObject.SetActive(true);
            }

            // 创建多余的无存档信息的按钮
            for (var i = 0; i < 20 - archiveInfosList.Count; i++)
            {
                var button = Instantiate(btnPrefab).GetComponent<ButtonArchive>();
                button.transform.SetParent(contentTransform);
                button.transform.localScale = Vector3.one;
                button.OnInit();
                button.UpdateInfo(archiveInfosList.Count + 1 + i, "");

                // 点击空白存档按钮储存新存档
                button.onClickOnce += (btn) =>
                {
                    // 高亮选中
                    OnBtnSelected(btn);
                    // 打开提示
                    GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(UIPrefabPath.GetUIPrefabPath("SystemTipsPanel"), EUIGroup.Top, new OpenFormArgs()
                    {
                        userData = ESystemTipsType.Save,
                        callBack = logic =>
                        {
                            if (logic is SystemTipsFormLogic systemTipsFormLogic)
                            {
                                systemTipsFormLogic.GetProxy<SystemTipsFormProxy>().onSureEvent = () =>
                                {
                                    SaveNewArchive();
                                    // 刷新
                                    UpdateInfo();
                                };

                                systemTipsFormLogic.GetProxy<SystemTipsFormProxy>().onCancelEvent = SetNowArchiveHighLight;
                            }
                        }
                    });
                };

                btnList.Add(button);
                button.gameObject.SetActive(true);
            }
        }

        /// <summary>
        ///     按钮选中消息回调
        /// </summary>
        private void OnBtnSelected(ButtonArchive button)
        {
            UIUtils.PlayTapExChangeSfx(this);
            // 记录选中的存档ID
            if (!string.IsNullOrEmpty(button.Guid))
            {
                selectedArchiveGuid = button.Guid;
            }

            for (var i = 0; i < btnList.Count; i++)
            {
                // 未选中的不高亮
                if (btnList[i] != button)
                {
                    btnList[i].IsSelect = false;
                }
            }
            
            SetNowArchiveHighLight();
        }

        /// <summary>
        /// 设置当前使用的存档为高亮
        /// </summary>
        private void SetNowArchiveHighLight()
        {
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

        /// <summary>
        /// 新建存档
        /// </summary>
        private void SaveNewArchive()
        {
            proxy.SaveNewArchive();
        }

        /// <summary>
        /// 旧存档继续保存
        /// </summary>
        private void SaveOldArchive()
        {
            proxy.SaveOldArchive();
        }

        /// <summary>
        /// 覆盖存档
        /// </summary>
        private void CoverArchive()
        {
            proxy.CoverArchive(selectedArchiveGuid);
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