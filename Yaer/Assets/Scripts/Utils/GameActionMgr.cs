using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GameActionMgr : object
{
    static GameActionMgr instance;
    public static GameActionMgr getInstance()
    {
        if (instance == null)
        {
            instance = new GameActionMgr();
        }
        return instance;
    }

    /* 执行MoveTo动作,实际是对DOTween里面DOLocalMove的封装使用
     * SetDelay表示在这个动作执行之前先延时多少秒
     */
    public static TweenerCore<Vector3, Vector3, VectorOptions> runMoveToAction(GameObject node, Vector3 targetPos, float time, float delayTime =0)
    {
        if (!node) { return null; }
        var action = node.transform.DOLocalMove(targetPos, time).SetDelay(delayTime);
        action.SetLink(node);// 绑定对象，对象销毁时自动销毁Tween动作
        action.SetAutoKill(true);// 默认动作执行完毕自动销毁
        action.OnKill(() => {
            action = null;
        });
        return action;
    }
    // 这个移动方法是相对于世界坐标移动的
    public static TweenerCore<Vector3, Vector3, VectorOptions> runMoveToWorldPosAction(GameObject node, Vector3 targetPos, float time, float delayTime = 0)
    {
        if (!node) { return null; }
        var action = node.transform.DOMove(targetPos, time).SetDelay(delayTime);
        action.SetLink(node);
        action.SetAutoKill(true);// 默认动作执行完毕自动销毁
        action.OnKill(() => {
            action = null;
        });
         
        return action;
    }


    // 执行缩放动作
    public static TweenerCore<Vector3, Vector3, VectorOptions> runScaleToAction(GameObject node, Vector3 targetScale, float time, float delayTime = 0)
    {
        if (!node) { return null; }
        var action = node.transform.DOScale(targetScale, time).SetDelay(delayTime);
        action.SetLink(node);
        action.SetAutoKill(true);// 默认动作执行完毕自动销毁
        action.OnKill(() => {
            action = null;
        });
         
        return action;
    }
    // 执行旋转动作
    public static TweenerCore<Quaternion, Vector3, QuaternionOptions> runRotateToAction(GameObject node, Vector3 targetRotate, float time, float delayTime = 0)
    {
        if (!node) { return null; }
        var action = node.transform.DOLocalRotate(targetRotate, time).SetDelay(delayTime);
        action.SetLink(node);
        action.SetAutoKill(true);// 默认动作执行完毕自动销毁
        action.OnKill(() => {
            action = null;
        });
         
        return action;
    }
    // 执行延时回调动作,使用此方法时一定要记得及时销毁该动作，以免回调方法的调用者已经被销毁了还调用了回调方法
    public static TweenerCore<float, float, FloatOptions> runDelayTimeAction(float time, Action callFunc, GameObject linkNode=null)
    {
        var action = DOTween.To(() => 0f, delegate { }, 0f, time)
            .OnComplete(() =>
            {
                if (callFunc != null)
                {
                    callFunc();
                }
            });
        if (linkNode != null)
        {
            action.SetLink(linkNode);// 设置有绑定对象的时候需要绑定对象和对象同时销毁
        }
        action.SetAutoKill(true);// 默认动作执行完毕自动销毁
        action.OnKill(() => {
            action = null;
        });
        return action;
    }
    // 执行淡入淡出.检测CanvasGroup组件,该效果会作用于该节点以及所有子节点
    // transparencyValue:0f表示完全不可见，1f,表示100%可见
    public static TweenerCore<float, float, FloatOptions> runFadeAction(GameObject node, float transparencyValue, float time, float delayTime = 0)
    {
        if (!node) { return null; }
        // 先检测是否有CanvasGroup组件
        var canvasGroup = node.GetComponent<CanvasGroup>();
        if (!canvasGroup) {  return null; }
        var action = canvasGroup.DOFade(transparencyValue, time).SetDelay(delayTime);
        action.SetLink(node);
        action.SetAutoKill(true);// 默认动作执行完毕自动销毁
        action.OnKill(() => {
            action = null;
        });
        return action;
    }
    // 执行淡入淡出,检测Image组件,该效果只作用于一个节点
    public static TweenerCore<Color, Color, ColorOptions> runFadeActionOther(GameObject node, float transparencyValue, float time, float delayTime = 0)
    {
        if (!node) { return null; }
        // 先检测是否有Image组件
        var image = node.GetComponent<Image>();
        if (!image) { return null; }
        var action = image.DOFade(transparencyValue, time).SetDelay(delayTime);
        action.SetLink(node);
        action.SetAutoKill(true);// 默认动作执行完毕自动销毁
        action.OnKill(() => {
            action = null;
        });
        return action;
    }
    
    // 执行淡入淡出,检测SpriteRender组件,该效果只作用于一个节点
    public static TweenerCore<Color, Color, ColorOptions> runFadeActionSpriteRender(GameObject node, float transparencyValue, float time, float delayTime = 0)
    {
        if (!node) { return null; }
        // 先检测是否有SpriteRenderer组件
        var image = node.GetComponent<SpriteRenderer>();
        if (!image) { return null; }
        var action = image.DOFade(transparencyValue, time).SetDelay(delayTime);
        action.SetLink(node);
        action.SetAutoKill(true);// 默认动作执行完毕自动销毁
        action.OnKill(() => {
            action = null;
        });
        return action;
    }

    // 执行跳跃动作
    public static Sequence runJumpToAction(GameObject node, Vector3 targetPos, int jumpPower, int jumpCount, float actionTime, bool snapping = false, float delayTime = 0f)
    {
        if (!node) { return null; }
        var action = node.transform.DOLocalJump(targetPos, jumpPower, jumpCount, actionTime, snapping).SetDelay(delayTime);
        action.SetLink(node);
        action.SetAutoKill(true);// 默认动作执行完毕自动销毁
        action.OnKill(() => {
            action = null;
        });
        return action;
    }
    // 执行进度条拉伸动作
    public static Tween runImageBarFillpmentAciton(GameObject node, float targetRate, float acitonTimes, float delayTime = 0)
    {
        if (node == null) { return null; }
        var image = node.GetComponent<Image>();
        if (image == null) { return null; }
        var action = image.DOFillAmount(targetRate, acitonTimes).SetDelay(delayTime);
        action.SetLink(node);
        action.SetAutoKill(true);// 默认动作执行完毕自动销毁
        action.OnKill(() => {
            action = null;
        });
        return action;
    }

    // 有需要再添加其他动作

    // 执行顺序动作
    public static Sequence runSequenceAction(GameObject node, List<Tween> actionList)
    {
        var sequence = DOTween.Sequence(node);
        if (actionList.Count <= 0)
        {
            return null;
        }
        foreach (var action in actionList)
        {
            var realAction = action as Tween;
            if(realAction != null)
            {
                sequence.Append(realAction);
            }
        }
        sequence.Play();
        sequence.SetLink(node);
        sequence.SetAutoKill(true);// 默认动作执行完毕自动销毁
        sequence.OnKill(() => {
            sequence = null;
        });
        return sequence;
    }


}