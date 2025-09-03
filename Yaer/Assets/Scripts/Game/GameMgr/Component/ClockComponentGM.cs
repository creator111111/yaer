using Game.GameMgr.Component.Base;
using UnityEngine;

namespace Game.GameMgr.Component
{
    public class ClockComponentGM : BaseComponentGM
    {
        private GameManager gameManager;
        private float time;
        public float Time => time;

        private void Update()
        {
            // if (gameManager.Pause) gameManager.GameTime += Time.deltaTime;
        }

        public void SetGameManager(GameManager m)
        {
            gameManager = m;
        }

        public void ResetTime()
        {
            time = 0;
        }
    }
}