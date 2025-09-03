//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using Game.GameMgr;
using Game.GameMgr.Component;
using Game.Static.Enum;
using GameFramework.UnityRuntime.UI;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace Game.GameRuntime.Procedure
{
    public class ProcedureMenu : ProcedureBase
    {
        public override bool UseNativeDialog => false;

        private bool startGame;
        private bool startNewGame;
        private EGameHard gameHard;
        private UIForm startForm;
        private ProcedureComponentGM procedureComponentGM;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            if (procedureComponentGM is null)
            {
                procedureComponentGM = GameManager.GetGMComponent<ProcedureComponentGM>();
            }
            
            procedureComponentGM.onStartGameAction += StartGame;
            procedureComponentGM.OpenMainMenu();
        }

        private void StartGame()
        {
            startGame = true;
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (startGame)
            {
                ChangeState<ProcedureGame>(procedureOwner);
                startGame = false;
            }
        }

        protected override void OnLeave(ProcedureOwner procedureOwner, bool isShutdown)
        {
            base.OnLeave(procedureOwner, isShutdown);
            // 监听是否从存档加载
            procedureComponentGM.onStartGameAction -= StartGame;
        }
    }
}