using System;
using Game.GameRuntime.Entities.Component.Map;
using Game.GameRuntime.GameSceneManager.Base;
using UnityEngine;

namespace Game.GameRuntime.GameSceneManager.Component
{
    public class MapControlComponentGSM :BaseComponentGSM
    {
        [SerializeField] private Map map;

        public Transform RightDefaultBornTsf => map.rightBornTsf;
        public Transform LeftDefaultBornTsf => map.leftBornTsf;
        public Transform DefaultBornTsf => map.defaultBornTsf;

        private void OnValidate()
        {
            if (map == null) Debug.LogWarning("map引用丢失哦", gameObject);
        }

        public override void OnInit(IGameSceneManager manager)
        {
            base.OnInit(manager);
            
            map.OnInit();
        }

        public void SetSceneUnlockCondition(Func<bool> left=null, Func<bool> right=null)
        {
            if (map.LeftDoor != null)
            {
                map.LeftDoor.CheckNextSceneUnlock = left;
            }
            if (map.RightDoor != null)
            {
                map.RightDoor.CheckNextSceneUnlock = right;
            }
        }
    }
}