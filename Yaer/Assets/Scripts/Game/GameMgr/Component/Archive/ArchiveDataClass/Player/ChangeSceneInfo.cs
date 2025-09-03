using System;
using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass.Struct;
using UnityEngine;

namespace Game.GameMgr.Component.Archive.ArchiveDataClass.Player
{
    [Serializable]
    public class ChangeSceneInfo
    {
        private readonly Pos jumpPos;

        public ChangeSceneInfo(string fromSceneName, string toSceneName, Transform tsfPos = null)
        {
            this.FromSceneName = fromSceneName;
            this.ToSceneName = toSceneName;

            if (tsfPos != null) jumpPos = new Pos(tsfPos);
        }

        public ChangeSceneInfo(string to)
        {
            ToSceneName = to;
        }

        public string FromSceneName { get; }

        public string ToSceneName { get; }

        public Vector2 GetJumpPos()
        {
            return new Vector2(jumpPos.x, jumpPos.y);
        }
    }
}