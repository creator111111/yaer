using Game.GameMgr;
using Game.Static.Name.Settings;
using GameFramework.Localization;
using NodeCanvas.Framework;
using ParadoxNotion.Design;


namespace NodeCanvas.Tasks.Conditions
{

    [Category("用户自定义检测True或者False")]
    public class CheckBooleanCustom : ConditionTask
    {
        public BBParameter<string> storyName;
        public BBParameter<string> args;

        protected override string info {
            get { return storyName.value + " Customized Methods is True"; }
        }

        protected override bool OnCheck() {

            return CheckStoryEvent(storyName.value, args.value);
        }

        private bool CheckStoryEvent(string storyName, string args)
        {
            bool flag = false;
            var strList = args != null ? args.Split(':') : null;
            var storyArg_1 = (strList != null && strList.Length > 0) ? strList[0] : "";
            //var storyArg_2 = strList.Length > 1 ? strList[1] : "";
            // 有需要自动往下添加参数
            switch (storyName)
            {
                case "WoodBattle":
                    flag = !WoodWormRootBattleMgr.getInstance().hasKillAllWorm;// 虫巢事件中是否有虫子逃跑
                    break;
                case "IsAnyLanguage":
                    var curLanguage = GameManager.Instance.language;
                    flag = storyArg_1 == LanguageType.GetLanaguageString(curLanguage);
                    break;
                default:
                    break;
            }
            return flag;
        }
    }
}