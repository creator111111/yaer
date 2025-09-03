using Game.GameMgr.Component.UI;
using Game.GameMgr;
using Game.Static.Path;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using Game.GameMgr.Component;
using Game.Static.Path.Sound;

namespace Game.GameRuntime.Story.Node
{
    [Category("Common")]
    [Name("场景播放背景音乐BGM或者SFX")]
    // 用于剧情对话系统中的事件处理
    public class ScenePlayBgmAciton : ActionTask
    {
        public BBParameter<string> bgmPath; // 音乐名称路径
        public BBParameter<SoundType> soundType = SoundType.BGM;
        public BBParameter<float> fadeOutTime = 0.7f; // 淡出时间
        public BBParameter<float> fadeInTime = 0f; // 淡入时间
        public BBParameter<bool> isLoop = true; // 是否循环
        public BBParameter<float> volume = -1f; // 音量，-1表示使用当前设置中的音量

        protected override string OnInit()
        {
            
            return base.OnInit();
        }

        protected override string info { 
            get
            {
                return "播放" + bgmPath.value;
            }
        }

        protected override void OnExecute()
        {

            GameManager.GetGMComponent<SoundComponentGM>().PlaySound(soundType.value, bgmPath.value, isLoop.value,
                fadeOutTime.value, fadeInTime.value, volume.value);

            EndAction();
        }
    }
}