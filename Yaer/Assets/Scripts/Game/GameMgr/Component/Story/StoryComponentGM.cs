using System;
using Game.GameMgr.Component.Base;
using GameFramework.ObjectPool;
using GameFramework.UnityRuntime.ObjectPool;
using GameFramework.UnityRuntime.Utility;
using UnityEngine;

namespace Game.GameMgr.Component.Story
{
    /// <summary>
    /// 剧情组件
    /// </summary>
    public class StoryComponentGM : BaseComponentGM
    {
        // 剧情对象池（容量可以根据需求配置）
        private IObjectPool<StoryObject> storyObjectPool;

        // 在组件初始化时创建对象池
        public override void OnInit()
        {
            base.OnInit();

            storyObjectPool = GameManager.GetGFComponent<ObjectPoolComponent>().CreateSingleSpawnObjectPool<StoryObject>("StoryPool", 16);
        }

        public void Unspawn(BaseStory story)
        {
            story.transform.SetParent(transform);
            story.transform.localPosition = Vector3.zero;
            storyObjectPool.Unspawn(story);
        }
    }
}