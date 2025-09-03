//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using Game.GameMgr;
using GameFramework.UnityRuntime.Base;
using GameFramework.UnityRuntime.Resource;
using System;
using ProcedureOwner = GameFramework.Fsm.IFsm<GameFramework.Procedure.IProcedureManager>;

namespace Game.GameRuntime.Procedure
{
    public class ProcedureLaunch : ProcedureBase
    {
        public override bool UseNativeDialog => true;

        private bool finishInitResource;

        protected override void OnEnter(ProcedureOwner procedureOwner)
        {
            base.OnEnter(procedureOwner);


            // 语言配置：设置当前使用的语言，如果不设置，则默认使用操作系统语言
            InitLanguageSettings();

            // 变体配置：根据使用的语言，通知底层加载对应的资源变体
            InitCurrentVariant();

            // 声音配置：根据用户配置数据，设置即将使用的声音选项
            InitSoundSettings();

            finishInitResource = false;
            try
            {
                GameEntry.GetComponent<ResourceComponent>().InitResources(OnFinishInitResource);
            }
            catch (NotSupportedException)
            {
                OnFinishInitResource();
            }
        }

        private void OnFinishInitResource()
        {
            GameManager.Instance.OnInit();
            finishInitResource = true;
        }

        protected override void OnUpdate(ProcedureOwner procedureOwner, float elapseSeconds, float realElapseSeconds)
        {
            base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);

            if (finishInitResource)
            {
                GameManager.Instance.OnEnter();
                // 进入资源加载
                ChangeState<ProcedurePreload>(procedureOwner);
            }
        }

        private void InitLanguageSettings()
        {

        }

        private void InitCurrentVariant()
        {

        }

        private void InitSoundSettings()
        {

        }
    }
}