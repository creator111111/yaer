using GameFramework.UnityRuntimeExtend.Resource;
using UnityEngine.U2D;

namespace Game.GameMgr.Manager.Res.SceneRes.Config.Generic.Package
{
    public class AtlasResPackage
    {
        public static PreloadAssetInfo[] Package => new PreloadAssetInfo[]
        {
            new PreloadAssetInfo<SpriteAtlas>("Yaer_Armor_NoHeadWear_DialogueAvatar"),
            new PreloadAssetInfo<SpriteAtlas>("Yaer_Dress_Crown_DialogueAvatar"),
            new PreloadAssetInfo<SpriteAtlas>("Yaer_Armor_Crown_DialogueAvatar"),
            new PreloadAssetInfo<SpriteAtlas>("Yaer_Armor_ArmorHead_DialogueAvatar"),
            new PreloadAssetInfo<SpriteAtlas>("King_DialogueAvatar"),
            new PreloadAssetInfo<SpriteAtlas>("Xiaer_DialogueAvatar")
        };
    }
}