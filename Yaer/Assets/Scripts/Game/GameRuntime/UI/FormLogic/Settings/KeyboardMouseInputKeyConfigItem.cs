using Game.GameMgr;
using Game.GameMgr.Manager.Settings;
using Game.GameMgr.Manager.Settings.Helper;
using Game.GameRuntime.UI.FormLogic.Settings;
using Game.Static.Enum;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using GameFramework.UnityRuntime.UI;

public class KeyboardMouseInputKeyConfigItem : MonoBehaviour
{
    [SerializeField]
    private ControlInputType controlInputType;

    private Button btn;
    private Text KeyCodeText;
    private bool isWaitingForInput = false;
    private string originalText;
    public UIFormLogic parentUILogic;
    private static SettingsConfigData configData = null;
    private static SettingsConfigData ConfigData
    {
        get
        {
            if (configData == null)
            {
                configData = GameManager.GetManager<SettingManager>().LoadSetting<SettingsConfigData>();
            }
            return configData;
        }
    }

    private void Awake()
    {
        btn = GetComponent<Button>();
        KeyCodeText = transform.Find("Text").GetComponent<Text>();
        
        // 绑定按钮点击事件
        btn.onClick.AddListener(OnButtonClick);
    }

    private void OnEnable()
    {
        Refresh();
    }

    private void Update()
    {
        if (isWaitingForInput)
        {
            CheckForKeyInput();
        }
    }

    private void Refresh()
    {
        if (ConfigData.KeyboardMouseInputConfig.TryGetValue(controlInputType, out var keycode))
        {
            KeyCodeText.gameObject.SetActive(true);
            string text=keycode.ToString();
            if (text.Equals("Escape"))
                text = "Esc";
            KeyCodeText.text = text;
			originalText = text;
        }
        else
        {
            KeyCodeText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 按钮点击事件 - 开始等待新按键输入
    /// </summary>
    private void OnButtonClick()
    {
        UIUtils.PlayBtnAudio(parentUILogic);
        if (!isWaitingForInput)
        {
            StartWaitingForInput();
        }
    }

    /// <summary>
    /// 开始等待按键输入
    /// </summary>
    private void StartWaitingForInput()
    {
        isWaitingForInput = true;
        KeyCodeText.text = "按下任意键...";
        KeyCodeText.color = Color.yellow;
        
        // 禁用按钮防止重复点击
        btn.interactable = false;
        
        // 5秒后自动取消等待
        StartCoroutine(CancelWaitingAfterTimeout());
    }

    /// <summary>
    /// 检测按键输入
    /// </summary>
    private void CheckForKeyInput()
    {
        // 检测键盘按键
        foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(keyCode))
            {
                // 排除一些不适合绑定的按键
                if (IsValidKeyCode(keyCode))
                {
                    SetNewKeyCode(keyCode);
                    return;
                }
            }
        }

        // ESC键取消绑定
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CancelWaiting();
        }
    }

    /// <summary>
    /// 检查按键是否有效
    /// </summary>
    private bool IsValidKeyCode(KeyCode keyCode)
    {
        // 排除一些系统按键和无效按键
        switch (keyCode)
        {
            case KeyCode.None:
            case KeyCode.Clear:
            case KeyCode.Pause:
            case KeyCode.Print:
            case KeyCode.SysReq:
                return false;
            default:
                return true;
        }
    }

    /// <summary>
    /// 设置新的按键
    /// </summary>
    private void SetNewKeyCode(KeyCode newKeyCode)
    {
        // 检查是否与其他功能冲突
        if (CheckForConflicts(newKeyCode))
        {
            // 如果有冲突，显示警告并取消
            ShowConflictWarning(newKeyCode);
            CancelWaiting();
            return;
        }

        // 使用KeyBindingHelper设置按键
        KeyBindingHelper.SetKeyBinding(controlInputType, newKeyCode);
        
        // 更新显示
        KeyCodeText.text = newKeyCode.ToString();
        KeyCodeText.color = Color.black;
        originalText = newKeyCode.ToString();
        
        // 结束等待状态
        EndWaiting();
        
        Debug.Log($"按键 {controlInputType} 已重新绑定为: {newKeyCode}");
    }

    /// <summary>
    /// 检查按键冲突
    /// </summary>
    private bool CheckForConflicts(KeyCode newKeyCode)
    {
        foreach (var kvp in ConfigData.KeyboardMouseInputConfig)
        {
            if (kvp.Key != controlInputType && kvp.Value == newKeyCode)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 显示冲突警告
    /// </summary>
    private void ShowConflictWarning(KeyCode conflictKeyCode)
    {
        Debug.LogWarning($"按键 {conflictKeyCode} 已被其他功能使用！");
        KeyCodeText.text = "按键冲突!";
        KeyCodeText.color = Color.red;
        
        // 2秒后恢复原始显示
        StartCoroutine(RestoreOriginalTextAfterDelay(2f));
    }

    /// <summary>
    /// 取消等待
    /// </summary>
    private void CancelWaiting()
    {
        KeyCodeText.text = originalText;
        KeyCodeText.color = Color.black;
        EndWaiting();
    }

    /// <summary>
    /// 结束等待状态
    /// </summary>
    private void EndWaiting()
    {
        isWaitingForInput = false;
        btn.interactable = true;
        StopAllCoroutines();
    }

    /// <summary>
    /// 超时取消等待
    /// </summary>
    private IEnumerator CancelWaitingAfterTimeout()
    {
        yield return new WaitForSeconds(5f);
        if (isWaitingForInput)
        {
            CancelWaiting();
        }
    }

    /// <summary>
    /// 延迟恢复原始文本
    /// </summary>
    private IEnumerator RestoreOriginalTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!isWaitingForInput)
        {
            KeyCodeText.text = originalText;
            KeyCodeText.color = Color.white;
        }
    }

    /// <summary>
    /// 重置为默认按键
    /// </summary>
    public void ResetToDefault()
    {
        var defaultConfig = new SettingsConfigData();
        if (defaultConfig.KeyboardMouseInputConfig.TryGetValue(controlInputType, out var defaultKey))
        {
            KeyBindingHelper.SetKeyBinding(controlInputType, defaultKey);
            Refresh();
        }
    }
}
