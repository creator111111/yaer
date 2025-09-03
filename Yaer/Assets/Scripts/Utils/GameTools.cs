using DG.Tweening;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;
public class GameTools : object
{
    static GameTools instance;

    private System.Random randomObj = new System.Random();
    public static GameTools getInstance()
    {
        if (instance == null)
        {
            instance = new GameTools();
        }
        return instance;
    }

    // 获取一个UI对象的大小
    public static Vector2 getObjectSize(GameObject obj)
    {
        var rect = obj.GetComponent<RectTransform>();
        if (rect == null)
        {
            return Vector2.zero;
        }
        return rect.sizeDelta;
    }

    // 通过代码创建一个图片
    public static GameObject createImageView(GameObject parent, string imgPath = "")
    {
        GameObject newGameObj = new GameObject("newGameObj");
        newGameObj.AddComponent<Image>();
        newGameObj.transform.SetParent(parent.transform);
        loadTexture(newGameObj, imgPath);
        return newGameObj;
    }

    // 设置一个对象的图片资源
    public static void loadTexture(GameObject gameObject, string imgPath, bool isNative = true)
    {
        var image = gameObject.GetComponent<Image>();
        if (image != null)
        {
            Sprite sprite = Resources.Load<Sprite>(imgPath);
            if (sprite == null)
            {
                Debug.LogWarning("=================imgPath:" + imgPath + " Error!!!");
                return;
            }
            // 加载图片资源
            image.sprite = sprite;
            // 设置组件为图片原尺寸大小
            if (isNative) { image.SetNativeSize(); }
        }
        else
        {
            Debug.LogWarning("=================GameObject:" + gameObject.name + " not have Component Image!!!");
        }
    }
    // 设置一个图片的资源从atlas
    public static void loadTextureByAtlas(GameObject gameObject, SpriteAtlas spriteAtlas, string imgName, bool isNative=true)
    {
        var image = gameObject.GetComponent<Image>();
        if (image != null)
        {
            // 加载图片资源
            image.sprite = spriteAtlas.GetSprite(imgName);
            // 设置组件为图片原尺寸大小
            if (isNative) { image.SetNativeSize(); }
        }
        else
        {
            Debug.LogWarning("=================GameObject:" + gameObject.name + " not have Component Image!!!");
        }
    }

