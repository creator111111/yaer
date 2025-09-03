using Game.GameMgr;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameRuntime.Entities.Monster;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.GameSceneManager.Base;
using Game.Static.Enum.Goods;
using System;
using UnityEngine;

// 掉落物脚本
public class DropItemSrc : MonoBehaviour
{ 
    public Collider2D colliderObj;
    public BaseMonster monsterLogic;

    public EMainItemName itemName; // 掉落物类别
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (colliderObj.OverlapPoint(mousePosition))
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (!CheckHasCollisonWithPlayer()) { return; }
                if (monsterLogic.GetDropItemHasPickUp()) { return; }
                
                monsterLogic.SetDropItemHasPickUp();
                // 人物添加掉落物
                var sceneMgr = GameManager.GetGameSceneManager() as BaseGameSceneManager;
                if (sceneMgr != null)
                {
                    var playerEntity = sceneMgr.GetPlayerEntity();
                    var playerLogic = playerEntity.Logic as PlayerLogic;
                    playerLogic.isEnableNorAtk = false; // 捡道具时短暂设置不能普通攻击
                }
                
                monsterLogic.SceneManager.GetArchiveData<PlayerBagData>().AddMainItem(itemName);
                //monsterLogic.SceneManager.GetModule<TipsComponentGSM>().OpenTipsForm("GetMpBall");
                // 设置一个淡出拾取掉落物
                var fadeAct = GameActionMgr.runFadeActionSpriteRender(gameObject, 0, 1f);
                if (fadeAct == null) { return; }
                fadeAct.onComplete = () =>
                {
                    // 掉落物设置消失
                    monsterLogic.RemoveDropItem();
                    var sceneMgr = GameManager.GetGameSceneManager() as BaseGameSceneManager;
                    if (sceneMgr != null)
                    {
                        var playerEntity = sceneMgr.GetPlayerEntity();
                        var playerLogic = playerEntity.Logic as PlayerLogic;
                        playerLogic.isEnableNorAtk = true; // 恢复可以普通攻击
                    }
                };
            }
        }
    }
    // 检测怪物是否和玩家相接触
    bool CheckHasCollisonWithPlayer()
    {
        var playerEntity = monsterLogic.SceneManager.GetPlayerEntity();
        if (playerEntity != null && playerEntity.Logic is PlayerLogic playerLogic)
        {
            var posDistance = Math.Abs(playerLogic.gameObject.transform.position.x - monsterLogic.gameObject.transform.position.x);
            if (posDistance <= 4)
            {
                return true;
            }
        }
        //if (!playerCollider) { return false; }
        //monsterLogic.bodyCld.enabled = true;
        //var result = monsterLogic.bodyCld.bounds.Intersects(playerCollider.bounds);
        return false;
    }
}
