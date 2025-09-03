using System;
using GameFramework.UnityRuntimeExtend.Resource;
using UnityEngine;
using UnityEngine.U2D;

namespace Game.GameMgr.Manager.Res.SceneRes.Config
{
    [CreateAssetMenu(menuName = "SceneResConfig/StartScene")]
    public class StartSceneResConfig : BaseSceneResConfig
    {
        public override void Preload(Action<bool> onComplete)
        {
            sceneResManager.PreloadResources(onComplete,
                new PreloadAssetInfo<GameObject>("StartPanel"),
                new PreloadAssetInfo<GameObject>("SaveGamePanel"),
                new PreloadAssetInfo<GameObject>("LoadGamePanel"),
                new PreloadAssetInfo<GameObject>("ButtonArchive"),
                new PreloadAssetInfo<GameObject>("SystemTipsPanel"),
                new PreloadAssetInfo<SpriteAtlas>("SystemTipsAvatar"),
                new PreloadAssetInfo<SpriteAtlas>("SystemTipsChar"),
                new PreloadAssetInfo<GameObject>("AchievementPanel"),
                new PreloadAssetInfo<GameObject>("SelectHardPanel"),
                new PreloadAssetInfo<GameObject>("SettingsPanel")
            );
        }

        public override void Release(Action<bool> onComplete)
        {
            onComplete?.Invoke(true);
        }
    }
}