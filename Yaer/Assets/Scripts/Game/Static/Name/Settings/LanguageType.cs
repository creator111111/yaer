using System.Collections;

public enum LanguageEnumType
{
    Chinese,
    English,
    Japanese,
}

namespace Game.Static.Name.Settings
{
    public static class LanguageType
    {
        public const LanguageEnumType Chinese = LanguageEnumType.Chinese;
        public const LanguageEnumType English = LanguageEnumType.English;
        public const LanguageEnumType Japanese = LanguageEnumType.Japanese;

        public static string GetLanaguageResTag(LanguageEnumType languageType)
        {
            switch (languageType) {
                case Chinese:return "";
                case English:return "_en";
                case Japanese:return "_jp";
                default:
                    return "";
            }
        }

        public static string GetLanaguageString(LanguageEnumType languageType)
        {
            switch (languageType)
            {
                case Chinese: return "zh-CN";
                case English: return "en-US";
                case Japanese: return "jp";
                default:
                    return "";
            }
        }
    }

    // 语言对应的图片资源路径
    
}