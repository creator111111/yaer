using GameFramework.UnityRuntimeExtend.Resource;
using UnityEngine;

namespace Game.GameMgr.Manager.Res.SceneRes.Config.Generic.Package
{
    public class PlayerAnimaResPackage
    {
        public static PreloadAssetInfo[] Package => new[]
        {
            new PreloadAssetInfo<RuntimeAnimatorController>("Yaer_Armor_Combat_NoHeadWear"),
            new PreloadAssetInfo<RuntimeAnimatorController>("Yaer_Armor_Home_Crown"),
            new PreloadAssetInfo<RuntimeAnimatorController>("Yaer_Armor_Home_ArmorHead"),
            new PreloadAssetInfo<RuntimeAnimatorController>("Yaer_Armor_Home_NoHeadWear"),
            new PreloadAssetInfo<RuntimeAnimatorController>("Yaer_Dress_Home_Crown")
        };
    }
}