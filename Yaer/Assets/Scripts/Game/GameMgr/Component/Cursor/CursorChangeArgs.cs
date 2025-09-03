using System;

namespace Game.GameMgr.Component.Cursor
{
    public class CursorChangeArgs : IComparable<CursorChangeArgs>
    {
        public CursorState TargetState;
        public Guid guid;
        public int Priority;

        public CursorChangeArgs(CursorState targetState, Guid guid, int priority)
        {
            TargetState = targetState;
            this.guid = guid;
            Priority = priority;
        }

        public int CompareTo(CursorChangeArgs other)
        {
            return -this.Priority.CompareTo(other.Priority);
        }
    }
}