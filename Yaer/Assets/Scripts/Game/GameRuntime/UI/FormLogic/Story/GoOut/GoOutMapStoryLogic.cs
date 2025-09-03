using Cysharp.Threading.Tasks;
using Game.GameMgr;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Scene;
using Game.GameRuntime.Entities.Component.Anima;
using Game.GameRuntime.GameSceneManager.Component;
using Game.Static.Enum.Goods;
using Game.Static.Enum.Map;
using Game.Static.Name.Res;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.GameRuntime.UI.FormLogic.Story.GoOut
{
    public class GoOutMapStoryLogic : MonoBehaviour
    {
        [SerializeField] private GoOutMapStoryMapFormLogic mapForm;
        [SerializeField] private Animator animator;

        private void Awake()
        {
            mapForm.OnInit(GameManager.GetGameSceneManager().GetArchiveData<PlayerMapData>());
        }

        private void Start()
        {
            animator.Rebind();
            /*
            componentSystemUI.GetComponent<BlackFadeComponent>().HideRow();
            dialogueForm.SetPause(true);*/
        }

/*        protected internal override void OnClose(bool isShutdown, object userData)
        {
            base.OnClose(isShutdown, userData);

            mapForm.OnClose(isShutdown, userData);
        }*/

        public void OnGetMapTips()
        {
            _OnGetMapTips().Forget();
        }

        private async UniTask _OnGetMapTips()
        {
            await UniTask.Delay(2000);
            GameManager.GetGameSceneManager().GetModule<TipsComponentGSM>().OpenTipsForm("GetMap");
        }

        public void OnHomeScene1_SetSignToHome()
        {
            mapForm.SetSign(PlaceName.Home);
        }

        public void OnHomeScene1_SetSignToCity()
        {
            mapForm.SetSign(PlaceName.AoGuShiCity);
        }

        public void OnHomeScene1_GotoForest()
        {
            var SceneManager = GameManager.GetGameSceneManager();
            SceneManager.GetArchiveData<PlayerBagData>().AddMainItem(EMainItemName.Map, 1);
            SceneManager.GetArchiveData<HomeScene1Data>().getMap = true;

            // 跳转森林
            SceneManager.GetModule<LoadSceneComponentGSM>().LoadScene(SceneName.ForestScene);
        }
    }
}