using Game.GameMgr;
using Game.GameRuntime.Entities.Component.Move;
using Game.GameRuntime.Entities.Player;
using Game.GameRuntime.GameSceneManager.Component;
using System;
using UnityEngine;

namespace Game.GameRuntime.Entities.Component.Effect
{
    public class EffectPlayerComponent: MonoBehaviour
    {
        [SerializeField]
        private string assetPath;

        public string AssetPath => $"Assets/GameRes/Prefabs/Entity/Effect/{assetPath}";
        public Transform posTsf;

        private EffectComponentGSM effectMgr => GameManager.GetGameSceneManager().GetModule<EffectComponentGSM>();

        public void PlayEffect<EffectType>(int times = 1, Action<EffectType> callback = null) where EffectType : AnimaEffectComponent
        {
            effectMgr?.PlayEffect<EffectType>(AssetPath, effect =>
            {
                SetPosition(effect.gameObject, posTsf);
                effect.Play(times);
                callback?.Invoke(effect);
            });
        }

        private void SetPosition(GameObject effectGo, Transform parent)
        {
            effectGo.transform.position = parent.position;
        }
    }
}