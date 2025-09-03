using Game.GameMgr;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Scene;

namespace GameDebug
{
    public class ChangeClothesComponentGT: BaseGTComponent
    {
        public void SetTimesChangeClothesScene(int times)
        {
            GameManager.GetGameSceneManager().GetArchiveData<SelectClothesSceneData>().exitTimes = times;
        }
    }
}