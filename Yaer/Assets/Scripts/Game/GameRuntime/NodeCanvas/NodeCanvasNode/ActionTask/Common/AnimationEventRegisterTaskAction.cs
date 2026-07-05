using Game.GameRuntime.Entities.Component.Anima;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace Game.GameRuntime.Story.Node
{
    [Category("Animation")]
    // 此 Name 为 NodeCanvas 节点在图中显示的标题，须为合法 UTF-8 中文，否则在编辑器中会变成 � 或乱码（此前为错误编码文件所致）。
    [Name("等待Animation事件")]
    public class AnimationEventRegisterTaskAction : ActionTask
    {
        public BBParameter<AnimationEventComponent> animationEventComponent;
        public BBParameter<string> eventName;

        protected override void OnExecute()
        {
            if (animationEventComponent.value == null)
            {
                Debug.LogError("animationEventComponent.value == null");
            }
            var ev = eventName.value;
            var comp = animationEventComponent.value;
            comp.RegisterEvent(ev, s =>
            {
                // 收到事件时取消注册、结束本 Action；需与 AnimaEventTrigger/ForestSceneLinEnStory.TryNotify 中发出的事件名一致，才能从「等待」进入下一节点。
                Debug.Log(
                    $"[NodeCanvas/等待Animation事件] 收到事件: \"{ev}\" -> 本等待节点完成，可继续执行对话图  (component={comp.gameObject.name})",
                    comp);
                EndAction();
            });
        }

        // NodeCanvas 在图中展开节点时从 info 取黑色提示行，勿写入损坏编码的中文字符。
        protected override string info
        {
            get
            {
                return string.Format("等待{0}事件: {1}", animationEventComponent, eventName);
            }
        }
    }
}
