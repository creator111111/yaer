using System;
using GameFramework.UnityRuntimeExtend.Resource;
using UnityEngine;
using UnityEngine.U2D;

namespace Game.GameMgr.Manager.Res.SceneRes.Config
{
    [CreateAssetMenu(menuName = "SceneResConfig/SelectClothesScene")]
    public class SelectClothesSceneResConfig : BaseSceneResConfig
    {

        public override void Preload(Action<bool> onComplete)
        {
            sceneResManager.PreloadResources(onComplete,
                new PreloadAssetInfo<GameObject>("SelectClothesPanel"),
                new PreloadAssetInfo<GameObject>("ToggleClothes"),
                new PreloadAssetInfo<GameObject>("YaerTemplateClothes"),
                new PreloadAssetInfo<GameObject>("YaerEmptyBone"),
                new PreloadAssetInfo<SpriteAtlas>("Yaer_Armor_NoHeadWear_DialogueAvatar"),
                new PreloadAssetInfo<SpriteAtlas>("Yaer_Dress_Crown_DialogueAvatar"),
                new PreloadAssetInfo<SpriteAtlas>("Yaer_Armor_Crown_DialogueAvatar"),
                new PreloadAssetInfo<SpriteAtlas>("Yaer_Armor_ArmorHead_DialogueAvatar"),
                new PreloadAssetInfo<SpriteAtlas>("King_DialogueAvatar"),
                new PreloadAssetInfo<SpriteAtlas>("Xiaer_DialogueAvatar"),
                new PreloadAssetInfo<RuntimeAnimatorController>("Yaer_Armor_Combat_NoHeadWear"),
                new PreloadAssetInfo<RuntimeAnimatorController>("Yaer_Armor_Home_Crown"),
                new PreloadAssetInfo<RuntimeAnimatorController>("Yaer_Armor_Home_ArmorHead"),
                new PreloadAssetInfo<RuntimeAnimatorController>("Yaer_Armor_Home_NoHeadWear"),
                new PreloadAssetInfo<RuntimeAnimatorController>("Yaer_Dress_Home_Crown")
            );
        }

        public override void Release(Action<bool> onComplete)
        {
            onComplete?.Invoke(true);
        }
    }
}