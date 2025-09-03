using UnityEngine;

// 游戏里面一些通用的颜色配置文件,游戏里面出现的颜色都需要读这里面配置好的颜色，防止游戏字体颜色太乱
public static class CommonColorConfig : object
{
    // =====================下面是一些通用常量
    public static readonly Color COLOR_BLACK = Color.black; // 黑色
    public static readonly Color COLOR_WHITE = Color.white; // 白色
    public static readonly Color COLOR_GRAY = Color.gray; // 灰色
    public static readonly Color COLOR_RED = Color.red; // 红色
    public static readonly Color COLOR_BLUE = Color.blue; // 蓝色
    public static readonly Color COLOR_GREEN = new Color(0.12f, 1, 0.20f, 1); // 绿色
    public static readonly Color COLOR_PURPLE = new Color(0.5f, 0, 0.5f, 1); // 紫色
    public static readonly Color COLOR_ORANGE = new Color(1f, 0.65f, 0, 1); // 橙色
    public static readonly Color COLOR_LIGHT_BLUE = new Color(0, 0.8f, 1, 1); // 浅蓝色
    public static readonly Color COLOR_LIGHT_PUPRLE = new Color(1, 0, 1, 1); // 亮紫色
    public static readonly Color COLOR_BLUE_2 = new Color(0, 0.6f, 1, 1); // 淡蓝色
    public static readonly Color COLOR_BLUE_3 = new Color(0, 1, 1, 1); // 青色

    // 不同属性伤害的颜色
    public static readonly Color COLOR_DAMAGE_NORMAL = Color.white; // 一般属性
    public static readonly Color COLOR_DAMAGE_FILE = Color.red; // 火属性
    public static readonly Color COLOR_DAMAGE_FROZEN = new Color(0, 0.1f, 1, 1); // 冰冻
    public static readonly Color COLOR_DAMAGE_GAS = new Color(0.7f, 0, 1, 1);// 气体（毒气）
    public static readonly Color COLOR_DAMAGE_ELECTRICAL = new Color(1, 1, 0, 1);// 电气
    public static readonly Color COLOR_DAMAGE_BEAM = new Color(0, 1, 0.8f, 1);// 光束
    public static readonly Color COLOR_DAMAGE_SOUND_WAVE = new Color(0.4f, 0, 1, 1);// 声波
    public static readonly Color COLOR_DAMAGE_MIND = new Color(1, 0, 1, 1);// 精神
    public static readonly Color COLOR_DAMAGE_CORROSION = Color.gray;// 腐蚀
    public static readonly Color COLOR_DAMAGE_WATER = new Color(0, 0.4f, 1, 1);//  水
    public static readonly Color COLOR_DAMAGE_WIND = new Color(0, 0.7f, 0, 1);// 风
    public static readonly Color COLOR_DAMAGE_DARK = new Color(0, 0, 0, 1);// 暗

    // 技能攻击远近的背景颜色
    public static readonly Color COLOR_SHORT_SKILL = new Color(0.8f, 0.1f, 0.2f); // 近距离技能颜色
    public static readonly Color COLOR_LOON_SKILL = new Color(0.1f, 0.3f, 1); // 远距离技能颜色

    // =====================下面是16进制用来给富文本

    public static string ColorToHex(Color color)
    {
        // 将浮点数转换为0-255范围的整数
        int r = Mathf.RoundToInt(color.r * 255);
        int g = Mathf.RoundToInt(color.g * 255);
        int b = Mathf.RoundToInt(color.b * 255);
        // 将RGB分量转换为16进制字符串
        return string.Format("#{0:X2}{1:X2}{2:X2}", r, g, b);
    }

    
}