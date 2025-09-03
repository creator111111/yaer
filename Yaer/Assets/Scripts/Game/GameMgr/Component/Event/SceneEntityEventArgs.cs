using GameFramework;
using GameFramework.Event;

namespace Game.GameMgr.Component.Event
{
    public class SceneEntityEventArgs: GameEventArgs
    {
        public static int EventId => typeof(SceneEntityEventArgs).GetHashCode();
        public override int Id => EventId;

        public string eventName;
        
        public override void Clear()
        {
            eventName = null;
        }

        public static SceneEntityEventArgs Create() => ReferencePool.Acquire<SceneEntityEventArgs>();
    }
    
    public class SceneEntityEventArgs<T>: GameEventArgs
    {
        public static int EventId => typeof(SceneEntityEventArgs<T>).GetHashCode();
        public override int Id => EventId;

        public string eventName;
        public T arg;
        
        public override void Clear()
        {
            eventName = null;
            arg = default;
        }

        public static SceneEntityEventArgs<T> Create() => ReferencePool.Acquire<SceneEntityEventArgs<T>>();
    }
}