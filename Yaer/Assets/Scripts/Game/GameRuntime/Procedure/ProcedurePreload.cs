//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.ChangeScene;
using Game.GameMgr.Component.UI;
using Game.GameRuntime.UI.FormLogic.Init;
using Game.Static.Name.Res;
using Game.Static.Path;
using GameFramework.UnityRuntime.UI;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace Game.GameRuntime.Procedure
{
    public class ProcedurePreload : ProcedureBase
    {
        public override bool UseNativeDialog => false;

        private bool isPreloadEnd;
        private bool formDone;
        private ProcedureOwner procedureOwner;
        private InitFormLogic initFormLogic;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);
            
            this.procedureOwner = procedureOwner;

            // 打开初始化面板
            GameManager.GetGMComponent<UIComponentGM>().OpenUIForm(UIPrefabPath.InitPanel, EUIGroup.System, new OpenFormArgs()
            {
                callBack = logic =>
                {
                    if (logic is InitFormLogic initFormLogic)
                    {
                        this.initFormLogic = initFormLogic;
                        // 初始化面板加载完成
                        initFormLogic.GetProxy<InitFormProxy>().onHideEnd = () =>
                        {
                            formDone = true;
                        };
                    }
                }
            });

            // 预加载资源
            isPreloadEnd = true;
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            // 加载结束开始淡入黑幕
            if (isPreloadEnd && formDone)
            {
                initFormLogic.FadeCloseForm(() =>
                {
                    // 切换场景
                    GameManager.GetGMComponent<ChangeSceneComponentGM>().LoadScene(new LoadSceneArgs()
                    {
                        sceneName = SceneName.StartScene,
                        callBack = () => { ChangeState<ProcedureMenu>(procedureOwner); } // 切换到主菜单
                    });
                });
                
                formDone = false;
                isPreloadEnd = false;
            }
        }
    }
}