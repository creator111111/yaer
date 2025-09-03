using System;
using Game.GameRuntime.Entities.Base.BaseSceneObj.Base;
using Game.GameRuntime.GameSceneManager.Base;
using Game.GameRuntime.GameSceneManager.Component;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.GameRuntime.Entities.Base.BaseSceneObj
{
    // public abstract class BaseInteractive : BaseSceneObject, IBaseInteractive
    // {
    //     [SerializeField] protected bool isClose; // 靠近触发范围
    //     [SerializeField] protected InputAction inputAction;
    //     private bool isTrigger; // 已经触发
    //
    //     protected override void OnDestroy()
    //     {
    //         base.OnDestroy();
    //
    //         inputAction.Dispose();
    //         isTrigger = false;
    //     }
    //
    //     public override void Init(IGameSceneManager m)
    //     {
    //         base.Init(m);
    //
    //         // 初始化 inputAction 并绑定 E 键
    //         inputAction = new InputAction(type: InputActionType.Button, binding: "<Keyboard>/e");
    //         inputAction.performed += ctx => OnInput();
    //         inputAction.Enable();
    //     }
    //
    //
    //     #region 触发事件相关
    //
    //     protected virtual void OnTriggerEnter2D(Collider2D other)
    //     {
    //         // 玩家进入触发器范围
    //         if (other.gameObject.CompareTag("Player") && other.gameObject.name == "Event")
    //         {
    //             isClose = true;
    //             EnterEvent();
    //         }
    //     }
    //
    //     protected virtual void OnTriggerExit2D(Collider2D other)
    //     {
    //         // 离开触发器响应范围
    //         if (other.gameObject.CompareTag("Player") && other.gameObject.name == "Event") isClose = false;
    //     }
    //
    //     /// <summary>
    //     ///     点击交互
    //     /// </summary>
    //     public void OnClick()
    //     {
    //         if (SceneManager.Pause) return;
    //
    //         if (!isClose) return;
    //         if (isTrigger) return;
    //         OnInteractive();
    //     }
    //
    //     /// <summary>
    //     ///     按键交互
    //     /// </summary>
    //     private void OnInput()
    //     {
    //         if (SceneManager.Pause) return;
    //
    //         if (!isClose) return;
    //         if (isTrigger) return;
    //         OnInteractive();
    //     }
    //
    //     protected virtual void OnInteractive()
    //     {
    //         isTrigger = true;
    //
    //         // 交互时禁止玩家操作
    //         SceneManager.AllowControl = false;
    //     }
    //
    //     protected virtual void EndInteractive()
    //     {
    //         isTrigger = false;
    //         SceneManager.AllowControl = true;
    //     }
    //
    //     protected virtual void EnterEvent()
    //     {
    //     }
    //
    //     #endregion
    //
    //
    //     #region 事件通知相关
    //
    //     protected void RegisterEvent(string eventName, Action<object> action)
    //     {
    //         SceneManager.GetModule<EventComponentGSM>().RegisterSceneEvent(eventName, action); 
    //     }
    //
    //     protected void TriggerNormalDialogue(string storyName, Action callBack = null)
    //     {
    //         if (callBack != null)
    //         {
    //             callBack += EndInteractive;
    //             var a = new ShowNormalDialoguePanelArgs
    //             {
    //                 fileName = storyName,
    //                 endDialogueCallBack = callBack
    //             };
    //             SceneManager.GetModule<StoryComponentGSM>().TriggerStory(storyName);
    //         }
    //         else
    //         {
    //             var a = new ShowNormalDialoguePanelArgs
    //             {
    //                 fileName = storyName,
    //                 endDialogueCallBack = EndInteractive
    //             };
    //             SceneManager.GetModule<StoryComponentGSM>().TriggerStory(storyName);
    //         }
    //     }
    //
    //     protected void TriggerLoopDialogue(string storyName, Action callBack = null)
    //     {
    //         if (callBack != null)
    //         {
    //             callBack += EndInteractive;
    //             var a = new ShowLoopDialoguePanelArgs
    //             {
    //                 fileName = storyName,
    //                 eachDialogueCallBack = EndInteractive,
    //                 endDialogueCallBack = callBack
    //             };
    //             SceneManager.GetModule<StoryComponentGSM>().TriggerStory(storyName);
    //         }
    //         else
    //         {
    //             var a = new ShowLoopDialoguePanelArgs
    //             {
    //                 fileName = storyName,
    //                 eachDialogueCallBack = EndInteractive,
    //                 endDialogueCallBack = EndInteractive
    //             };
    //             SceneManager.GetModule<StoryComponentGSM>().TriggerStory(storyName);
    //         }
    //     }
    //
    //     #endregion
    // }
}