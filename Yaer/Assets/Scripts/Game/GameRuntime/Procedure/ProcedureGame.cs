//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameRuntime.GameMode;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace Game.GameRuntime.Procedure
{
    public class ProcedureGame : ProcedureBase
    {
        public override bool UseNativeDialog => false;

        private BaseGameMode gameMode;
        private ProcedureOwner procedureOwner;
        private ProcedureComponentGM procedureComponentGM;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);

            this.procedureOwner = procedureOwner;

            procedureComponentGM = GameManager.GetGMComponent<ProcedureComponentGM>();
            procedureComponentGM.onStartGameAction += StartGame;
            procedureComponentGM.onReturnToMenuAction += ReturnToMenu;
        }

        private void ReturnToMenu()
        {
            ChangeState<ProcedureMenu>(procedureOwner);
        }

        private void StartGame()
        {
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (!procedureComponentGM.Pause)
            {
                gameMode?.Update(elapseSeconds, realElapseSeconds);
            }
        }
    }
}