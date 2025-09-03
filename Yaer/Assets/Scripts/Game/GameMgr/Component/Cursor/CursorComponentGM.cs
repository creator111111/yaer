using Game.GameMgr.Component.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameMgr.Component.Cursor
{
    public enum CursorState
    {
        Normal,
        Catch,
        View,
        Chat
    }

    [Serializable]
    public class CursorTextureConfig
    {
        public Texture2D Tex;
        /// <summary>
        /// 鼠标原点在Tex上的UV
        /// </summary>
        public Vector2 Hotspot;
    }

    public class CursorComponentGM : BaseComponentGM
    {
        [SerializeField]
        private CursorTextureConfig Normal;
        [SerializeField]
        private CursorTextureConfig CatchCursorNormal;
        [SerializeField]
        private CursorTextureConfig CatchCursorHold;
        [SerializeField]
        private CursorTextureConfig ViewCursorNormal;
        [SerializeField]
        private CursorTextureConfig ViewCursorHold;
        [SerializeField]
        private CursorTextureConfig[] ChatCursors;
        [SerializeField]
        private float ChatCursorAnimationInterval;
        [SerializeField]
        private float ViewCursorAnimationInterval1;
        [SerializeField]
        private float ViewCursorAnimationInterval2;

        private Coroutine cursorAnimation;
        

        private IEnumerator ChatCursorAnimation()
        {
            var interval = new WaitForSeconds(ChatCursorAnimationInterval);
            while (true)
            {
                foreach (var ChatCursor in ChatCursors)
                {
                    SetCursor(ChatCursor);
                    yield return interval;
                }
            }
        }

        private IEnumerator ViewCursorAnimation()
        {
            var interval1 = new WaitForSeconds(ViewCursorAnimationInterval1);
            var interval2 = new WaitForSeconds(ViewCursorAnimationInterval2);
            while (true)
            {
                SetCursor(ViewCursorNormal);
                yield return interval1;
                SetCursor(ViewCursorHold);
                yield return interval2;
            }
        }

        private CursorState cursorState
        {
            get
            {
                if (CursorChangeQueue == null || CursorChangeQueue.Count == 0) return CursorState.Normal;
                else
                {
                    return CursorChangeQueue[0].TargetState;
                }
            }
        }

        private List<CursorChangeArgs> CursorChangeQueue;

        public override void OnEnter()
        {
            base.OnEnter();
            CursorChangeQueue = new List<CursorChangeArgs>();
            OnEnterChangeTrigger(new CursorChangeArgs(CursorState.Normal, Guid.NewGuid(), 0));
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (Input.GetMouseButtonDown(0))
            {
                OnPointerHold();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                OnPointerRelease();
            }
        }

        private void SetCursor(CursorTextureConfig cursorTexConfig)
        {
            UnityEngine.Cursor.SetCursor(cursorTexConfig.Tex, cursorTexConfig.Hotspot, CursorMode.Auto);
        }

        public void OnEnterChangeTrigger(CursorChangeArgs cursorChangeArgs)
        {
            CursorChangeQueue.Add(cursorChangeArgs);
            UpdateCursorTexture();
        }

        public void OnExitChangeTrigger(Guid id)
        {
            int success = CursorChangeQueue.RemoveAll((args) => args.guid == id);
            if (success > 0)
            {
                UpdateCursorTexture();
            }
        }

        private void UpdateCursorTexture()
        {
            if (cursorAnimation != null)
            {
                StopCoroutine(cursorAnimation);
                cursorAnimation = null;
            }

            CursorChangeQueue.Sort();

            switch (cursorState)
            {
                case CursorState.Normal:
                    SetCursor(Normal);
                    break;
                case CursorState.Catch:
                    SetCursor(CatchCursorNormal);
                    break;
                case CursorState.View:
                    cursorAnimation = StartCoroutine(ViewCursorAnimation());
                    break;
                case CursorState.Chat:
                    cursorAnimation = StartCoroutine(ChatCursorAnimation());
                    break;
            }
        }



        private void OnPointerHold()
        {
            switch (cursorState)
            {
                case CursorState.Catch:
                    SetCursor(CatchCursorHold);
                    break;
            }
        }

        private void OnPointerRelease()
        {
            switch (cursorState)
            {
                case CursorState.Catch:
                    SetCursor(CatchCursorNormal);
                    break;
            }
        }
    }
}