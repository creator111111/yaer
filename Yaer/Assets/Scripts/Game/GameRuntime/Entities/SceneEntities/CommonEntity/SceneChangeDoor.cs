using Game.GameMgr.Component.UI;
using Game.GameMgr;
using Game.GameRuntime.Entities.Base;
using Game.GameRuntime.Entities.Base.BaseSceneObj;
using Game.GameRuntime.Entities.Component.Interactive;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.GameSceneManager.Component;
using Game.Static.Path;
using GameFramework.UnityRuntimeExtend.Component;
using System;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Map
{
    [RequireComponent(typeof(SceneEntity), typeof(ComponentSystemMono))]
    public class SceneChangeDoor : BaseSceneEntityLogic
    {
        [SerializeField] protected Transform bornPos;
        [SerializeField] protected string NextSceneName;
        [SerializeField] protected bool TriggerWhenMoveIn = false;
        [SerializeField] protected bool ShowLoadingUI = false;

        public Transform BornPos => bornPos;
        protected InteractiveComponent interactiveComponent;

        private bool isEnter;
        public Func<bool> CheckNextSceneUnlock = null;

        protected internal override void OnInit(object userData)
        {
            if (enabled)
            {
                base.OnInit(userData);
                interactiveComponent = componentSystem.GetComponent<InteractiveComponent>();

                interactiveComponent.onClickInteractiveEvent += EnterDoor;
                if (TriggerWhenMoveIn)
                {
                    interactiveComponent.onEnterInteractiveEvent += (component) =>
                    {
                        if (component.Entity?.Logic is PlayerLogic playuerLogic)
                        {
                            if (!playuerLogic.isDead)
                            {
                                EnterDoor(component);
                            }
                        }
                    };
                }
            }
        }

        protected virtual void OnEnterSuccess()
        {

        }

        protected virtual void OnEnterFail()
        {

        }

        protected virtual void EnterDoor(InteractiveComponent component)
        {
            if (string.IsNullOrEmpty(NextSceneName))
            {
                Debug.LogError($"δ������һ������������");
            }
            else
            {
                if (CheckNextSceneUnlock == null || CheckNextSceneUnlock())
                {
                    if (isEnter)
                    {
                        Debug.LogWarning($"SceneChangeDoor�ѽ����");
                    }
                    else
                    {
                        isEnter = true;
                        OnEnterSuccess();
                        if (ShowLoadingUI)
                        {
                            GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(UIPrefabPath.GetUIPrefabPath("LoadingPanel"), EUIGroup.Top, new OpenFormArgs()
                            {
                                userData = new Action(() =>
                                {
                                    
                                }),
                                callBack = (uiFormLogic) =>
                                {
                                    SceneManager.GetModule<LoadSceneComponentGSM>().LoadScene(NextSceneName, null, false);
                                }
                            });
                            
                        }
                        else
                        {
                            SceneManager.GetModule<LoadSceneComponentGSM>().LoadScene(NextSceneName, null, true);
                        }
                    }
                }
                else
                {
                    OnEnterFail();
                    Debug.Log($"����{NextSceneName}δ����");
                }
            }
        }
    }
}