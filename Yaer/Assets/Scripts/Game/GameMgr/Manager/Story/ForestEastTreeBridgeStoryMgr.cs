using DG.Tweening;
using Cinemachine;
using Game.GameMgr;
using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.Entities.SceneEntities.ForestEastScene;
using Game.GameRuntime.GameSceneManager.Base;
using Game.GameRuntime.GameSceneManager.Component.CameraGSM;
using UnityEngine;

// 倒树 / 树洞剧情管理
public class ForestEastTreeBridgeStoryMgr : BaseSceneStoryMgr
{
    public TreeBridgeLogic storyLogic;
    public bool playerIsInTreeBridge;
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

    /// <summary>爬行晃动改抖 Framing TrackedObjectOffset.y 时记录的基线，Stop 必须还原。</summary>
    private float _climbShakeOffsetBaseY;

    /// <summary>当前参与晃动的 Framing；Stop / 缺引用时清空。</summary>
    private CinemachineFramingTransposer _climbShakeFraming;

    /// <summary>晃动幅度（世界单位）；与旧 DOMove ±0.3 手感对齐。</summary>
    private const float ClimbCameraShakeAmplitude = 0.3f;

    /// <summary>单程晃动时长（秒）。</summary>
    private const float ClimbCameraShakeHalfDuration = 0.3f;

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
        if (sceneMgr == null)
        {
            return;
        }

        var cameraMgr = sceneMgr.GetModule<CameraComponentGSM>();
        if (cameraMgr == null || cameraMgr.CameraComponent == null)
        {
            return;
        }

        // 0922 上漂：禁止再 DOMove Camera 根（会连带 Confiner 上移）。
        // 改抖 FramingTransposer.TrackedObjectOffset.y —— 只抬构图，不动盒几何。
        // 替代（否决）：抖 MainCamera 本地 —— Brain 每帧覆盖；恢复 Stop→(0,0) —— 出洞闪滑回潮。
        var vcam = cameraMgr.CameraComponent.VirtualCamera;
        var framing = vcam != null
            ? vcam.GetCinemachineComponent<CinemachineFramingTransposer>()
            : null;
        if (framing == null)
        {
            Debug.LogWarning("[TreeBridgeCam] CameraAction：无 FramingTransposer，跳过爬行晃动（避免误抖 Camera 根）。");
            return;
        }

        // 若历史残留把根抬高了，开晃前先清 Y（只 local Y）
        cameraMgr.CameraComponent.ResetCameraRigLocalY();

        cameraTween?.Kill(true);
        RestoreClimbShakeFramingOffset();

        _climbShakeFraming = framing;
        _climbShakeOffsetBaseY = framing.m_TrackedObjectOffset.y;
        float baseY = _climbShakeOffsetBaseY;
        float peakY = baseY + ClimbCameraShakeAmplitude;

        PlayTreeBridgeMoveSfx();

        var up = DOTween.To(
                () => _climbShakeFraming != null ? _climbShakeFraming.m_TrackedObjectOffset.y : baseY,
                y => SetClimbShakeOffsetY(y),
                peakY,
                ClimbCameraShakeHalfDuration)
            .SetEase(Ease.Linear);

        var down = DOTween.To(
                () => _climbShakeFraming != null ? _climbShakeFraming.m_TrackedObjectOffset.y : baseY,
                y => SetClimbShakeOffsetY(y),
                baseY,
                ClimbCameraShakeHalfDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() => { PlayTreeBridgeMoveSfx(); });

        cameraTween = DOTween.Sequence()
            .Append(up)
            .Append(down)
            .SetLoops(-1)
            .SetAutoKill(false);
    }

    public void StopCameraAction()
    {
        var sceneMgr = GameManager.GetGameSceneManager() as BaseGameSceneManager;
        if (sceneMgr == null)
        {
            return;
        }

        var cameraMgr = sceneMgr.GetModule<CameraComponentGSM>();
        if (cameraMgr == null || cameraMgr.CameraComponent == null)
        {
            return;
        }

        cameraTween?.Kill(true);
        cameraTween = null;

        // 还原 Framing 晃动偏移（方案 B）
        RestoreClimbShakeFramingOffset();

        // 方案 A：仅清 Camera 根 local Y；禁止全轴 →(0,0)
        cameraMgr.CameraComponent.ResetCameraRigLocalY();
        cameraMgr.CameraComponent.transform.DOKill(true);

        // 停晃后若仍在洞内，再贴一次底边（OPEN Q2 / 0914）
        if (playerIsInTreeBridge && storyLogic != null && storyLogic.newCameraBoundingArea != null)
        {
            var treeIn = storyLogic.newCameraBoundingArea.GetComponent<PolygonCollider2D>();
            if (treeIn != null)
            {
                cameraMgr.SnapLiveOrthoYToConfinerFloor(treeIn);
            }
        }
    }

    /// <summary>写入 Framing TrackedObjectOffset.y（仅 Y）。</summary>
    private void SetClimbShakeOffsetY(float y)
    {
        if (_climbShakeFraming == null)
        {
            return;
        }

        var o = _climbShakeFraming.m_TrackedObjectOffset;
        o.y = y;
        _climbShakeFraming.m_TrackedObjectOffset = o;
    }

    /// <summary>Stop / 重开晃动前：把 Offset.y 收回归档基线。</summary>
    private void RestoreClimbShakeFramingOffset()
    {
        if (_climbShakeFraming == null)
        {
            return;
        }

        SetClimbShakeOffsetY(_climbShakeOffsetBaseY);
        _climbShakeFraming = null;
    }

    public void PlayTreeBridgeMoveSfx()
    {
        if (storyLogic != null)
        {
            storyLogic.PlayTreeBridgeMoveSfx();
        }
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
