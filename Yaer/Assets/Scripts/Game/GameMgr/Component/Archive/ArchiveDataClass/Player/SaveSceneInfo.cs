namespace Game.GameMgr.Component.Archive.ArchiveDataClass.Player
{
    public class SaveSceneInfo
    {
        public SaveSceneInfo()
        {
        }

        public SaveSceneInfo(float savePosX, float savePosY, string sceneName)
        {
            SavePosX = savePosX;
            SavePosY = savePosY;
            SceneName = sceneName;
        }

        public float SavePosX { get; }

        public float SavePosY { get; }

        public string SceneName { get; }
    }
}