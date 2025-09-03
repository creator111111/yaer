using Game.GameMgr.Component.Archive.ArchiveDataClass.Scene;
using Game.GameRuntime.Entities.Component.Interactive;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameRuntime.Entities.SceneEntities.ForestScene
{
    public class ForestSceneRabbitStory : SimpleStoryTrigger
    {
        [SerializeField]
        public Transform SelectionPos;
        private ForestSceneData ArchiveData;

        public bool rabbitFirstDialogue
        {
            get => ArchiveData.rabbitFirstDialogue;
            set => ArchiveData.rabbitFirstDialogue = value;
        }
        public bool chooseTakeRabbit
        {
            get => ArchiveData.chooseTakeRabbit;
            set => ArchiveData.chooseTakeRabbit = value;
        }

        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);
            ArchiveData = SceneManager.GetArchiveData<ForestSceneData>();
            if (!rabbitFirstDialogue && chooseTakeRabbit)
            {
                Destroy(this.gameObject);
            }
        }
    }
}