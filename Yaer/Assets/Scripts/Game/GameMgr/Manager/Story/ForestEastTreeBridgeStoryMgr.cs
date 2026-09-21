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

// 锟斤拷锟斤拷锟铰硷拷锟斤拷锟斤拷锟斤拷
public class ForestEastTreeBridgeStoryMgr : BaseSceneStoryMgr
{
    public TreeBridgeLogic storyLogic; // 锟铰硷拷锟斤拷锟斤拷锟斤拷锟斤拷疟锟�?
    public bool playerIsInTreeBridge; // 锟斤拷锟斤拷欠锟斤拷锟斤拷锟斤拷锟斤拷锟�?
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
        // 锟斤拷锟矫筹拷锟接筹拷锟斤拷锟斤拷锟斤拷锟狡讹拷
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

    /// <summary>
    /// 杩涙礊鏃舵墦寮€杩涙礊瀵圭櫧鐩掋€佸叧鎺夊嚭娲炵洅锛涘嚭娲炵浉鍙嶃€�
    /// 绗�浜屽�勫繀椤诲啓 Right锛氭棫浠ｇ爜鎶� Out Left SetActive 鍐欎簡涓ら亶锛屽彸鍙ｅ嚭娲炵洅浠庢湭琚�鏀癸紙渚︽帰 E锛夈€�
    /// </summary>
    public void ChangeEnterAndOutNodeActive(bool isEnterTree)
    {
        if (storyLogic == null) { return; }
        storyLogic.storyTriggerEnterNodeLeft.SetActive(isEnterTree);
        storyLogic.storyTriggerEnterNodeRight.SetActive(isEnterTree);
        storyLogic.storyTriggerOutNodeLeft.SetActive(!isEnterTree);
        storyLogic.storyTriggerOutNodeRight.SetActive(!isEnterTree);
    }

    /// <summary>
    /// 榛戝箷缁撴潫鍚庨噸鏂版縺娲诲洓渚ц繘鍑虹洅銆傚悓鏍峰繀椤诲啓 Right锛屼笉鑳藉啀鍙屽啓 Left銆�
    /// </summary>
    public void AwakeAllStoryNodeActive()
    {
        if (storyLogic == null) { return; }
        storyLogic.storyTriggerEnterNodeLeft.SetActive(true);
        storyLogic.storyTriggerEnterNodeRight.SetActive(true);
        storyLogic.storyTriggerOutNodeLeft.SetActive(true);
        storyLogic.storyTriggerOutNodeRight.SetActive(true);
    }

    /// <summary>
    /// 锟斤拷/锟斤拷锟斤拷锟斤拷锟叫伙拷锟斤拷锟斤拷锟斤拷呓锟斤拷锟�? OrthoSize锟斤拷
    /// 锟斤拷锟斤拷锟斤拷锟斤拷锟斤拷 A锟斤拷锟斤拷锟叫猴拷 + Size=5 锟襟，帮拷 <c>bounds.min.y + orthoSize</c> Force 只锟斤拷 Y锟斤拷锟斤拷 <c>CameraTreeInArea</c> 锟阶边★拷
    /// 锟斤拷锟侥边斤拷锟绞诧拷锟斤拷锟斤拷锟斤拷锟斤拷 false锟斤拷锟斤拷 CameraArea + Size 7.9锟斤拷锟斤拷锟斤拷 ScreenY/Offset 锟斤拷锟斤拷锟斤拷
    /// 锟斤拷锟斤拷锟斤拷锟节讹拷锟斤拷锟斤拷同一 API锟斤拷<c>CheckPlayerHasInSpcArea</c>锟斤拷锟斤拷锟斤拷锟斤拷锟斤拷锟斤拷路锟斤拷
    /// </summary>
    public void ChangeCamera(bool isEnterTree, CameraComponentGSM cameraMgr)
    {
        if (storyLogic == null) { return; }
        
        var targetColliderArea = isEnterTree ? storyLogic.newCameraBoundingArea : storyLogic.oldCameraBoundingArea;
        var colliderArea = targetColliderArea.GetComponent<PolygonCollider2D>();
        cameraMgr.ChangeCameraBoundingArea(colliderArea);
        // 锟斤拷锟斤拷锟斤拷锟斤拷锟斤拷锟绞撅拷锟斤拷锟�?
        if (isEnterTree)
        {
            cameraMgr.ChangeVirtualCameraShowSize(5);
            // 锟斤拷锟斤拷锟斤拷锟阶ｏ拷DeadZoneHeight=1 锟斤拷锟斤拷 Y锟斤拷锟斤拷锟斤拷锟斤拷锟酵ｏ拷诤戏锟斤拷锟斤拷?锟斤拷 锟斤拷 头锟斤拷锟秸★拷锟斤拷止魔锟斤拷锟斤拷 -2.9锟斤拷
            cameraMgr.SnapLiveOrthoYToConfinerFloor(colliderArea);
        }
        else
        {
            cameraMgr.ResetVirtualCameraShowSize();
        }

        // 外壳淡出绑在切镜完成后（非 Interactive 靠近）：进洞透、出洞/切回不透明；读档洞内同源。
        // 时序：黑幕仍盖着时开始 DOFade（方案 A）；若要揭幕后才淡改 A+B。
        storyLogic.OuterSpriteFade(isEnterTree ? 0f : 1f);
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
        // 锟斤拷锟斤拷锟斤拷锟斤拷锟斤拷锟截癸拷原锟姐（OPEN Q3锟斤拷拽 MainCamera 锟斤拷 (0,0) 锟斤拷锟斤拷锟斤拷Brain 锟斤拷帧锟斤拷腔兀锟斤拷锟斤拷诓锟斤拷模锟�?
        GameActionMgr.runMoveToWorldPosAction(mainCamera, Vector2.zero, 0.1f).SetEase(Ease.Linear);

        // OPEN Q2锟斤拷锟斤拷锟斤拷 CameraAction 锟斤拷锟斤拷锟斤拷 MainCamera锟斤拷锟斤拷锟斤拷锟矫革拷 VCam锟斤拷停锟斤拷锟斤拷锟斤拷锟斤拷锟节讹拷锟斤拷锟斤拷锟斤拷一锟轿底边★拷
        if (playerIsInTreeBridge && storyLogic != null && storyLogic.newCameraBoundingArea != null)
        {
            var treeIn = storyLogic.newCameraBoundingArea.GetComponent<PolygonCollider2D>();
            if (treeIn != null)
            {
                cameraMgr.SnapLiveOrthoYToConfinerFloor(treeIn);
            }
        }
    }

    public void PlayTreeBridgeMoveSfx()
    {
        storyLogic.PlayTreeBridgeMoveSfx();
    }

    // 锟斤拷医锟斤拷锟斤拷锟竭筹拷锟斤拷锟斤拷
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

    // =============================锟芥档锟酵讹拷锟斤拷Start
    public override void ParseInternal(MasterGameData masterData)
    {
    }

    public override void SerializeInternal(MasterGameData masterData)
    {
    }

    //===============================锟芥档锟酵讹拷锟斤拷End
}
