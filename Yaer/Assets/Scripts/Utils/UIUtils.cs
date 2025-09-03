using DG.Tweening;
using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.Entities.Component.Effect;
using Game.GameRuntime.Entities.Effect.Player.Atk;
using Game.GameRuntime.Entities.Effect.Player.BeHurt;
using Game.GameRuntime.UI.FormLogic.Base;
using Game.Static.Path;
using GameFramework.UnityRuntime.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum AniEffectType { 
    PlayerNorAtk, // 人物普通攻击特效
    PlayerBeHurt, // 人物受伤特效
}


public class UIUtils : object
{
    static UIUtils instance;
    public static UIUtils getInstance()
    {
        if (instance == null)
        {
            instance = new UIUtils();
        }
        return instance;
    }

    // 定义一个方法，查找某个GameObject下的第一个同名的子节点
    public static GameObject findChild(GameObject parent, string name, bool hasDebugLog=true)
    {
        if (!parent)
        {
            Debug.LogError("Parent node ERROR!!!");
            return null;
        }
        var targetObj = realFindChild(parent, name);
        if (targetObj == null)
        {
            if (hasDebugLog)
            {
                Debug.Log("HINT: In parent [" + parent.name + "] has not find child [" + name + "] !!!");
            }
            return null;
        }
        else
        {
            return targetObj;
        }
       
    }

