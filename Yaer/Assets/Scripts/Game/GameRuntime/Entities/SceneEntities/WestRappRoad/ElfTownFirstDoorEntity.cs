using Game.GameMgr.Component;
using Game.GameMgr;
using Game.GameRuntime.Entities.Base.BaseSceneObj;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.GameSceneManager.Component;
using UnityEngine;

namespace Game.GameRuntime.Entities.SceneEntities.WestRappRoad
{
    public class ElfTownFirstDoorEntity : BaseSceneEntityLogic
    {
        [SerializeField]
        private float OpenDoorDistance;

        private bool OpenDoor;

        private SceneEntityComponentGSM sceneEntityComponentGSM;
        private Animator animator;
        private PlayerLogic player;

        protected internal override void OnInit(object userData)
        {
            base.OnInit(userData);
            animator = GetComponent<Animator>();
            sceneEntityComponentGSM = userData as SceneEntityComponentGSM;
        }

        protected override void Start()
        {
           
        }

        protected internal override void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            if (player == null)
            {
                player = GameManager.GetGMComponent<EntityComponentGM>().GetEntityLogic<PlayerLogic>();
            }
            if (player != null)
            {
                if (Mathf.Abs(player.transform.position.x - transform.position.x) > OpenDoorDistance)
                {
                    if (!OpenDoor)
                    {
                        OpenDoor = true;
                        if (player.transform.position.x < transform.position.x)
                        {
                            animator.SetTrigger("OpenDoorLookLeft");
                        }
                        else
                        {
                            animator.SetTrigger("OpenDoorLookRight");
                        }
                    }
                }
                else
                {
                    if (OpenDoor)
                    {
                        OpenDoor = false;
                        animator.SetTrigger("CloseDoor");
                    }
                }
            }
        }
    }
}