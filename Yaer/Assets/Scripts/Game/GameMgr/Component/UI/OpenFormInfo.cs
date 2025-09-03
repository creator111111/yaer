namespace Game.GameMgr.Component.UI
{
    public class OpenFormInfo
    {
        public int formID;
        public EUIGroup group;
        public string uiAssetName;

        public OpenFormInfo(int formID, EUIGroup group, string uiAssetName)
        {
            this.formID = formID;
            this.group = group;
            this.uiAssetName = uiAssetName;
        }
    }
}