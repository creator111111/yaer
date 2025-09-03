using System;
using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;

namespace Game.GameMgr.Component.Archive.ArchiveDataClass
{
    [Serializable]
    public class ArchiveInfo
    {
        public MasterGameData data = new MasterGameData();

        public int id = 1; // 存档槽id
        public string guid = Guid.NewGuid().ToString();
        public string name;
        public DateTime createTime;
        public float playTime;
        public int currentChapterIndex;
        public string currentSceneName;

        public string GetCreateTimeStr()
        {
            return createTime.ToString("yyyyMMdd_HHmmss_fff");
        }
        public void ParseInternal()
        {
            id = ES3.Deserialize<int>(data.GetValue<byte[]>("ArchiveInfo_id"));
            guid = ES3.Deserialize<string>(data.GetValue<byte[]>("ArchiveInfo_guid"));
            name = ES3.Deserialize<string>(data.GetValue<byte[]>("ArchiveInfo_name"));
            createTime = ES3.Deserialize<DateTime>(data.GetValue<byte[]>("ArchiveInfo_createTime"));
            playTime = ES3.Deserialize<float>(data.GetValue<byte[]>("ArchiveInfo_playTime"));
            currentChapterIndex = ES3.Deserialize<int>(data.GetValue<byte[]>("ArchiveInfo_currentChapterIndex"));
            currentSceneName = ES3.Deserialize<string>(data.GetValue<byte[]>("ArchiveInfo_currentSceneName"));
        }

        public void SerializeInternal()
        {
            data.SetValue("ArchiveInfo_id", ES3.Serialize(id));
            data.SetValue("ArchiveInfo_guid", ES3.Serialize(guid));
            data.SetValue("ArchiveInfo_name", ES3.Serialize(name));
            data.SetValue("ArchiveInfo_createTime", ES3.Serialize(createTime));
            data.SetValue("ArchiveInfo_playTime", ES3.Serialize(playTime));
            data.SetValue("ArchiveInfo_currentChapterIndex", ES3.Serialize(currentChapterIndex));
            data.SetValue("ArchiveInfo_currentSceneName", ES3.Serialize(currentSceneName));

            id = default;
            guid = default;
            name = default;
            createTime = default;
            playTime = default;
        }
    }
}