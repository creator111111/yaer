using Game.DataTable.AchievenmentConfig;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameMgr;
using Game.GameMgr.Component.UI;
using Game.GameMgr.Manager.Settings;
using Game.Static.Name.Settings;
using Game.Static.Path;
using GameFramework.DataTable;
using GameFramework.UnityRuntime.Setting;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Game.GameRuntime.Entities.Monster;


public class SceneMonsterDataMgr
{
    //public List<BaseMonster> hasDeadMonsters = new List<BaseMonster>(); // 当前场景中的所有怪物对象

    public static SceneMonsterDataMgr instance = null;
    public static SceneMonsterDataMgr getInstance()
    {
        if (instance == null)
        {
            instance = new SceneMonsterDataMgr();
        }
        return instance;
    }
    #region 初始化

    public void Init()
    {

    }

    #endregion

    public SceneMonsterData GetSceneMonsterData()
    {
        return GameManager.GetGameSceneManager().GetArchiveData<SceneMonsterData>();
    }

    public bool MonterHasDead(BaseMonster entityLogic)
    {
        var sceneMonsterData = GetSceneMonsterData();
        if (sceneMonsterData != null)
        {
            return sceneMonsterData.GetMonsterHasDeadByTag(entityLogic.sceneMonsterTag);
        }
        return false;
    }

    public void RecordMonsterHasDead(BaseMonster monster)
    {
        var sceneMonsterData = GetSceneMonsterData();
        if (sceneMonsterData != null)
        {
            sceneMonsterData.RecordMonsterByTag(monster.sceneMonsterTag);
        }
    }

    public void ClearAllMonsterSafeState()
    {
        var sceneMonsterData = GetSceneMonsterData();
        if (sceneMonsterData != null)
        {
            var keys = sceneMonsterData.sceneMonsterSafeStates.Keys;
            foreach (var key in new List<string>(keys))
            {
                sceneMonsterData.sceneMonsterSafeStates[key] = false;
            }
        }
    }
}