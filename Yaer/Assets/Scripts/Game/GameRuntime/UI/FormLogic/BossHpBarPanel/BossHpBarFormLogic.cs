using Game.GameRuntime.Entities.Component.Health;
using Game.GameRuntime.Entities.Monster;
using Game.GameRuntime.UI.FormLogic.Base;
using GameFramework.CoreExtend.Component;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

// BOSS血条显示界面
public class BossHpBarFormLogic : BaseUIFormLogic
{
    public GameObject root;
    public GameObject hpSlider;
    public GameObject imgBar_2;
    public GameObject hpNode; // 所有HP相关UI的父节点，用于修改血条的位置
    public GameObject imgLightNode;
    BaseMonster monsterLogic; // 绑定的怪物
    HealthComponent healthComponent; 
    float lastHp;
    float maxHp;
    float curHp;
    bool hasChangeHp = false;
    float hpDistance;
    bool isAutoClosePanelInHpZero = false; // 是否在HP为0时自动关闭血条界面
    bool isPauseUpdate = false;
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }
    // Update is called once per frame
    void Update()
    {
        if (isPauseUpdate) return;
        if (isAutoClosePanelInHpZero && curHp <= 0)
        {
            AutoClosePanel();
            return;
        }
        if (!hasChangeHp) { return; }
        if (lastHp > curHp)
        {
            lastHp -= Time.deltaTime * hpDistance * 2;
            if (lastHp <= curHp)
            {
                hasChangeHp = false;
                lastHp = curHp;
            }
        }
        else if (lastHp < curHp)
        {
            lastHp -= Time.deltaTime * hpDistance * 2;
            if (lastHp <= curHp)
            {
                hasChangeHp = false;
                lastHp = curHp;
            }
        }
        // 刷新血条UI
        imgBar_2.GetComponent<Image>().fillAmount = lastHp / maxHp;
    }

    protected internal override void OnInit(object userData)
    {
        base.OnInit(userData);
    }

    protected internal override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        
        monsterLogic = (BaseMonster)userData;
        if (monsterLogic == null)
        {
            Debug.LogWarning("=================HP界面未设置绑定的怪物对象!!!!");
            return;
        }
        healthComponent = monsterLogic.componentSystem.TryGetComponent<HealthComponent>();
        healthComponent.onHpChange += OnHPChanged;
        if (healthComponent != null)
        {
            maxHp = monsterLogic.GetMonsterMaxHp();
            curHp = maxHp;
            lastHp = curHp;
            
        }
        imgLightNode.SetActive(curHp >= maxHp);
        // 淡入显示界面
        root.GetComponent<CanvasGroup>().alpha = 0;
        GameActionMgr.runFadeAction(root, 1, 2);
        // 初始刷新一次HP区域
        OnHPChanged(curHp);
    }

    // 部分特殊怪物需要手动传入HP和MAXHP
    public void UpdateHpAndMaxHp(float curHp, float maxHp)
    {
        this.maxHp = maxHp;
        this.curHp = curHp;
    }

    void OnHPChanged(float curMonsterHp)
    {
        curHp = monsterLogic.GetMonsterCurHp();
        hpDistance = Math.Abs(curHp - lastHp);
        hasChangeHp = true;
        imgLightNode.SetActive(curHp < maxHp);
        hpSlider.GetComponent<Slider>().value = curHp / maxHp;
    }


    public void SetAutoCloseOnHpInZero(bool v)
    {
        isAutoClosePanelInHpZero = v;
    }

    private void AutoClosePanel()
    {
        isPauseUpdate = true;
        root.GetComponent<CanvasGroup>().alpha = 1;
        var act = GameActionMgr.runFadeAction(root, 0, 2);
        act.onComplete = () =>
        {
            UIUtils.ClosePanel("BossHpBarPanel");
        };
    }
}
