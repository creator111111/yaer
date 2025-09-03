using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.GameRuntime.UI.FormLogic.Cartoon
{
    public class CartoonPlayer : MonoBehaviour
    {
        [Serializable]
        public class CartoonPage
        {
            public CanvasGroup pageCanvasGroup;
            public CanvasGroup[] PageContents;
        }

        [SerializeField]
        public List<CartoonPage> CartoonPages;

        private Action PlayEndCallback;

        private void ResetCartoon()
        {
            foreach (var page in CartoonPages)
            {
                page.pageCanvasGroup.alpha = 0;
                for (int j = 0; j < page.PageContents.Length; j++)
                {
                    page.PageContents[j].alpha = 0;
                }
            }
        }

        public void PlayCartoon(Action callback = null)
        {
            PlayEndCallback = callback;
            ResetCartoon();
            StartCoroutine(_PlayCartoon());
        }

        private IEnumerator _PlayCartoon()
        {
            foreach (var page in CartoonPages)
            {
                page.pageCanvasGroup.alpha = 1;
                for (int contentIdx = 0; contentIdx < page.PageContents.Length; contentIdx++)
                {
                    yield return StartCoroutine(ShowContent(page.PageContents[contentIdx], 0, 1));
                }
                yield return StartCoroutine(ShowContent(page.pageCanvasGroup, 1, 0));
            }
            PlayEndCallback?.Invoke();
            PlayEndCallback = null;
        }

        private IEnumerator ShowContent(CanvasGroup contentCanvasGroup, float startAlpha, float endAlpha, float duration = 3)
        {
            contentCanvasGroup.alpha = startAlpha;
            float speed = (endAlpha - startAlpha) / duration;
            while (Mathf.Abs(contentCanvasGroup.alpha - endAlpha) > 0.01f)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    break;
                }
                contentCanvasGroup.alpha += speed * Time.deltaTime;
                yield return null;
            }
            contentCanvasGroup.alpha = endAlpha;
            yield return null;
        }
    }
}

