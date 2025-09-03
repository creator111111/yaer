using System;
using System.Collections.Generic;
using GameFramework.UnityRuntimeExtend.Resource;
using UnityEngine;
using UnityEngine.U2D;

namespace Game.GameMgr.Manager.Res.SceneRes.Config
{
    [CreateAssetMenu(menuName = "SceneResConfig/ForestScene")]
    public class ForestSceneResConfig : BaseSceneResConfig
    {
        public override void Preload(Action<bool> onComplete)
        {
            List<PreloadAssetInfo> array = new List<PreloadAssetInfo>()
            {
                new PreloadAssetInfo<GameObject>("FightingPanel"),
                // new PreloadAssetInfo<GameObject>("Effect", "Effect_Player_NormalAttack"), 
                // new PreloadAssetInfo<GameObject>("Effect", "Effect_Player_JumpUpDust"),
                // new PreloadAssetInfo<GameObject>("Effect", "Effect_Player_JumpDownDust"),
                // new PreloadAssetInfo<GameObject>("Effect", "Effect_Player_ChangeDirDust"),
                // new PreloadAssetInfo<GameObject>("Effect", "Effect_MonsterState_Angry"), 
                // new PreloadAssetInfo<GameObject>("Effect", "Effect_MonsterState_Weak"),
                // new PreloadAssetInfo<GameObject>("Effect", "Effect_MonsterState_Tired"), 
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
            };

            foreach (var i in assetInfos)
            {
                var newInfo = new PreloadAssetInfo(i.GetAssetType(), i.path);
                array.Add(newInfo);
            }
            
            sceneResManager.PreloadResources(onComplete, array.ToArray());
        }

        public override void Release(Action<bool> onComplete)
        {
            onComplete?.Invoke(true);
        }
    }
}