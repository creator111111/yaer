using System.Collections.Generic;
using UnityEngine;

public class KeyCodeStrConfig
{
    private static readonly Dictionary<KeyCode, string> keyCodeMap = new Dictionary<KeyCode, string>()
    {
        // 字母键
        {KeyCode.A, "A"},
        {KeyCode.B, "B"},
        {KeyCode.C, "C"},
        {KeyCode.D, "D"},
        {KeyCode.E, "E"},
        {KeyCode.F, "F"},
        {KeyCode.G, "G"},
        {KeyCode.H, "H"},
        {KeyCode.I, "I"},
        {KeyCode.J, "J"},
        {KeyCode.K, "K"},
        {KeyCode.L, "L"},
        {KeyCode.M, "M"},
        {KeyCode.N, "N"},
        {KeyCode.O, "O"},
        {KeyCode.P, "P"},
        {KeyCode.Q, "Q"},
        {KeyCode.R, "R"},
        {KeyCode.S, "S"},
        {KeyCode.T, "T"},
        {KeyCode.U, "U"},
        {KeyCode.V, "V"},
        {KeyCode.W, "W"},
        {KeyCode.X, "X"},
        {KeyCode.Y, "Y"},
        {KeyCode.Z, "Z"},
        // 数字键
        {KeyCode.Alpha0, "0"},
        {KeyCode.Alpha1, "1"},
        {KeyCode.Alpha2, "2"},
        {KeyCode.Alpha3, "3"},
        {KeyCode.Alpha4, "4"},
        {KeyCode.Alpha5, "5"},
        {KeyCode.Alpha6, "6"},
        {KeyCode.Alpha7, "7"},
        {KeyCode.Alpha8, "8"},
        {KeyCode.Alpha9, "9"},

        // ... 添加所有数字键
        
        // 功能键
        {KeyCode.Space, "Space"},
        {KeyCode.Return, "Enter"},
        {KeyCode.Backspace, "Backspace"},
        {KeyCode.Tab, "Tab"},
        {KeyCode.Escape, "Esc"},
        {KeyCode.LeftShift, "Left Shift"},
        {KeyCode.RightShift, "Right Shift"},
        {KeyCode.LeftControl, "Left Ctrl"},
        {KeyCode.RightControl, "Right Ctrl"},
        {KeyCode.LeftAlt, "Left Alt"},
        {KeyCode.RightAlt, "Right Alt"},
        
        // 方向键
        {KeyCode.UpArrow, "↑"},
        {KeyCode.DownArrow, "↓"},
        {KeyCode.LeftArrow, "←"},
        {KeyCode.RightArrow, "→"},
        
        // 符号键
        {KeyCode.Comma, ","},
        {KeyCode.Period, "."},
        {KeyCode.Slash, "/"},
        {KeyCode.BackQuote, "`"},
        {KeyCode.LeftBracket, "["},
        {KeyCode.RightBracket, "]"},
        {KeyCode.Backslash, "\\"},
        {KeyCode.Semicolon, ";"},
        {KeyCode.Quote, "'"},
        {KeyCode.Minus, "-"},
        {KeyCode.Equals, "="},
        {KeyCode.Plus, "+"},
        
        // 小键盘
        {KeyCode.Keypad0, "Num 0"},
        {KeyCode.Keypad1, "Num 1"},
        {KeyCode.KeypadPlus, "Num +"},
        {KeyCode.KeypadMinus, "Num -"},
        
        // 其他
        {KeyCode.CapsLock, "Caps Lock"},
        {KeyCode.Numlock, "Num Lock"},
        {KeyCode.ScrollLock, "Scroll Lock"},
        {KeyCode.Print, "Print Screen"},
        {KeyCode.Insert, "Insert"},
        {KeyCode.Delete, "Delete"},
        {KeyCode.Home, "Home"},
        {KeyCode.End, "End"},
        {KeyCode.PageUp, "Page Up"},
        {KeyCode.PageDown, "Page Down"},
        {KeyCode.Pause, "Pause"},
        {KeyCode.Break, "Break"},
        // 鼠标
        { KeyCode.Mouse0, "Mouse0" },
        { KeyCode.Mouse1, "Mouse1" },
    };

    public static string GetKeyString(KeyCode keyCode)
    {
        // 尝试从字典获取
        if (keyCodeMap.TryGetValue(keyCode, out string keyString))
        {
            return keyString;
        }

        // 对于没有映射的键，使用默认名称
        return keyCode.ToString();
    }

    // 获取修饰键组合字符串
    public static string GetModifiedKeyString(KeyCode mainKey, bool shift = false, bool ctrl = false, bool alt = false)
    {
        List<string> parts = new List<string>();

        if (ctrl) parts.Add("Ctrl");
        if (alt) parts.Add("Alt");
        if (shift) parts.Add("Shift");

        parts.Add(GetKeyString(mainKey));

        return string.Join(" + ", parts);
    }
}