    // 设置一个图片的透明度
    public static void setGameObjectOpacity(GameObject gameObject, float value)
    {
        // 优先判断是否有CanvasGroup组件
        if (gameObject.GetComponent<CanvasGroup>() != null)
        {
            var canvasGroup = gameObject.GetComponent<CanvasGroup>();
            canvasGroup.alpha = value;
            return;
        }
        if (gameObject.GetComponent<Image>() != null)
        {
            var image = gameObject.GetComponent<Image>();
            var oldColor = image.color;
            image.color = new Color(oldColor.r, oldColor.g, oldColor.b, value);
            return;
        }
        
    }
    // 获取一个图片纹理的原始大小
    public static Vector2 getTextureRealSize(string imgPath)
    {
        Vector2 size = new Vector2();
        if (string.IsNullOrEmpty(imgPath))
        {
            return size;
        }
        // 读取图片文件的字节数据
        Texture2D texture = Resources.Load<Texture2D>(imgPath);
        size.x = texture.width;
        size.y = texture.height;
        return size;
    }
    // 设置一个游戏对象的点击事件,按钮会被缩放
    /*
     * autoAddEventTrigger:是否自动添加事件触发组件
    */
    public static void setObjectClickFunc(GameObject targetObj, Action clickFunc, Action touchStartFunc = null, bool autoAddEventTrigger = true, float scaleRate = 0.9f)
    {
        if (targetObj == null) { return; }
        // 检查该对象是否有触摸事件组件
        var eventTrigger = targetObj.GetComponent<EventTrigger>();
        if (!eventTrigger && autoAddEventTrigger)
        {
            // 没有就添加一个触摸事件组件
            eventTrigger = targetObj.AddComponent<EventTrigger>();
        }
        // 遍历所有的事件条目并移除特定类型的事件
        for (int i = eventTrigger.triggers.Count - 1; i >= 0; i--)
        {
            if (eventTrigger.triggers[i].eventID == EventTriggerType.PointerDown ||
                eventTrigger.triggers[i].eventID == EventTriggerType.PointerUp ||
                eventTrigger.triggers[i].eventID == EventTriggerType.PointerExit ||
                eventTrigger.triggers[i].eventID == EventTriggerType.PointerEnter ||
                eventTrigger.triggers[i].eventID == EventTriggerType.Drag)
            {
                eventTrigger.triggers[i].callback.RemoveAllListeners();
                // 移除指定的事件
                eventTrigger.triggers.RemoveAt(i);
            }
        }
        // 通过此方法添加的按钮点击是缩放的
        var scaleTime = 0.1f;// 缩放时间
        var oldScale = targetObj.transform.localScale;

        var hasTouchTag = false; // 是否可以触发按钮点击后的回调事件
        //var btnCallFunc = clickFunc;
        //var btnTouchFunc = touchStartFunc;// 触碰按钮就触发的事件
        // 按下按钮状态
        Entry entry = new Entry();
        entry.eventID = EventTriggerType.PointerDown;
        // 松开按钮状态
        Entry entry2 = new Entry();
        entry2.eventID = EventTriggerType.PointerUp;
        // 移出按钮状态
        Entry entry3 = new Entry();
        entry3.eventID = EventTriggerType.PointerExit;
        // 进入按钮状态
        Entry entry4 = new Entry();
        entry4.eventID = EventTriggerType.PointerEnter;
        // 指针在按钮上移动状态
        Entry entry5 = new Entry();
        entry5.eventID = EventTriggerType.Drag;

        entry.callback.AddListener((data) =>
        {
            Debug.Log("=======================Touch Button:" + targetObj.name);
            var curScale = targetObj.transform.localScale;
            var scaleArg = scaleRate * curScale;// 按钮相比原来缩放多少
            eventTrigger.triggers.Remove(entry);
            // 只有点击之后才会添加取消事件
            if (!eventTrigger.triggers.Contains(entry2))
            {
                eventTrigger.triggers.Add(entry2);
            }

            // 按下时按钮缩小
            GameActionMgr.runScaleToAction(targetObj, scaleArg, scaleTime);
            hasTouchTag = true;
            if (touchStartFunc != null)
            {
                touchStartFunc();
            }
        });
        eventTrigger.triggers.Add(entry);

        entry2.callback.AddListener((data) =>
        {
            if (!hasTouchTag) { return; }
            // 松开按钮时立刻去掉取消按钮事件
            eventTrigger.triggers.Remove(entry2);
            // 松开按钮时恢复原大小
            var action = GameActionMgr.runScaleToAction(targetObj, oldScale, scaleTime);
            action.onComplete = () =>
            {
                // 按钮松开恢复到原状之后才能再次点击
                if (!eventTrigger.triggers.Contains(entry))
                {
                    eventTrigger.triggers.Add(entry);
                }
            };
            // 一般按钮松开按钮时触发回调事件
            if (clickFunc != null)
            {
                clickFunc();
            }
        });
        //eventTrigger.triggers.Add(entry2);
        bool isPointTouchExitBtn = false;// 触摸鼠标是否移出按钮的范围
        entry3.callback.AddListener((data) =>
        {
            isPointTouchExitBtn = true;
        });
        eventTrigger.triggers.Add(entry3);
        entry4.callback.AddListener((data) =>
        {
            isPointTouchExitBtn = false;
        });
        eventTrigger.triggers.Add(entry4);

        entry5.callback.AddListener((data) =>
        {
            if (!hasTouchTag) { return; }
            if (isPointTouchExitBtn)
            {
                hasTouchTag = false;
                // 取消按钮事件触发时也需要去掉取消事件
                //eventTrigger.triggers.Remove(entry2);

                var action = GameActionMgr.runScaleToAction(targetObj, oldScale, scaleTime);
                action.onComplete = () =>
                {
                    // 按钮松开恢复到原状之后才能再次点击
                    if (!eventTrigger.triggers.Contains(entry))
                    {
                        eventTrigger.triggers.Add(entry);
                    }
                };
            }
        });
        eventTrigger.triggers.Add(entry5);

    }

