using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

/// <summary>
/// CanvasGroup ͸���Ƚ��䶯����
/// �������� BBParameter&lt;UnityEngine.CanvasGroup&gt;���� BB δ�������򰴱��������ҡ�
/// �������ͱ���д UnityEngine.CanvasGroup�������� NodeCanvas.Framework.CanvasGroup �� CS0104����
/// NewGame��Prepare ��� Animator����������󿪻ز��䵽 YaerShow ĩ֡������ص� Start ������Ūû���� KingMove ���νӡ�
/// </summary>
[Category("UI")]
[Name("CanvasGroup͸���Ƚ��䶯��")]
public class CanvasGroupAlphaActionTask : ActionTask
{
    public BBParameter<UnityEngine.CanvasGroup> canvasGroup;
    public BBParameter<float> StartAlpha;
    public BBParameter<float> EndAlpha;
    public BBParameter<float> Duration;
    /// <summary>��ʼ����ǰ�ȴ����������ڡ��ȼ� BG ���ġ���ļ����</summary>
    public BBParameter<float> Delay;
    public BBParameter<bool> EndActionOnAnimationEnd;

    /// <summary>NewGame ���»����������ֺ��״̬����ĩ֡ alpha=1������ת�� KingMove��</summary>
    const string NewGameYaerShowState = "YaerShow";

    protected override void OnExecute()
    {
        Do().Forget();
    }

    private async UniTask Do()
    {
        var cg = ResolveCanvasGroup();
        if (cg == null)
        {
            Debug.LogWarning(
                $"[CanvasGroupAlpha] δ�ҵ� CanvasGroup��bbName={canvasGroup?.name} agent={agent?.name}",
                agent);
            EndAction();
            return;
        }

        float delay = Delay != null ? Delay.value : 0f;
        float startA = StartAlpha != null ? StartAlpha.value : 0f;
        float endA = EndAlpha != null ? EndAlpha.value : 1f;
        float duration = Duration != null ? Duration.value : 0f;
        bool waitEnd = EndActionOnAnimationEnd != null && EndActionOnAnimationEnd.value;

        cg.alpha = startA;

        if (waitEnd)
        {
            if (delay > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(delay));
            }

            await cg.DOFade(endA, duration).AsyncWaitForCompletion();
            TryRestoreStoryAnimatorAfterPortraitFade(cg, endA);
            EndAction();
        }
        else
        {
            if (delay > 0f)
            {
                var seq = DOTween.Sequence();
                seq.AppendInterval(delay);
                seq.Append(cg.DOFade(endA, duration));
                seq.OnComplete(() => TryRestoreStoryAnimatorAfterPortraitFade(cg, endA));
            }
            else
            {
                cg.DOFade(endA, duration).OnComplete(() => TryRestoreStoryAnimatorAfterPortraitFade(cg, endA));
            }

            EndAction();
        }
    }

    /// <summary>
    /// Prepare �ص��Ĺ��¸� Animator�����غ������䵽 YaerShow ĩ֡��
    /// ԭ����ֻ Enable ͣ�� Start���ذ�/Ĭ��ֵ���ڶԻ������ʱ������ alpha ��� 0��
    /// KingMove Ҳֻ�ܴ� YaerShow ת���������Ƚ����״̬��
    /// ���������һֱ���� Animator �� KingMove ǰ�ٿ��������̨���ڵ㣬�Ķ������
    /// </summary>
    static void TryRestoreStoryAnimatorAfterPortraitFade(UnityEngine.CanvasGroup cg, float endAlpha)
    {
        if (cg == null)
        {
            return;
        }

        Transform t = cg.transform;
        while (t != null)
        {
            var anim = t.GetComponent<Animator>();
            if (anim == null)
            {
                t = t.parent;
                continue;
            }

            // �������� Prepare �ص��ģ�����������KenMuNi��Animator һֱ���ţ�����״̬��
            if (!anim.enabled)
            {
                anim.enabled = true;

                int stateHash = Animator.StringToHash(NewGameYaerShowState);
                if (anim.HasState(0, stateHash))
                {
                    anim.Play(stateHash, 0, 1f);
                    anim.Update(0f);
                    Debug.Log("[CanvasGroupAlpha] Animator �� YaerShow end on " + t.name);
                }
                else
                {
                    Debug.Log("[CanvasGroupAlpha] re-enable Animator���� YaerShow ״̬��on " + t.name);
                }
            }

            // �����Ƿ���״̬������Ŀ��͸�����ٶ�һ�Σ���ֹ Animator ��֡��д
            cg.alpha = endAlpha;
            return;
        }
    }

    UnityEngine.CanvasGroup ResolveCanvasGroup()
    {
        if (canvasGroup != null && canvasGroup.value != null)
        {
            return canvasGroup.value;
        }

        var objectName = canvasGroup != null ? canvasGroup.name : null;
        if (string.IsNullOrEmpty(objectName))
        {
            return null;
        }

        var found = FindCanvasGroupByObjectName(agent != null ? agent.transform : null, objectName);
        if (found != null)
        {
            Debug.Log($"[CanvasGroupAlpha] BB Ϊ�գ������������� {objectName}");
            return found;
        }

        var ownerAgent = ownerSystemAgent;
        if (ownerAgent != null && (agent == null || ownerAgent.transform != agent.transform))
        {
            found = FindCanvasGroupByObjectName(ownerAgent.transform, objectName);
            if (found != null)
            {
                Debug.Log($"[CanvasGroupAlpha] BB Ϊ�գ�ownerAgent ������������ {objectName}");
                return found;
            }
        }

        return null;
    }

    static UnityEngine.CanvasGroup FindCanvasGroupByObjectName(Transform root, string objectName)
    {
        if (root == null || string.IsNullOrEmpty(objectName))
        {
            return null;
        }

        var transforms = root.GetComponentsInChildren<Transform>(true);
        for (var i = 0; i < transforms.Length; i++)
        {
            var t = transforms[i];
            if (t.name != objectName)
            {
                continue;
            }

            var cg = t.GetComponent<UnityEngine.CanvasGroup>();
            if (cg != null)
            {
                return cg;
            }
        }

        return null;
    }

    protected override string info
    {
        get
        {
            float delay = Delay != null ? Delay.value : 0f;
            if (delay > 0f)
            {
                return string.Format("<i>' {3}͸����: wait {4}s, {0} -> {1}, {2}s '</i>", StartAlpha, EndAlpha, Duration, canvasGroup, Delay);
            }

            return string.Format("<i>' {3}͸����: {0} -> {1}, {2}s '</i>", StartAlpha, EndAlpha, Duration, canvasGroup);
        }
    }
}
