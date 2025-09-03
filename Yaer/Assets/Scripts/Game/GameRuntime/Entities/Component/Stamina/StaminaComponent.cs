using GameFramework.UnityRuntimeExtend.Component;
using System;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component
{
    /// <summary>
    /// 体力组件
    /// </summary>
    public class StaminaComponent : BaseGFComponentMono
    {
        public float Stamina;
        public float MaxStamina;

        public float RecoverSpeed;

        public event Action<float> OnStaminaChanged;


        float timeCount = 0;
        float timeDistance = 0.05f; // 体力刷新的时间间隔

        protected override void OnInit()
        {
            Stamina = MaxStamina;
            RecoverSpeed = 0;
        }

        public void SetData(float Stamina, float MaxStamina)
        {
            this.Stamina = Stamina;
            this.MaxStamina = MaxStamina;
        }
        public bool IsMax => Stamina >= MaxStamina;

        public bool ChekcHasEnoughStamina(float needValue)
        {
            if (Stamina < needValue)
            {
                Debug.Log("===========体力不足!!!");
            }
            return Stamina >= needValue;
        }
        public void AddStamina(float value)
        {
            Stamina += value;
            Stamina = Mathf.Clamp(Stamina, 0, MaxStamina);
            OnStaminaChanged?.Invoke(Stamina);
        }

        public void SetRecoverSpeed(float speed)
        {
            RecoverSpeed = speed;
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (RecoverSpeed == 0) { return; }
            timeCount += Time.deltaTime;
            if (timeCount >= timeDistance)
            {
                timeCount = 0;
                AddStamina(RecoverSpeed * timeDistance);
            }
            //if (RecoverSpeed != 0)
            //{
            //    AddStamina(RecoverSpeed * Time.deltaTime);
            //}
        }
    }
}


