using Game.GameMgr.Component.Archive.ArchiveDataClass.Scene;

namespace Game.GameRuntime.Entities.SceneEntities.VerdantCorridor
{
    public class GushaNacklaceStoryTrigger : SimpleStoryTrigger
    {
        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);
            var pick = SceneManager.GetArchiveData<VerdantCorridorData>().PickGushaNacklace;
            if (pick)
            {
                Destroy(this.gameObject);
            }
        }
    }
}

