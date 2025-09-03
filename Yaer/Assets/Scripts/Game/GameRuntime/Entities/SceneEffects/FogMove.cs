// using System;

using System.Collections;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Game.GameRuntime.Entities.SceneEffects
{
    public class FogMove : MonoBehaviour
    {
        [SerializeField] private Transform FarFog;
        [SerializeField] private Transform MidFog;
        [SerializeField] private Transform NearFog;

        [SerializeField] private float FogRandomTime = 10;
        [SerializeField] private float FogXLimit = 5;

        private Vector3 FarFogPos;

        private Vector3 FarFogTargetPos;
        private Vector3 MidFogPos;
        private Vector3 MidFogTargetPos;

        private bool Move;
        private Vector3 NearFogPos;
        private Vector3 NearFogTargetPos;

        private void Start()
        {
            FarFogPos = FarFog.localPosition;
            MidFogPos = MidFog.localPosition;
            NearFogPos = NearFog.localPosition;

            StartCoroutine(RandomMove());
        }

        private void Update()
        {
            if (Move)
            {
                Move = false;
                FarFog.DOLocalMove(FarFogTargetPos, FogRandomTime);
                MidFog.DOLocalMove(MidFogTargetPos, FogRandomTime);
                NearFog.DOLocalMove(NearFogTargetPos, FogRandomTime);
            }
        }

        private IEnumerator RandomMove()
        {
            while (gameObject.activeInHierarchy)
            {
                SetTarget();
                Move = true;
                yield return new WaitForSeconds(FogRandomTime);
            }
        }

        private Vector3 SetSingleTarget(Vector3 origin)
        {
            // 为简单起见，这里假设游戏场景是10x10单位
            // 根据实际需要调整这些值
            var x = Random.Range(-FogXLimit, FogXLimit);
            var targetPos = new Vector3(x, 0, 0);
            // Debug.Log(origin + targetPos);
            return origin + targetPos;
        }

        private void SetTarget()
        {
            FarFogTargetPos = SetSingleTarget(FarFogPos);
            MidFogTargetPos = SetSingleTarget(MidFogPos);
            NearFogTargetPos = SetSingleTarget(NearFogPos);
        }
    }
}