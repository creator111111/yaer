using Game.GameRuntime.Entities.Component.Anima;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameRuntime.Story.Node
{
    [Category("Animation")]
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
            animationEventComponent.value.RegisterEvent(eventName.value, s =>
            {
                EndAction();
            });
        }

        protected override string info
        {
            get
            {
                return string.Format("等待{0}事件: {1}", animationEventComponent, eventName);
            }
        }
    }
}