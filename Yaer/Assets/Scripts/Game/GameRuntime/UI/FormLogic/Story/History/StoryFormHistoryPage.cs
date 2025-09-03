using System;
using System.Collections.Generic;
using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameRuntime.GameSceneManager.Component;
using Game.GameRuntime.UI.FormLogic.Story.Base.Control;
using UnityEngine;
using UnityEngine.UI;

namespace Game.GameRuntime.UI.FormLogic.Story.Base
{
    public class StoryFormHistoryPage : MonoBehaviour
    {
        [SerializeField] private HistoryDialogueBox prefab;
        [SerializeField] private ScrollRect sv;
        private readonly List<HistoryDialogueBox> boxes = new List<HistoryDialogueBox>();
        private bool open;

        private void Awake()
        {
            open = true;
            sv = transform.Find("Scroll View").GetComponent<ScrollRect>();
        }

        public void Update()
        {
            // esc失活
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                gameObject.SetActive(false);
            }

            if (Math.Abs(sv.verticalNormalizedPosition - 0) > 0.1f && open)
                sv.verticalNormalizedPosition = 0;
            else
                open = false;
        }


        private void OnEnable()
        {
            open = true;
            // 暂时关闭esc打开菜单功能
            GameManager.GetGameSceneManager().GetModule<InputComponentGSM>().SetAllowOpenMenu(false);
        }

        private void OnDisable()
        {
            GameManager.GetGameSceneManager().GetModule<InputComponentGSM>().SetAllowOpenMenu(true);
        }

        public void UpdateDialogue(List<HistoryDialogueInfo> infos)
        {
            if (prefab == null) return;

            foreach (var box in boxes) Destroy(box.gameObject);
            boxes.Clear();

            foreach (var info in infos)
            {
                var box = Instantiate(prefab, prefab.transform.parent).GetComponent<HistoryDialogueBox>();
                box.UpdateDialogue(info);
                box.gameObject.SetActive(true);
                boxes.Add(box);
            }

            sv.verticalNormalizedPosition = 0;
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }
    }
}