using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;
using System.Collections.Generic;

namespace Game.GameMgr.Component.Archive.ArchiveDataClass
{
    /// <summary>
    /// 记录对话触发次数
    /// </summary>
    public class StoryTriggerCountData : BaseArchiveData
    {
        private Dictionary<string, int> StoryTriggerCount;
        public override void ParseInternal(MasterGameData masterData)
        {
            string dataStr = masterData.GetValue("StoryTriggerCountData_StoryTriggerCount", "");
            StoryTriggerCount = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, int>>(dataStr);
            if (StoryTriggerCount == null)
            {
                StoryTriggerCount = new Dictionary<string, int>();
            }
        }

        public override void SerializeInternal(MasterGameData masterData)
        {
            string dataStr = Newtonsoft.Json.JsonConvert.SerializeObject(StoryTriggerCount);
            masterData.SetValue("StoryTriggerCountData_StoryTriggerCount", dataStr);
        }
        /// <summary>
        /// 对话触发次数增加
        /// </summary>
        /// <param name="storyName"></param>
        public void OnStoryTriggered(string storyName)
        {
            if (string.IsNullOrEmpty(storyName)) return;
            if (StoryTriggerCount.ContainsKey(storyName))
            {
                StoryTriggerCount[storyName]++;
            }
            else
            {
                StoryTriggerCount.Add(storyName, 1);
            }
        }
        /// <summary>
        /// 获取对话触发次数
        /// </summary>
        /// <param name="storyName"></param>
        /// <returns></returns>
        public int GetStoryTriggerCount(string storyName)
        {
            if (StoryTriggerCount.TryGetValue(storyName, out int count))
            {
                return count;
            }
            return 0;
        }
        /// <summary>
        /// 返回对话是否触发过
        /// </summary>
        /// <param name="storyName"></param>
        /// <returns></returns>
        public bool CheckStoryUsed(string storyName)
        {
            return StoryTriggerCount.ContainsKey(storyName);
        }
    }
}

