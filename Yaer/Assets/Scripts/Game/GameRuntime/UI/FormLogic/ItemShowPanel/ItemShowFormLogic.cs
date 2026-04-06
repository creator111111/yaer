using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameRuntime.UI.FormLogic.Base;
using Game.GameRuntime.UI.FormLogic.Menu;
using Game.GameRuntime.UI.FormLogic.Menu.MainItemPage;
using Game.Static.Name.Settings;
using System;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;


public class ItemShowFormLogic : BaseUIFormLogic
{
    /// <summary>ItemShowPanel ??????????????????????????—¥??</summary>
    public static event Action OnPanelOpened;
    /// <summary>ItemShowPanel ?????????Esc??ClosePanel ?????</summary>
    public static event Action OnPanelClosed;
    [SerializeField] private MenuFormMainItemPage mainItemPage;
    [SerializeField] private DetailFormLogic detailForm;
    public Button closeBtn;

    SpriteAtlas returnBtnAtlas;
    protected internal override void OnInit(object userData)
    {
        base.OnInit(userData);

        closeBtn.onClick.AddListener(() =>
        {
            CloseForm();
        });

        LoadAtlas(1);
    }

    protected override void LoadAtlas(int targetAtlasCount)
    {
        base.LoadAtlas(targetAtlasCount);
        var path = "Assets/GameRes/Atlas/CommonBtn/returnBtnAtlas.spriteatlas";
        GameManager.GetGMComponent<ResComponentGM>().LoadAsset<SpriteAtlas>(path, atlas =>
        {
            if (atlas == null) { return; }
            if (returnBtnAtlas != null) { return; }
            returnBtnAtlas = atlas;
            loadAtlasCallFunc();
        });
    }
    public override void UpdateUI()
    {
        base.UpdateUI();
        closeBtn.gameObject.SetActive(true);
        var curResTag = GameManager.GetCurLanguageResTag();
        if (GameManager.Instance.language == LanguageEnumType.Japanese)
        {
            // ?????????????
            curResTag = LanguageType.GetLanaguageResTag(LanguageEnumType.English);
        }
        var norResName = "returnNor" + curResTag;
        var clickResName = "returnClick" + curResTag;
        var selectResName = "returnSelect" + curResTag;
        var norResSprite = returnBtnAtlas.GetSprite(norResName);
        var clickResSprite = returnBtnAtlas.GetSprite(clickResName);
        var selectResSprite = returnBtnAtlas.GetSprite(selectResName);
        GameTools.loadBtnSprite(closeBtn, norResSprite, selectResSprite, clickResSprite);
    }

    public void initData(MenuFormProxy menuFormProxy, MenuFormLogic menuFormLogic)
    {
        mainItemPage.OnInit(menuFormProxy, menuFormLogic);
        mainItemPage.OnOpen();
    }
    // Start is called before the first frame update
    protected override void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    protected internal override void OnOpen(object userData)
    {
        base.OnOpen(userData);
        detailForm.gameObject.SetActive(false);
        closeBtn.gameObject.SetActive(false);
        OnPanelOpened?.Invoke();
    }

    protected internal override void OnClose(bool isShutdown, object userData)
    {
        base.OnClose(isShutdown, userData);
        OnPanelClosed?.Invoke();
    }

    public override void CloseFormOnEsc()
    {
        base.CloseFormOnEsc();
    }
}
