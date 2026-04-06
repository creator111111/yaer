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

        #region ????

        [SerializeField] private List<Image> images;
        [SerializeField] private List<GameObject> textItemCounts;

        [SerializeField] int maxUseItemCount = 6; // ??????????????
        Dictionary<KeyCode, int> keyToItemIndexData = new Dictionary<KeyCode, int>() {
            {KeyCode.Alpha1, 0}, {KeyCode.Alpha2, 1}, {KeyCode.Alpha3, 2}, {KeyCode.Alpha4, 3}, {KeyCode.Alpha5, 4}, {KeyCode.Alpha6, 5},
        };
        #region ????

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

        #region ????????

        private void Start()
        {
            // ???????????????????
            // ????????????????????????????
            //PlayerBagData.RefreshMainItemDataInTest();
        }

        private void OnEnable()
        {
            var bagData = PlayerBagData;
            if (bagData == null) { return; }
            PlayerBagData.OnDataChange += OnDataChange;
            var procedureComp = GameManager.GetGMComponent<ProcedureComponentGM>();
            if (procedureComp != null)
            {
                procedureComp.onCompleteLoadingSceneEvent -= RefreshAfterLoadArchive;
                procedureComp.onCompleteLoadingSceneEvent += RefreshAfterLoadArchive;
            }
            OnDataChange(bagData);
        }

        private void OnDisable()
        {
            var procedureComp = GameManager.GetGMComponent<ProcedureComponentGM>();
            if (procedureComp != null)
            {
                procedureComp.onCompleteLoadingSceneEvent -= RefreshAfterLoadArchive;
            }
            // OnDataChange 为 PlayerBagData 的静态事件，取消订阅不依赖 SceneManager；若仅在 GetGameSceneManager()==null 时 return，会导致死亡/切场景时未 -=，下次 OnEnable 重复订阅，读档后快捷栏表现异常。
            PlayerBagData.OnDataChange -= OnDataChange;
        }

        private void Update()
        {
            OnDataChange(PlayerBagData);
            if (PlayerLogic == null) { return; }
            if (!PlayerLogic.isEnableQuickUseItem) { return; }
            foreach (var data in keyToItemIndexData)
            {
                var keyCode = data.Key;
                var itemIndex = data.Value;
                if (Input.GetKeyDown(keyCode))
                {
                    // ????????????????????????????
                    ShowItemEffect(itemIndex);
                }
            }
        }

        #endregion

        #region ????

        private void OnDataChange(PlayerBagData data)
        {
            if (data == null) { return; }
            for (int i = 0; i < data.quickItem.Length; i++)
            {
                if (!string.IsNullOrEmpty(data.quickItem[i]) && data.HasMainItem(data.quickItem[i]))
                {
                    images[i].sprite = data.GetMainItem(data.quickItem[i]).icon;
                    images[i].color = Color.white;
                    // ??????????
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
                Debug.LogWarning("=====================?????????:" + itemName + ",????:"+ itemIndex);
                return;
            }
            var itemData = PlayerBagData.GetMainItem(itemName);
            if (itemData == null) { return; }
            if (itemData.num <= 0)
            {
                Debug.Log("===================????????????:" + itemName);
                return;
            }
            ItemEffectDataMgr.getInstance().UseItem(itemName);
        }

        private void RefreshAfterLoadArchive()
        {
            OnDataChange(PlayerBagData);
        }
        #endregion
    }
}