    private static GameObject realFindChild(GameObject parent, string name)
    {
        var childCount = parent.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            var childNode = parent.transform.GetChild(i);
            if (childNode.name == name)
            {
                // 找到第一个对应名字的子节点
                return childNode.gameObject;
            }
            var childNodeCount = childNode.childCount;
            if (childNodeCount > 0)
            {
                var newChild = realFindChild(childNode.gameObject, name);
                if (newChild)
                {
                    return newChild;
                }
            }
        }
        return null;
    }

    // 实例化任意预制体,从Resources路径下找
    public static GameObject initPrefabByPath(string prePath, GameObject parent = null)
    {
        var prefabObj = Resources.Load<GameObject>(prePath);
        if (!prefabObj)
        {
            return null;
        }
        var realPreafab = UnityEngine.Object.Instantiate(prefabObj);// 实例化预制体
        // 存在预设的父节点则设置父节点
        if (parent)
        {
            // 使用Setparent方法是为了重置子预制体的缩放
            //realPreafab.transform.parent = parent.transform;
            realPreafab.transform.SetParent(parent.transform, false);
        }
        return realPreafab;
    }

    // 添加特效
    public static void addBehurtAnimationEffect(AniEffectType effectType, BaseEntityLogic entityLogic, Vector2 pos, int playCount = 1, Action onAddEffectCallFunc = null)
    {
        // 播放动画
        if (entityLogic.beHurtEffectNode != null)
        {
            // 已经存在有受伤节点了就只用设置可见度和坐标就行了
            entityLogic.beHurtEffectNode.SetActive(true);
            var effectNode = UnityEngine.Object.Instantiate(entityLogic.beHurtEffectNode);
            effectNode.SetActive(true);
            playEffectNodeAni(effectNode, effectType, entityLogic, pos, playCount, onAddEffectCallFunc);
            entityLogic.beHurtEffectNode.SetActive(false);
            return;
        }
        var effectPathDictData = new Dictionary<AniEffectType, string>() {
            { AniEffectType.PlayerNorAtk, "Assets/GameRes/Prefabs/Entity/Effect/Player/Battle/Effect_NormalAttack.prefab"},
            { AniEffectType.PlayerBeHurt, "Assets/GameRes/Prefabs/Entity/Effect/Player/Battle/Effect_BeHurt.prefab"},
        };
        if (!effectPathDictData.ContainsKey(effectType)) { return; }
        var prefabPath = effectPathDictData[effectType];
        var resMgr = GameManager.GetGMComponent<ResComponentGM>();
        
        resMgr.LoadAsset<GameObject>(prefabPath, (obj) =>
        {
            if (entityLogic.beHurtEffectNode != null) { return; }
            var parent = entityLogic.gameObject;
            entityLogic.beHurtEffectNode = obj; // 保存加载好的预制体，用于后续实例化使用
            var effectNode = UnityEngine.Object.Instantiate(obj);
            effectNode.SetActive(true);
            playEffectNodeAni(effectNode, effectType, entityLogic, pos, playCount, onAddEffectCallFunc);
            entityLogic.beHurtEffectNode.SetActive(false);
        });
    }

    public static void playEffectNodeAni(GameObject effectNode, AniEffectType effectType, BaseEntityLogic entityLogic, 
        Vector2 pos, int playCount = 1, Action onAddEffectCallFunc = null)
    {
        AnimaEffectComponent effect = null;
        var parent = entityLogic.gameObject;
        //effectNode.transform.SetParent(parent.transform, false);
        effectNode.transform.position = pos;
        // 播放动画
        if (effectType == AniEffectType.PlayerNorAtk) { effect = effectNode.GetComponent<PlayerNormalAttackEffect>(); }
        else if (effectType == AniEffectType.PlayerBeHurt) { effect = effectNode.GetComponent<PlayerBeHurtEffect>(); }
        if (effect != null)
        {
            effect.Play(playCount);
        }
        if (onAddEffectCallFunc != null) onAddEffectCallFunc?.Invoke();
    }

    // 在某个界面上播放一个按钮音效，该界面只能同时播放一个按钮音效
    public static void PlayBtnAudio(UIFormLogic uiLogic, string audioName = "确认.mp3")
    {
        var btnSfxNode = findChild(uiLogic.gameObject, "CommonBtnSfx", false);
        SoundToggleComponent sfxCpn;
        if (btnSfxNode == null)
        {
            btnSfxNode = new GameObject();
            btnSfxNode.gameObject.transform.SetParent(uiLogic.transform, false); // 添加到当前界面
            btnSfxNode.name = "CommonBtnSfx";
            btnSfxNode.SetActive(false);
            sfxCpn = btnSfxNode.AddComponent<SoundToggleComponent>();
            sfxCpn.enabled = false;
        }
        else
        {
            sfxCpn = btnSfxNode.GetComponent<SoundToggleComponent>();
        }
        btnSfxNode.SetActive(true);
        sfxCpn.isAutoPlay = false;
        sfxCpn.SetIsLoop(false);
        sfxCpn.SetSoundType(Game.Static.Path.Sound.SoundType.SFX);
        sfxCpn.ChangeSoundRes(audioName);
        sfxCpn.PlaySound();
    }

    // 播放一个翻页的声音
    public static void PlayTapExChangeSfx(UIFormLogic uiLogic) {
        PlayBtnAudio(uiLogic, "翻页的声音.wav");
    }


    // 打开一个指定名称的界面
    public static void OpenPanel(string panelName, EUIGroup uiGroupType=EUIGroup.Middle, object userData=null, Action<UIFormLogic> openCallFunc=null)
    {
        string uiPrefabPath = UIPrefabPath.GetUIPrefabPath(panelName);
        var uiForm = GameManager.GetGMComponent<UIComponentGM>().GetUIForm(uiPrefabPath);
        if (uiForm == null)
        {
            GameManager.GetGMComponent<UIComponentGM>()
            .OpenUIForm(uiPrefabPath, uiGroupType, new OpenFormArgs()
            {
                userData = userData,
                callBack = openCallFunc
            });
        }
    }

    // 关闭一个打开的界面
    public static void ClosePanel(string panelName)
    {
        string uiPrefabPath = UIPrefabPath.GetUIPrefabPath(panelName);
        GameManager.GetGMComponent<UIComponentGM>().CloseUIForm(uiPrefabPath);
    }
    public static void ClosePanel(UIForm uiForm)
    {
        GameManager.GetGMComponent<UIComponentGM>().CloseUIForm(uiForm);
    }

    // 获取一个界面
    public static T GetPanel<T>(string panelName) where T : class
    {
        string uiPrefabPath = UIPrefabPath.GetUIPrefabPath(panelName);
        var uiForm = GameManager.GetGMComponent<UIComponentGM>().GetUIForm(uiPrefabPath);
        return uiForm.Logic as T;
    }
}