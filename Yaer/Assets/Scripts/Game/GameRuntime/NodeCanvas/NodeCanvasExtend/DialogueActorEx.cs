using Game.GameMgr.Component;
using Game.GameMgr.Manager.Res.PathHelper;
using Game.GameMgr;
using Game.Static.Enum.Dialogue;
using Game.Static.Name.Clothes;
using NodeCanvas.DialogueTrees;
using System;
using UnityEngine;
using UnityEngine.U2D;
using System.Collections.Generic;

namespace Game.GameRuntime.Story.NodeCanvasExtend
{
    [AddComponentMenu("NodeCanvas/Dialogue Actor Extend")]
    public class DialogueActorEx : DialogueActor
    {
        [SerializeField]
        protected DialogueRoleName _roleName;

        public DialogueRoleName RoleName
        {
            get { return _roleName; }
        }

        public event Action<DialogueRoleName, DialogueFaceType, Sprite> OnRefreshAvatarEvent;

        /// <summary>
        /// 刷新角色头像
        /// </summary>
        /// <param name="roleName">角色</param>
        /// <param name="faceType">表情</param>
        /// <returns></returns>
        public void RefreshAvatar(DialogueFaceType faceType, Action<Sprite> callBack)
        {
            callBack += (sprite) => OnRefreshAvatarEvent?.Invoke(RoleName, faceType, sprite);

            DialogueAvatarLoader.GetAvatar(RoleName, faceType, callBack);
        }

    }
}