using Game.GameMgr.Component;
using Game.GameMgr.Manager.Res.PathHelper;
using Game.GameMgr;
using Game.Static.Enum.Dialogue;
using Game.Static.Name.Clothes;
using System.Collections.Generic;
using UnityEngine.U2D;
using UnityEngine;
using System;

public class DialogueAvatarLoader
{
    private static Dictionary<string, SpriteAtlas> avatarAtlasDic = new Dictionary<string, SpriteAtlas>();

    public static void GetAvatar(DialogueRoleName roleName, DialogueFaceType faceType, Action<Sprite> callback)
    {
        string path;
        if (roleName == DialogueRoleName.Yaer)
        {
            var data = GameManager.GetGMComponent<PlayerDataComponentGM>().GetClothesData();
            path = DialogueAvatarPathHelper.GetPath(roleName.ToString(), data.GetClothesName(BoneName.Clothes), data.GetClothesName(BoneName.Headwear));
        }
        else
        {
            path = DialogueAvatarPathHelper.GetPath(roleName.ToString());
        }

        // 没有头像
        if (string.IsNullOrEmpty(path))
        {
            callback?.Invoke(null);
            return;
        }

        if (avatarAtlasDic.ContainsKey(path) == false)
        {
            // 加载
            GameManager.GetGMComponent<ResComponentGM>().LoadAsset<SpriteAtlas>(path, atlas =>
            {
                avatarAtlasDic[path] = atlas;
                callback?.Invoke(atlas.GetSprite(faceType.ToString()));
            });
        }
        else
        {
            callback?.Invoke(avatarAtlasDic[path].GetSprite(faceType.ToString()));
        }
    }

}
