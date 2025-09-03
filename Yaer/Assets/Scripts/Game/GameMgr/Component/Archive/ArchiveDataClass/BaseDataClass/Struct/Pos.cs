using System;
using UnityEngine;

namespace Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass.Struct
{
    [Serializable]
    public struct Pos
    {
        public float x;
        public float y;
        public float z;

        public Pos(Transform tsf)
        {
            var position = tsf.position;
            x = position.x;
            y = position.y;
            z = position.z;
        }

        public Pos(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }
}