using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.Entities.Player.Components;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic.Fighting
{
    public class FightingCanUsed : MonoBehaviour
    {

        #region 属性

        [SerializeField] private List<Image> images;
        [SerializeField] private List<GameObject> textItemCounts;

        [SerializeField] int maxUseItemCount = 6; // 最大可使用道具数量
        Dictionary<KeyCode, int> keyToItemIndexData = new Dictionary<KeyCode, int>() {
            {KeyCode.Alpha1, 0}, {KeyCode.Alpha2, 1}, {KeyCode.Alpha3, 2}, {KeyCode.Alpha4, 3}, {KeyCode.Alpha5, 4}, {KeyCode.Alpha6, 5},
        };
        bool hasUpdateItemArea = false;
        #region 引用

        private PlayerLogic PlayerLogic => GameManager.GetGMComponent<EntityComponentGM>().GetEntityLogic<PlayerLogic>();
        private PlayerInputComponent InputComponent => PlayerLogic.componentSystem.GetComponent<PlayerInputComponent>();
        private PlayerBagData PlayerBagData
        {
            get
            {
                if (GameManager.GetGameSceneManager() != null)
                {
                    return GameManager.GetGameSceneManager().GetArchiveData<PlayerBagData>();
                }
                else { return null; }
            }
        }

        #endregion

        #endregion

        #region 生命周期

        private void Start()
        {
            // 游戏打包后需要注释该方法
            // 同步当前存档的道具数据为配置表数据
            //PlayerBagData.RefreshMainItemDataInTest();
        }

        private void OnEnable()
        {
            PlayerBagData.OnDataChange += OnDataChange;
            OnDataChange(PlayerBagData);
        }

        private void OnDisable()
        {
            PlayerBagData.OnDataChange -= OnDataChange;
        }

        private void Update()
        {
            if (!hasUpdateItemArea)
            {
                OnDataChange(PlayerBagData);
            }
            if (PlayerLogic == null) { return; }
            if (!PlayerLogic.isEnableQuickUseItem) { return; }
            foreach (var data in keyToItemIndexData)
            {
                var keyCode = data.Key;
                var itemIndex = data.Value;
                if (Input.GetKeyDown(keyCode))
                {
                    // 按下对应的数字键则使用对应位置的道具
                    ShowItemEffect(itemIndex);
                }
            }
        }

        #endregion

        #region 方法

        private void OnDataChange(PlayerBagData data)
        {
            if (data == null) { return; }
            hasUpdateItemArea = true;
            for (int i = 0; i < data.quickItem.Length; i++)
            {
                if (!string.IsNullOrEmpty(data.quickItem[i]) && data.HasMainItem(data.quickItem[i]))
                {
                    images[i].sprite = data.GetMainItem(data.quickItem[i]).icon;
                    images[i].color = Color.white;
                    // 刷新道具数量
                    var itemCount = data.GetMainItemCount(data.quickItem[i]);
                    if (!textItemCounts[i].activeSelf) { textItemCounts[i].SetActive(true); }
                    GameTools.setTMPUGUIText(textItemCounts[i], itemCount.ToString());
                }
                else
                {
                    images[i].color = Color.clear;
                    textItemCounts[i].SetActive(false);
                }
            }
        }

        private void ShowItemEffect(int itemIndex)
        {
            if (PlayerBagData == null) { return; }
            var itemName = PlayerBagData.quickItem.Length > itemIndex ? PlayerBagData.quickItem[itemIndex] : "";
            if (itemName == "" || itemName == null)
            {
                Debug.LogWarning("=====================道具不存在:" + itemName + ",下标:"+ itemIndex);
                return;
            }
            var itemData = PlayerBagData.GetMainItem(itemName);
            if (itemData == null) { return; }
            if (itemData.num <= 0)
            {
                Debug.Log("===================道具数量不足:" + itemName);
                return;
            }
            ItemEffectDataMgr.getInstance().UseItem(itemName);
        }
        #endregion
    }
}