using DG.Tweening;
using DG.Tweening.Core.Easing;
using Game.GameMgr;
using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.Entities.SceneEntities.ForestEastScene;
using Game.GameRuntime.GameSceneManager.Base;
using Game.GameRuntime.GameSceneManager.Component.CameraGSM;
using System;
using System.Collections.Generic;
using UnityEngine;

// 树洞事件管理器
public class ForestEastTreeBridgeStoryMgr : BaseSceneStoryMgr
{
    public TreeBridgeLogic storyLogic; // 事件场景对象脚本
    public bool playerIsInTreeBridge; // 玩家是否在树洞中
    public Tween cameraTween = null;
    public new static ForestEastTreeBridgeStoryMgr instance;
    public new static ForestEastTreeBridgeStoryMgr getInstance()
    {
        if (instance == null)
        {
            instance = new ForestEastTreeBridgeStoryMgr();
        }
        return instance;
    }

    public override void OnSceneStoryTrigger(bool isStart)
    {
        base.OnSceneStoryTrigger(isStart);
        // 设置虫子朝向人物移动
        if (storyLogic != null)
        {
            foreach(var woodWormLogic in storyLogic.storyWoodWormLogicList)
            {
                if (woodWormLogic != null)
                {
                    woodWormLogic.moveToPlayer();
                }
            }
        }
    }

    public override void CheckEventHasEnd()
    {
        base.CheckEventHasEnd();
    }

    public GameObject GetEnterStartNode(bool isLeft = true)
    {
        if (storyLogic == null) { return null; }
        if (isLeft) { return storyLogic.enterNodeLeft; }
        else { return storyLogic.enterNodeRight;}
    }
    public GameObject GetOutStartNode(bool isLeft = true)
    {
        if (storyLogic == null) { return null; }
        if (isLeft) { return storyLogic.outNodeLeft; }
        else { return storyLogic.outNodeRight; }
    }

    // 在系统控制人物行动时需要改变入口和出口的激活状态
    public void ChangeEnterAndOutNodeActive(bool isEnterTree)
    {
        if (storyLogic == null) { return; }
        storyLogic.storyTriggerEnterNodeLeft.SetActive(isEnterTree);
        storyLogic.storyTriggerEnterNodeRight.SetActive(isEnterTree);
        storyLogic.storyTriggerOutNodeLeft.SetActive(!isEnterTree);
        storyLogic.storyTriggerOutNodeLeft.SetActive(!isEnterTree);
    }

    public void AwakeAllStoryNodeActive()
    {
        storyLogic.storyTriggerEnterNodeLeft.SetActive(true);
        storyLogic.storyTriggerEnterNodeRight.SetActive(true);
        storyLogic.storyTriggerOutNodeLeft.SetActive(true);
        storyLogic.storyTriggerOutNodeLeft.SetActive(true);
    }

    // 修改摄像机数据
    public void ChangeCamera(bool isEnterTree, CameraComponentGSM cameraMgr)
    {
        if (storyLogic == null) { return; }
        
        var targetColliderArea = isEnterTree ? storyLogic.newCameraBoundingArea : storyLogic.oldCameraBoundingArea;
        var colliderArea = targetColliderArea.GetComponent<PolygonCollider2D>();
        cameraMgr.ChangeCameraBoundingArea(colliderArea);
        // 设置摄像机显示缩放
        if (isEnterTree)
        {
            cameraMgr.ChangeVirtualCameraShowSize(5);
        }
        else
        {
            cameraMgr.ResetVirtualCameraShowSize();
        }
    }
    public void CameraAction()
    {
        var sceneMgr = GameManager.GetGameSceneManager() as BaseGameSceneManager;
        var cameraMgr = sceneMgr.GetModule<CameraComponentGSM>();
        var mainCamera = cameraMgr.CameraComponent.gameObject;
        var basePos = mainCamera.transform.position;
        mainCamera.transform.DOKill(true);
        PlayTreeBridgeMoveSfx();
        List<Tween> moveTweens = new List<Tween>() {
                GameActionMgr.runMoveToWorldPosAction(mainCamera, new Vector2(basePos.x, basePos.y + 0.3f), 0.3f).SetEase(Ease.Linear),
                GameActionMgr.runMoveToWorldPosAction(mainCamera, new Vector2(basePos.x, basePos.y), 0.3f).SetEase(Ease.Linear).OnComplete(() =>
                {
                    PlayTreeBridgeMoveSfx();
                }),
                //GameActionMgr.runMoveToWorldPosAction(mainCamera, new Vector2(basePos.x, basePos.y-0.3f), 0.2f).SetEase(Ease.Linear),
                //GameActionMgr.runMoveToWorldPosAction(mainCamera, new Vector2(basePos.x, basePos.y), 0.2f).SetEase(Ease.Linear),
            };
        cameraTween?.Kill(true);
        cameraTween = GameActionMgr.runSequenceAction(mainCamera, moveTweens).SetLoops(-1);
        cameraTween.SetAutoKill(false);
    }

    public void StopCameraAction()
    {
        var sceneMgr = GameManager.GetGameSceneManager() as BaseGameSceneManager;
        var cameraMgr = sceneMgr.GetModule<CameraComponentGSM>();
        var mainCamera = cameraMgr.CameraComponent.gameObject;
        //DOTween.Kill(mainCamera.transform, true);
        //DOTween.Kill(cameraTween, true);
        cameraTween.Kill(true);
        mainCamera.transform.DOKill(true);
        // 最后让摄像机回归原点
        GameActionMgr.runMoveToWorldPosAction(mainCamera, Vector2.zero, 0.1f).SetEase(Ease.Linear);
    }

    public void PlayTreeBridgeMoveSfx()
    {
        storyLogic.PlayTreeBridgeMoveSfx();
    }

    // 玩家进入或者出树洞
    public void OnEnterOrOutTreeBridge(bool isEnterTree)
    {
        foreach (var obj in storyLogic.hideObjsInEnterTreeBridge)
        {
            obj.gameObject.SetActive(!isEnterTree);
        }
        if (isEnterTree)
        {
            storyLogic.waterSoundEntity.ChangeVolumeByRate(0.5f);
        }
        else
        {
            storyLogic.waterSoundEntity.ResetCurVolume();
        }
    }

    // =============================存档和读档Start
    public override void ParseInternal(MasterGameData masterData)
    {
    }

    public override void SerializeInternal(MasterGameData masterData)
    {
    }

    //===============================存档和读档End
}
