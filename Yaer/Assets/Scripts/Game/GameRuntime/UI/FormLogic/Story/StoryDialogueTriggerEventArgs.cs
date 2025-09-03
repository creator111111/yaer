using GameFramework;
using GameFramework.Event;

namespace Game.GameRuntime.UI.FormLogic.Story.Base
{
    public class StoryDialogueTriggerEventArgs: GameEventArgs
    {
        public override int Id => EventId;

        public static readonly int EventId = typeof(StoryDialogueTriggerEventArgs).GetHashCode();
        
        public string eventName;

        public override void Clear()
        {
            
        }
        
        public static StoryDialogueTriggerEventArgs Create(string eventName)
        {
            StoryDialogueTriggerEventArgs args = ReferencePool.Acquire<StoryDialogueTriggerEventArgs>();
            args.eventName = eventName;
            return args;
        }
    }
}