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
    /// 进/出洞：切换边界与 OrthoSize。
    /// 进洞：Size=5 后按 bounds.min.y + orthoSize Force 只改 Y，贴 CameraTreeInArea 底边。
    /// 出洞：Reset Size 7.9，还原 CameraArea。
    /// 读档洞内同一 API（CheckPlayerHasInSpcArea）。
    /// 揭幕前须再调 <see cref="AlignCameraAfterTreeBridgeChange"/>（策略 T）。
    /// </summary>
    public void ChangeCamera(bool isEnterTree, CameraComponentGSM cameraMgr)
    {
        if (storyLogic == null) { return; }
        
        var targetColliderArea = isEnterTree ? storyLogic.newCameraBoundingArea : storyLogic.oldCameraBoundingArea;
        var colliderArea = targetColliderArea.GetComponent<PolygonCollider2D>();
        cameraMgr.ChangeCameraBoundingArea(colliderArea);
        if (isEnterTree)
        {
            cameraMgr.ChangeVirtualCameraShowSize(5);
            // 进洞贴底：DeadZoneHeight=1 不跟 Y；禁止魔法数 -2.9
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

    /// <summary>
    /// 策略 T（0922）：ChangeCamera 后、揭幕前全轴贴玩家。
    /// 东郊已 smoothTime=0，SetFollow(forceSnap) 当帧对齐；进洞再 Snap Y 保留 0914 贴底。
    /// 不关洞内 CameraAction 爬行晃动；不挪 CameraTreeInArea。
    /// </summary>
    /// <remarks>
    /// 原因：只 Snap Y + 立刻 CloseFormFade 时，CM Framing 多帧追 X → 闪/滑；与进场 smoothTime 无关。
    /// 替代：拉长黑幕 hold —— 契约面大，否决作主修。
    /// </remarks>
    public void AlignCameraAfterTreeBridgeChange(
        bool isEnterTree,
        CameraComponentGSM cameraMgr,
        Transform playerRoot)
    {
        if (cameraMgr == null || playerRoot == null)
        {
            return;
        }

        cameraMgr.SetFollow(playerRoot);
        if (isEnterTree && storyLogic != null && storyLogic.newCameraBoundingArea != null)
        {
            var treeIn = storyLogic.newCameraBoundingArea.GetComponent<PolygonCollider2D>();
            if (treeIn != null)
            {
                cameraMgr.SnapLiveOrthoYToConfinerFloor(treeIn);
            }
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
        cameraTween.Kill(true);
        mainCamera.transform.DOKill(true);
        // 0922：禁止拽 MainCamera→(0,0)。旧写法与 Cinemachine Brain 抢位，出洞揭幕加重闪滑。
        // 替代：只 Kill 爬行晃动 Tween，机位留给后续 ChangeCamera + Align。

        // 停晃后若仍在洞内，再贴一次底边（OPEN Q2）
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