    // 清除一个游戏对象的点击事件（前提是由GameTools.setObjectClickFunc添加的事件)
    public static void clearObjectClickFunc(GameObject targetObj)
    {
        var eventTrigger = targetObj.GetComponent<EventTrigger>();
        if (!eventTrigger)
        {
            return;
        }
        // 遍历所有的事件条目并移除特定类型的事件
        for (int i = eventTrigger.triggers.Count - 1; i >= 0; i--)
        {
            if (eventTrigger.triggers[i].eventID == EventTriggerType.PointerDown ||
                eventTrigger.triggers[i].eventID == EventTriggerType.PointerUp ||
                eventTrigger.triggers[i].eventID == EventTriggerType.PointerExit ||
                eventTrigger.triggers[i].eventID == EventTriggerType.PointerEnter ||
                eventTrigger.triggers[i].eventID == EventTriggerType.Drag)
            {
                eventTrigger.triggers[i].callback.RemoveAllListeners();
                // 移除指定的事件
                eventTrigger.triggers.RemoveAt(i);
            }
        }
    }

    // 获取两个整数之间随机一个整数
    public static int getRandomIntNum(int min, int max)
    {
        var range = max - min;
        if (range <= 0) { return min; } // 如果最小值比最大值大就直接返回最小值
        var random = getInstance().randomObj;
        return random.Next(min, max + 1);
    }
    // 随机从某个列表中获取一个值
    public static T getRandomValueFromList<T>(List<T> targetList)
    {
        if (targetList.Count <= 0) { return default; }
        var randomIndex = getRandomIntNum(0, targetList.Count - 1);
        return targetList[randomIndex];
    }
    // 打乱列表
    public static void disruptIntVector<T>(List<T> dataList, int defaultCount = 5)
    {
        var count = defaultCount;// 重复次数
        int maxIndex = dataList.Count - 1;
        for (var j = 0; j < count; j++)
        {
            for (var i = 0; i < dataList.Count; i++)
            {
                var randomIndex = getRandomIntNum(0, maxIndex);
                var tempData = dataList[i];
                dataList[i] = dataList[randomIndex];
                dataList[randomIndex] = tempData;
            }
        }
    }
    // 随机是否满足%X的概率,num是百分比的分子
    public static bool randomRateHasGet(int num)
    {
        if (num >= 100)
        {
            return true;
        }
        List<int> totalNumList = new List<int>();
        for (var i = 0; i < 100; i++)
        {
            if (i < num)
            {
                totalNumList.Add(1);
            }
            else
            {
                totalNumList.Add(0);
            }
        }
        disruptIntVector(totalNumList);
        var randomIndex = getRandomIntNum(0, totalNumList.Count - 1);
        return totalNumList[randomIndex] == 1;
    }
    // 清除一个GameObject下的所有子GameObject
    public static void clearAllChildren(GameObject parentObj)
    {
        var childCount = parentObj.transform.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            var childNode = parentObj.transform.GetChild(i);
            // 立刻删除子节点
            UnityEngine.Object.DestroyImmediate(childNode.gameObject);
        }
    }

    // 设置一个对象带有普通Text组件的文本内容
    public static void setText(GameObject parentObj, string text)
    {
        if (parentObj == null) { return; }
        var textComponent = parentObj.GetComponent<Text>();
        if (textComponent != null)
        {
            textComponent.text = text;
        }
    }
    // 设置一个按钮带有普通组件的文本内容,这个按钮只能有一个子节点有文本组件
    public static void setNormalBtnText(GameObject normalBtn, string text)
    {
        if (normalBtn == null) { return; }
        var childCount = normalBtn.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            var childObj = normalBtn.transform.GetChild(i);// 获取按钮下的子节点
            var textComponent = childObj.GetComponent<Text>();
            if (textComponent != null)
            {
                // 如果找到按钮子节点带有Text组件，则设置文本内容并直接退出
                textComponent.text = text;
                return;
            }
        }
    }

    // 设置一个带有TextMeshProUGUI组件的文本内容
    public static void setTMPUGUIText(GameObject obj, string text)
    {
        if (obj == null) { return; }
        var textComponent = obj.GetComponent<TextMeshProUGUI>();
        if (textComponent != null)
        {
            textComponent.text = text;
        }
    }

    // 退出游戏
    public static void exitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }


    // 设置一个GameObject节点被选中，播放选中动画
    public static void setObjectHasSelect(GameObject obj)
    {
        var actionList = new List<Tween>()
        {
            GameActionMgr.runScaleToAction(obj, new Vector3(0.8f, 0.8f, 0), 0.1f),
            GameActionMgr.runScaleToAction(obj, new Vector3(1.2f, 1.2f, 0), 0.2f),
            GameActionMgr.runScaleToAction(obj, new Vector3(0.9f, 0.9f, 0), 0.1f),
            GameActionMgr.runScaleToAction(obj, new Vector3(1, 1, 0), 0.1f),
        };
        GameActionMgr.runSequenceAction(obj, actionList);
    }
    // 设置一个GameObject执行缓慢缩放动作
    public static Tween setObjectRunWaitScaleAction(GameObject obj, bool isLoop = true)
    {
        var oldScale = obj.transform.localScale;// 节点原来的缩放度
        var time = 0.8f;
        var actionList = new List<Tween>()
        {
            GameActionMgr.runScaleToAction(obj, 0.9f * oldScale, time).SetEase(Ease.Linear),
            GameActionMgr.runScaleToAction(obj, oldScale, time).SetEase(Ease.Linear),
            GameActionMgr.runScaleToAction(obj, 1.1f * oldScale, time).SetEase(Ease.Linear),
            GameActionMgr.runScaleToAction(obj, oldScale, time).SetEase(Ease.Linear),
        };
        Tween action;
        if (isLoop)
        {
            action = GameActionMgr.runSequenceAction(obj, actionList).SetLoops(-1, (int)LoopType.Restart);
            //GameActionMgr.runScaleToAction(obj, 1.1f * oldScale, time).SetLoops(-1, LoopType.Yoyo);
        }
        else
        {
            //GameActionMgr.runScaleToAction(obj, 1.1f * oldScale, time);
            action = GameActionMgr.runSequenceAction(obj, actionList);
        }
        return action;
    }
    // 设置一个节点执行震动效果
    public static void setNodeShockAction(GameObject obj, Action endCallback)
    {
        var oldPos = obj.transform.localPosition;
        var time = 0.1f;
        var actionCount = getRandomIntNum(8, 10);// 一轮动作的次数
        var tempActList = new List<Tween>();
        for (var i = 0; i < actionCount; i++)
        {
            var firstMoveX = getRandomIntNum(-5, 5);
            var firstMoveY = getRandomIntNum(-5, 5);
            var moveTo = GameActionMgr.runMoveToAction(obj, new Vector3(oldPos.x + firstMoveX, oldPos.y + firstMoveY, 0), time);
            tempActList.Add(moveTo);
        }
        var sequence = GameActionMgr.runSequenceAction(obj, tempActList);
        // 设置动作结束回调
        sequence.onComplete = () =>
        {
            obj.transform.localPosition = oldPos;
            // 调用传入的结束回调
            if (endCallback != null)
            {
                endCallback();
            }
        };
    }

    // 设置一个GameObject鼠标进入，点击和退出回调方法
    public static void setObjPointTouchEvent(GameObject targetObj, Action clickFunc, Action pointInFunc = null, Action pointOutFunc = null, bool autoAddEventTrigger = true)
    {
        if (targetObj == null) { return; }
        // 检查该对象是否有触摸事件组件
        var eventTrigger = targetObj.GetComponent<EventTrigger>();
        if (!eventTrigger && autoAddEventTrigger)
        {
            // 没有就添加一个触摸事件组件
            eventTrigger = targetObj.AddComponent<EventTrigger>();
        }
        // 遍历所有的事件条目并移除特定类型的事件
        for (int i = eventTrigger.triggers.Count - 1; i >= 0; i--)
        {
            if (eventTrigger.triggers[i].eventID == EventTriggerType.PointerClick ||
                eventTrigger.triggers[i].eventID == EventTriggerType.PointerEnter ||
                eventTrigger.triggers[i].eventID == EventTriggerType.PointerExit)
            {
                // 移除指定的事件
                eventTrigger.triggers.RemoveAt(i);
            }
        }
        // 按下按钮状态
        Entry entry = new Entry();
        entry.eventID = EventTriggerType.PointerClick;
        // 进入指定区域状态
        Entry entry2 = new Entry();
        entry2.eventID = EventTriggerType.PointerEnter;
        // 离开指定区域状态
        Entry entry3 = new Entry();
        entry3.eventID = EventTriggerType.PointerExit;
        entry.callback.AddListener((data) =>
        {
            if (clickFunc != null)
            {
                clickFunc();
            }
        });
        eventTrigger.triggers.Add(entry);
        entry2.callback.AddListener((data) =>
        {
            if (pointInFunc != null)
            {
                pointInFunc();
            }
        });
        eventTrigger.triggers.Add(entry2);
        entry3.callback.AddListener((data) =>
        {
            if (pointOutFunc != null)
            {
                pointOutFunc();
            }
        });
        eventTrigger.triggers.Add(entry3);
    }

    // 把一个string类型作为键的字典转为int类型作为键的字典
    public static Dictionary<int, T> changeDictStringKeyToIntKey<T>(Dictionary<string, T> dict)
    {
        var tempDict = new Dictionary<int, T>();
        foreach (var strKey in dict.Keys)
        {
            if (int.TryParse(strKey, out int intKey))
            {
                // 将可以转成int类型的键的值复制到新字典中
                tempDict[intKey] = dict[strKey];
            }
        }
        return tempDict;
    }
    // 把一个int类型作为键的字典转为string类型作为键的字典
    public static Dictionary<string, T> changeDictIntKeyToStrKey<T>(Dictionary<int, T> dict)
    {
        var tempDict = new Dictionary<string, T>();
        foreach (var intKey in dict.Keys)
        {
            var strKey = intKey.ToString();
            // 将可以转成int类型的键的值复制到新字典中
            tempDict[strKey] = dict[intKey];
        }
        return tempDict;
    }

    // 从给定的列表随机抽取X个值
    // hasRepeatValue：是否可以有重复值
    public static List<T> getRandomValueFromList<T>(List<T> targetList, int num, bool hasRepeatValue = false)
    {
        if (num >= targetList.Count) {  return targetList; }
        var list = new List<T>();
        var tempList = new List<T>(targetList);
        for (var i = 0; i < num; i++)
        {
            if (tempList.Count <= 0) { break; }
            var randomIndex = getRandomIntNum(0, tempList.Count - 1);
            var targetValue = tempList[randomIndex];
            if (!hasRepeatValue)
            {
                tempList.Remove(targetValue);
            }
            list.Add(targetValue);
        }
        return list;
    }

    // 设置一个按钮为图片交换，并且设置各个状态的图片
    public static void loadBtnSprite(Button btn, Sprite norSprite=null, Sprite selectSprite=null, Sprite clickSprite=null, Sprite disableSprite=null)
    {
        btn.transition = Selectable.Transition.SpriteSwap;
        var img = btn.targetGraphic as Image;
        img.sprite = norSprite;
        SpriteState spriteState = new SpriteState
        {
            highlightedSprite = selectSprite,
            pressedSprite = clickSprite,
            disabledSprite = disableSprite
        };
        btn.spriteState = spriteState;
    }

    // 获取一个图集的sprite，不存在则从备用atlas里面找
    public static Sprite getSpriteByTwoAtlas(SpriteAtlas baseAtlas, string spriteName, SpriteAtlas otherAtlas= null)
    {
        var sprite = baseAtlas.GetSprite(spriteName);
        if (sprite == null && otherAtlas != null)
        {
            return otherAtlas.GetSprite(spriteName);// 如果备用也没找到则返回null算了
        }
        else
        {
            return sprite;
        }
    }
}

