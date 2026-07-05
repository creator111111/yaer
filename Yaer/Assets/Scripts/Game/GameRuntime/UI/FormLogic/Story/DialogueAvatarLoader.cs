using Game.GameMgr.Component;
using Game.GameMgr.Manager.Res.PathHelper;
using Game.GameMgr;
using Game.Static.Enum.Dialogue;
using Game.Static.Name.Clothes;
using System.Collections.Generic;
using UnityEngine.U2D;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// �Ի�����ͼ������������ʽ��Ϸ�� <see cref="ResComponentGM"/> + �浵װ�磻DialogDebug ɳ���� GF ���ʱ���߻���·����
/// </summary>
public class DialogueAvatarLoader
{
    private static Dictionary<string, SpriteAtlas> avatarAtlasDic = new Dictionary<string, SpriteAtlas>();

    /// <summary>
    /// DialogDebug / �޴浵���ʱ���Ŷ�Ĭ��װ�磨�� <see cref="PlayerDataComponentGM.InitNewGameData"/> һ�£���
    /// ��Ӧͼ����<c>Avatar_Yaer_Dress_Crown.spriteatlas</c>��
    /// </summary>
    private const string SandboxDefaultClothes = ClothesName.Clothes.Dress;
    private const string SandboxDefaultHeadwear = ClothesName.HeadWear.Crown;

    public static void GetAvatar(DialogueRoleName roleName, DialogueFaceType faceType, Action<Sprite> callback)
    {
        string path = ResolveAvatarAtlasPath(roleName);

        // û��ͷ��
        if (string.IsNullOrEmpty(path))
        {
            callback?.Invoke(null);
            return;
        }

        if (avatarAtlasDic.TryGetValue(path, out var cachedAtlas) && cachedAtlas != null)
        {
            callback?.Invoke(cachedAtlas.GetSprite(faceType.ToString()));
            return;
        }

        // ��ʽ���ߣ�ResComponentGM �� GF ResourceComponent
        var resComponent = GameManager.GetGMComponent<ResComponentGM>();
        if (resComponent != null)
        {
            resComponent.LoadAsset<SpriteAtlas>(path, atlas =>
            {
                avatarAtlasDic[path] = atlas;
                callback?.Invoke(atlas != null ? atlas.GetSprite(faceType.ToString()) : null);
            });
            return;
        }

        // ɳ�л��ˣ�Open DialogDebug ֱ�� Play ʱû�� ResComponentGM��Editor ���� AssetDatabase ֱ��ͼ��
        // ����������� Bootstrap ��ע��� ResComponentGM �� ������ GameEntry������˴� null ���� + ֱ������
        LoadAvatarAtlasWithoutResComponent(path, atlas =>
        {
            avatarAtlasDic[path] = atlas;
            callback?.Invoke(atlas != null ? atlas.GetSprite(faceType.ToString()) : null);
        });
    }

    /// <summary>
    /// ��������ͼ��·�����Ŷ���浵װ�磻�� <see cref="PlayerDataComponentGM"/> ʱ��ɳ��Ĭ��װ�磬���� NRE��
    /// </summary>
    private static string ResolveAvatarAtlasPath(DialogueRoleName roleName)
    {
        if (roleName == DialogueRoleName.Yaer)
        {
            var playerData = GameManager.GetGMComponent<PlayerDataComponentGM>();
            if (playerData != null)
            {
                var data = playerData.GetClothesData();
                return DialogueAvatarPathHelper.GetPath(
                    roleName.ToString(),
                    data.GetClothesName(BoneName.Clothes),
                    data.GetClothesName(BoneName.Headwear));
            }

            Debug.LogWarning(
                "[DialogueAvatarLoader] PlayerDataComponentGM δ������ʹ��ɳ��Ĭ���Ŷ�װ�磨Dress + Crown����");
            return DialogueAvatarPathHelper.GetPath(
                roleName.ToString(),
                SandboxDefaultClothes,
                SandboxDefaultHeadwear);
        }

        return DialogueAvatarPathHelper.GetPath(roleName.ToString());
    }

    /// <summary>
    /// �� ResComponentGM ʱ��ͼ�����ء�Editor Play �� AssetDatabase�����������ص� null�����������棬��Ļ�Կɲ�����
    /// </summary>
    private static void LoadAvatarAtlasWithoutResComponent(string path, Action<SpriteAtlas> onComplete)
    {
#if UNITY_EDITOR
        var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);
        if (atlas == null)
        {
            Debug.LogWarning(
                $"[DialogueAvatarLoader] ɳ��ģʽδ�ҵ�ͼ��: {path}����ȷ�� Assets/GameRes/Atlas/Avatar ����Դ���ڡ�");
        }

        onComplete?.Invoke(atlas);
#else
        Debug.LogWarning(
            $"[DialogueAvatarLoader] �� ResComponentGM �ҷ� Editor����������: {path}");
        onComplete?.Invoke(null);
#endif
    }
}
