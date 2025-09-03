using Game.GameMgr.Component.Archive.ArchiveDataClass;
using Game.GameRuntime.Entities.Component.Interactive;
using Game.GameRuntime.Entities.Component.Map;
using Game.GameRuntime.GameSceneManager.Component.Story;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameRuntime.Entities.SceneEntities.ForestEastScene
{
    public class VerdantCorridorDoor : SceneChangeDoor
    {
        [SerializeField]
        private string FirstEnterStoryName;
        private bool FirstEnterDialogueUsed
        {
            get => SceneManager.GetArchiveData<StoryTriggerCountData>().CheckStoryUsed(FirstEnterStoryName);
        }

        private StoryComponentGSM storyComponentGSM;

        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);
            storyComponentGSM = SceneManager.GetModule<StoryComponentGSM>();
        }

        protected override void EnterDoor(InteractiveComponent component)
        {
            if (!FirstEnterDialogueUsed)
            {
                storyComponentGSM.TriggerStory(FirstEnterStoryName);
            }
            else
            {
                base.EnterDoor(component);
            }
        }
    }
}

