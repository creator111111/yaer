using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameFramework;
using System.IO;

namespace UnityGameFramework.Editor.ResourceTools
{
    /// <summary>
    /// 提供 GameFramework 配置文件路径的静态类。
    /// </summary>
    public static class GameFrameworkConfigs
    {
        /// <summary>
        /// 资源集合配置文件路径。
        /// </summary>
        [ResourceCollectionConfigPath]
        public static string ResourceCollectionConfig = Utility.Path.GetRegularPath(
            Path.Combine(Application.dataPath, "Editor/GFAssetBundleSettings/xml/ResourceCollection.xml"));

        /// <summary>
        /// 资源编辑器配置文件路径。
        /// </summary>
        [ResourceEditorConfigPath]
        public static string ResourceEditorConfig = Utility.Path.GetRegularPath(
            Path.Combine(Application.dataPath, "Editor/GFAssetBundleSettings/xml/ResourceEditor.xml"));

        /// <summary>
        /// 资源构建器配置文件路径。
        /// </summary>
        [ResourceBuilderConfigPath]
        public static string ResourceBuilderConfig = Utility.Path.GetRegularPath(
            Path.Combine(Application.dataPath, "Editor/GFAssetBundleSettings/xml/ResourceBuilder.xml"));
    }
}