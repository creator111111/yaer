using GameFramework.CoreExtend.Base;
using GameFramework.CoreExtend.Resource;

namespace GameFramework.CoreExtend.Config
{
    /// <summary>全局配置管理器接口。</summary>
    public interface IConfigSystem : IGFExtendSystem
    {
        /// <summary>获取全局配置项数量。</summary>
        int Count { get; }

        /// <summary>设置资源管理器。</summary>
        /// <param name="system">资源管理器。</param>
        void SetResourceSystem(IResourceSystem system);

        /// <summary>检查是否存在指定全局配置项。</summary>
        /// <param name="configName">要检查全局配置项的名称。</param>
        /// <returns>指定的全局配置项是否存在。</returns>
        bool HasConfig(string configName);

        /// <summary>从指定全局配置项中读取布尔值。</summary>
        /// <param name="configName">要获取全局配置项的名称。</param>
        /// <returns>读取的布尔值。</returns>
        bool GetBool(string configName, string key);

        /// <summary>从指定全局配置项中读取布尔值。</summary>
        /// <param name="configName">要获取全局配置项的名称。</param>
        /// <param name="defaultValue">当指定的全局配置项不存在时，返回此默认值。</param>
        /// <returns>读取的布尔值。</returns>
        bool GetBool(string configName, string key, bool defaultValue);

        /// <summary>从指定全局配置项中读取整数值。</summary>
        /// <param name="configName">要获取全局配置项的名称。</param>
        /// <returns>读取的整数值。</returns>
        int GetInt(string configName, string key);

        /// <summary>从指定全局配置项中读取整数值。</summary>
        /// <param name="configName">要获取全局配置项的名称。</param>
        /// <param name="defaultValue">当指定的全局配置项不存在时，返回此默认值。</param>
        /// <returns>读取的整数值。</returns>
        int GetInt(string configName, string key, int defaultValue);

        /// <summary>从指定全局配置项中读取浮点数值。</summary>
        /// <param name="configName">要获取全局配置项的名称。</param>
        /// <returns>读取的浮点数值。</returns>
        float GetFloat(string configName, string key);

        /// <summary>从指定全局配置项中读取浮点数值。</summary>
        /// <param name="configName">要获取全局配置项的名称。</param>
        /// <param name="defaultValue">当指定的全局配置项不存在时，返回此默认值。</param>
        /// <returns>读取的浮点数值。</returns>
        float GetFloat(string configName, string key, float defaultValue);

        /// <summary>从指定全局配置项中读取字符串值。</summary>
        /// <param name="configName">要获取全局配置项的名称。</param>
        /// <returns>读取的字符串值。</returns>
        string GetString(string configName, string key);

        /// <summary>从指定全局配置项中读取字符串值。</summary>
        /// <param name="configName">要获取全局配置项的名称。</param>
        /// <param name="defaultValue">当指定的全局配置项不存在时，返回此默认值。</param>
        /// <returns>读取的字符串值。</returns>
        string GetString(string configName, string key, string defaultValue);

        /// <summary>增加指定全局配置项。</summary>
        /// <param name="configName">要增加全局配置项的名称。</param>
        /// <param name="configValue">全局配置项的值。</param>
        /// <returns>是否增加全局配置项成功。</returns>
        bool AddConfig(string configName, ConfigTable table);

        /// <summary>移除指定全局配置项。</summary>
        /// <param name="configName">要移除全局配置项的名称。</param>
        /// <returns>是否移除全局配置项成功。</returns>
        bool RemoveConfig(string configName);

        /// <summary>清空所有全局配置项。</summary>
        void RemoveAllConfigs();
    }
}