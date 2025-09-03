using System;

namespace GameFramework.CoreExtend.Event
{
    public class EventInfo : IEventInfo
    {
        public Action action;

        // 构造函数方便外部初始化直接添加事件
        public EventInfo(Action newAction)
        {
            action += newAction;
        }
    }
    
    public class EventInfo<T> : IEventInfo
    {
        public Action<T> action;

        public EventInfo(Action<T> newAction)
        {
            action += newAction;
        }
    }
}