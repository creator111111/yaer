using System.Collections.Generic;
using Game.GameMgr.Manager.Base;
using Game.GameMgr.Manager.Res;
using Game.Static.Name.Settings;
using Game.Static.Utility.JsonReader;
using UnityEngine;

namespace Game.GameMgr.Manager.Clothes
{
    public class ClothesManager :MonoBehaviour , IManager
    {
        private IGameResourcesManager resourcesManager;
        // 键-部位，值-所有衣服数据， 键-衣服名，值-衣服数据
        private Dictionary<string, List<string>> clothingTemplateDataDic = new Dictionary<string, List<string>>(); // 所有衣服的数据模板
        
        public bool IsChanging { get; private set; }

        public void Init()
        {
        }

        public void InitData(string templateName)
        {
            clothingTemplateDataDic.Clear();
            // ReadPrefabTemplateData(templateName);
            IsChanging = true;
        }

        public void ExitChanging()
        {
            IsChanging = false;
            Clear();
        }

        #region 加载数据

        /// <summary>
        ///     加载衣服模板数据
        /// </summary>
        public void ReadPrefabTemplateData(string assetPath)
        {
            clothingTemplateDataDic.Clear();
            resourcesManager.LoadAsset<GameObject>(assetPath, prefab =>
            {
                // 获取衣服模板的骨架, 索引1开始剔除身体那个骨架
                for (var i = 1; i < prefab.transform.childCount; i++)
                {
                    var boneTransform = prefab.transform.GetChild(i);
                    if (!clothingTemplateDataDic.ContainsKey(boneTransform.name))
                    {
                        var clothesName = new List<string>();

                        // 获取骨架下的所有衣服対象
                        for (var j = 0; j < boneTransform.childCount; j++)
                            if (!clothesName.Contains(boneTransform.GetChild(j).name))
                                clothesName.Add(boneTransform.GetChild(j).name);

                        // 骨架数据缓存进字典
                        clothingTemplateDataDic.Add(boneTransform.name, clothesName);
                    }
                }
            });
        }

        #endregion

        private void OnLoadGame()
        {
            Clear();
        }

        private void OnSaveGame()
        {
        }

        private void Clear()
        {
            clothingTemplateDataDic.Clear();
        }

        #region 外部获取数据

        /// <summary>
        ///     获取骨架上所有衣服名称数据
        /// </summary>
        public Dictionary<string, List<string>> GetAllClothesNames()
        {
            return clothingTemplateDataDic;
        }

        /// <summary>
        ///     获取模板中指定中骨骼下所有衣服名称数据
        /// </summary>
        public Dictionary<string, string> GetAllClothesNamesForBones(string boneName)
        {
            if (clothingTemplateDataDic.ContainsKey(boneName) == false)
            {
                Debug.LogWarning("没有这个骨骼" + boneName);
                return null;
            }

            // 根据配置文件获取本地化名称
            var r = new ClothesNameConfigHelper();
            // r.Read();

            var res = new Dictionary<string, string>();
            var languageStrName = LanguageType.GetLanaguageString(GameManager.Instance.language);
            foreach (var c in clothingTemplateDataDic[boneName]) res.Add(c, r.GetClothesName(languageStrName, boneName, c));

            return res;
        }

        #endregion
    }
}