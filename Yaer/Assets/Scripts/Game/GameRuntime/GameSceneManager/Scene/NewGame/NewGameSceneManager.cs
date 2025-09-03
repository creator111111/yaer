using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.GameSceneManager.Base;
using Game.GameRuntime.GameSceneManager.Component;
using Game.GameRuntime.GameSceneManager.Component.Story;
using Game.GameRuntime.UI.FormLogic.Cartoon;
using Game.GameRuntime.UI.FormLogic.Story.Dialogue;
using Game.Static.Name.Res;
using Game.Static.Path;
using Game.Static.Path.Sound;

namespace Game.GameRuntime.GameSceneManager.Scene.NewGame
{
    public class NewGameSceneManager : BaseGameSceneManager
    {
        private NewGameCartoonFormLogic cartoonFormLogic;

        public override void OnInit()
        {
            base.OnInit();
            nowSceneName = SceneName.NewGameScene;
        }

        public override void OnEnterScene()
        {
            base.OnEnterScene();
            GetModule<InputComponentGSM>().SetAllowOpenMenu(false);
            // 漫画面板
            GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(UIPrefabPath.GetUIPrefabPath("NewGameCartoonPanel"), EUIGroup.Bottom, new OpenFormArgs()
            {
                callBack = logic =>
                {
                    if (logic is NewGameCartoonFormLogic cartoonFormLogic)
                    {
                        this.cartoonFormLogic = cartoonFormLogic;
                        
                        // 监听是否完成漫画
                        cartoonFormLogic.GetProxy<NewGameCartoonFormProxy>().onFinishEvent = () =>
                        {
                            // 打开剧情
                            GetModule<StoryComponentGSM>().TriggerStory("NewGameStory");
                            // 开始播放剧情音乐
                            GameManager.GetGMComponent<SoundComponentGM>().PlaySound(SoundType.BGM, "龙宫内BGM.ogg", true);
                        };
                    }
                }
            });
        }

        // public override void OnExitScene()
        // {
        //     base.OnExitScene();
        //     
        //     // 关闭漫画面板
        //     if (cartoonFormLogic.isActiveAndEnabled)
        //     {
        //         cartoonFormLogic.OnShutDown();
        //         cartoonFormLogic = null;
        //     }
        // }

        public override void OnShutDown()
        {
            base.OnShutDown();
            
            // 关闭漫画面板
            if (cartoonFormLogic != null && cartoonFormLogic.isActiveAndEnabled)
            {
                cartoonFormLogic.OnShutDown();
                cartoonFormLogic = null;
            }
        }

        public override void initAllSceneMonster()
        {

        }
    }
}