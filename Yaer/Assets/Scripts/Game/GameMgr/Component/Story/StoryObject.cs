using GameFramework;
using GameFramework.ObjectPool;
using UnityEngine;

namespace Game.GameMgr.Component.Story
{
    /// <summary>
    /// 用于包装 BaseStory 的对象池对象
    /// </summary>
    public class StoryObject : ObjectBase
    {
        public static StoryObject Create(string name, object target)
        {
            StoryObject storyObject = ReferencePool.Acquire<StoryObject>();
            storyObject.Initialize(name, target);
            return storyObject;
        }

        protected override void Release(bool isShutdown)
        {
            BaseStory story = (BaseStory)Target;
            if (story != null)
            {
                Object.Destroy(story.gameObject);
            }
        }
    }
}