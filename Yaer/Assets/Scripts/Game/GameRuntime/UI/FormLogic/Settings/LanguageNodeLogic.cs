using UnityEngine;

// 单个语言Node的脚本
public class LanguageNodeLogic : MonoBehaviour
{
    public LanguageEnumType languageType = LanguageEnumType.Chinese;

    public GameObject itemBg;
    public GameObject itemNormal;
    public GameObject itemSelect;
    public LanguageSelectLogic languageSelectLogic;

    bool hasSelect = false;
    // Start is called before the first frame update
    void Start()
    {
        itemBg.SetActive(false);

        GameTools.setObjPointTouchEvent(gameObject, () =>
        {
            languageSelectLogic.onSelectOneItemBtn(languageType);
        }, () =>
        {
            if (hasSelect) { return; }
            itemBg.SetActive(true);
        }, () =>
        {
            itemBg.SetActive(false);
        });
    }

    public void setSelectItem(bool isSelect=true)
    {
        if (hasSelect == isSelect) { return; }
        hasSelect = isSelect;
        itemBg.SetActive(false);
        itemNormal.SetActive(!hasSelect);
        itemSelect.SetActive(hasSelect);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
