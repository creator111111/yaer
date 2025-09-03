using Game.GameMgr;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameRuntime.GameSceneManager.Component;
using Game.Static.Enum.Goods;

namespace GameDebug
{
    public class AddItemComponentGT: BaseGTComponent
    {
        protected override void OnInit()
        {
            base.OnInit();
        }

        public void AddItem(string itemName, int count)
        {
            GameManager.GetGameSceneManager().GetArchiveData<PlayerBagData>().AddMainItem(itemName, count);
        }
    }
}