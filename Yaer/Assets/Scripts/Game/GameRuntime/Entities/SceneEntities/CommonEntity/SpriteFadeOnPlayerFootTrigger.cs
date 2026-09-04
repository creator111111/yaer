using System.Collections;
using UnityEngine;

namespace Game.GameRuntime.Entities.SceneEntities.CommonEntity
{
    /// <summary>
    /// 玩家脚底（PlayerFoot）进入感应区时，将目标 <see cref="SpriteRenderer"/> 的 alpha 渐变为半透明；
    /// 全部离开后恢复 <see cref="normalAlpha"/>。只改 <c>color.a</c>，不改 Sorting Layer / Order。
    /// </summary>
    /// <remarks>
    /// 挂载约定：与 <see cref="BoxCollider2D"/>（IsTrigger）同 GO，Collider 略大于墙图；
    /// <see cref="targetRenderers"/> 拖父节点隔断墙的 SpriteRenderer。
    /// 检测约定对齐 <see cref="ActivateChildOnPlayerFootTrigger"/>：按物体名识别 PlayerFoot，并实现 Trigger/Collision 四套回调。
    /// </remarks>
    public class SpriteFadeOnPlayerFootTrigger : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("需要半透明的 SpriteRenderer，通常拖父节点隔断墙的 Renderer")]
        private SpriteRenderer[] targetRenderers;

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("远离感应区时的 alpha")]
        private float normalAlpha = 1f;

        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("靠近感应区时的 alpha（策划常用 0.35～0.5）")]
        private float nearAlpha = 0.4f;

        [SerializeField]
        [Tooltip("渐变时长（秒）；0 表示瞬切")]
        private float fadeDuration = 0.2f;

        [SerializeField]
        [Tooltip("脚底碰撞体所在物体的名字，需与 Player.prefab 中 PlayerFoot 一致")]
        private string playerFootObjectName = "PlayerFoot";

        /// <summary>PlayerFoot 重叠计数，防止多 Collider 进出抖动导致 alpha 闪回。</summary>
        private int _overlapCount;

        private Coroutine _fadeCoroutine;

        private void OnDisable()
        {
            // 场景卸载或禁用时停止渐变并恢复不透明，避免卡在半透明
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }

            _overlapCount = 0;
            ApplyAlphaImmediate(normalAlpha);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryChangeOverlap(other, +1);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            TryChangeOverlap(other, -1);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision == null || collision.collider == null)
            {
                return;
            }

            TryChangeOverlap(collision.collider, +1);
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision == null || collision.collider == null)
            {
                return;
            }

            TryChangeOverlap(collision.collider, -1);
        }

        /// <summary>
        /// 若对方是配置的脚底物体，更新重叠计数并在 0↔1 边界触发渐变。
        /// </summary>
        private void TryChangeOverlap(Collider2D other, int delta)
        {
            if (other == null || other.gameObject.name != playerFootObjectName)
            {
                return;
            }

            int prev = _overlapCount;
            _overlapCount = Mathf.Max(0, _overlapCount + delta);

            if (prev == 0 && _overlapCount > 0)
            {
                StartFadeTo(nearAlpha);
            }
            else if (prev > 0 && _overlapCount == 0)
            {
                StartFadeTo(normalAlpha);
            }
        }

        private void StartFadeTo(float targetAlpha)
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

            _fadeCoroutine = StartCoroutine(FadeRoutine(targetAlpha));
        }

        /// <summary>协程 Lerp alpha；fadeDuration=0 时瞬切。</summary>
        private IEnumerator FadeRoutine(float targetAlpha)
        {
            if (targetRenderers == null || targetRenderers.Length == 0)
            {
                _fadeCoroutine = null;
                yield break;
            }

            if (fadeDuration <= 0f)
            {
                ApplyAlphaImmediate(targetAlpha);
                _fadeCoroutine = null;
                yield break;
            }

            float[] startAlphas = new float[targetRenderers.Length];
            for (int i = 0; i < targetRenderers.Length; i++)
            {
                SpriteRenderer r = targetRenderers[i];
                startAlphas[i] = r != null ? r.color.a : targetAlpha;
            }

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                for (int i = 0; i < targetRenderers.Length; i++)
                {
                    SpriteRenderer r = targetRenderers[i];
                    if (r == null)
                    {
                        continue;
                    }

                    Color c = r.color;
                    c.a = Mathf.Lerp(startAlphas[i], targetAlpha, t);
                    r.color = c;
                }

                yield return null;
            }

            ApplyAlphaImmediate(targetAlpha);
            _fadeCoroutine = null;
        }

        private void ApplyAlphaImmediate(float alpha)
        {
            if (targetRenderers == null)
            {
                return;
            }

            for (int i = 0; i < targetRenderers.Length; i++)
            {
                SpriteRenderer r = targetRenderers[i];
                if (r == null)
                {
                    continue;
                }

                Color c = r.color;
                c.a = alpha;
                r.color = c;
            }
        }
    }
}
