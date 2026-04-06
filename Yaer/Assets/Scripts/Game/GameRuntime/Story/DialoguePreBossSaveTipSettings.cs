using UnityEngine;

namespace Game.GameRuntime.Story
{
    /// <summary>
    /// 挂在任意常驻场景物体上（例如场景根空物体），配置「哪段剧情、第几句字幕」弹出保存提示。
    /// 句号从 1 开始，与当前对话中字幕出现顺序一致。
    /// </summary>
    public class DialoguePreBossSaveTipSettings : MonoBehaviour
    {
        public static DialoguePreBossSaveTipSettings Instance { get; private set; }

        [Tooltip("要匹配的剧情预制体名，与 TriggerStory / StoryPrefabName 一致")]
        public string targetStoryName = "WestRappRoadGoblinAndGusha";

        [Tooltip("从 1 开始，等于该段对话中要打断的那一句（通常为最后一句 = 字幕总句数）。为 0 表示不启用。")]
        public int pauseAtSubtitleLineIndex = 0;

        [Tooltip("可选：非空时还要求本句文本包含该子串（防止句号数错）")]
        public string alsoRequireLineContains = "";

        [Tooltip("提示界面资源名（Assets/GameRes/Prefabs/UI/{name}.prefab）")]
        public string tipsPanelPrefabName = "SystemTipsPanel2";

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
