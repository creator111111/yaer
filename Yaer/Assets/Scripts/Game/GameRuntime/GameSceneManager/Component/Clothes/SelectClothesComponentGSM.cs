using System.Collections.Generic;
using Game.GameMgr;
using Game.GameMgr.Component;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameRuntime.GameSceneManager.Base;
using Game.Static.Name.Settings;
using Game.Static.Utility.JsonReader;
using UnityEngine;

namespace Game.GameRuntime.GameSceneManager.Component
{
    /// <summary>
    /// 处理换装逻辑
    /// </summary>
    public class SelectClothesComponentGSM : BaseComponentGSM
    {
        [SerializeField] private GameObject yaerObj; // 骨架预制体
        [SerializeField] private string bonePrefabPath;
        [SerializeField] private TextAsset clothesConfig;
        private ClothesNameConfigHelper configHelper;
        private Dictionary<string, string> wearingClothesDic = new Dictionary<string, string>(); // 当前换上的衣服
        private Dictionary<string, List<string>> templateDic = new Dictionary<string, List<string>>(); // 键-部位，值-所有衣服数据， 键-衣服名，值-衣服数据 所有衣服的数据模板
        private Dictionary<string, List<GameObject>> yaerClothesDic = new Dictionary<string, List<GameObject>>(); // yaerObj的骨架和骨架下的衣服対象

        public override void OnInit(IGameSceneManager manager)
        {
            base.OnInit(manager);
            
            ReadPrefabTemplateData(bonePrefabPath);
            InitYaerObj();
            
            // 
            foreach (var p in SceneManager.GetArchiveData<PlayerClothesData>().GetAllClothesName())
            {
                wearingClothesDic.Add(p.Key, p.Value);
            }
            
            // 根据配置文件获取本地化名称
            configHelper = new ClothesNameConfigHelper();
            configHelper.Read(clothesConfig.text);
            
            // 根据数据显示衣服
            ShowClothes();
        }
        
        private void InitYaerObj()
        {
            yaerClothesDic.Clear();
            // 遍历衣服模板的骨架, 索引1开始剔除身体那个骨架
            for (var i = 1; i < yaerObj.transform.childCount; i++)
            {
                var clothesList = new List<GameObject>();
                var bone = yaerObj.transform.GetChild(i);

                // 遍历骨架下的所有衣服対象
                for (var j = 0; j < bone.childCount; j++) clothesList.Add(bone.GetChild(j).gameObject);

                // 骨架数据缓存进字典    
                yaerClothesDic.Add(bone.name, clothesList);
            }
        }

        /// <summary>
        ///     加载衣服模板数据
        /// </summary>
        private void ReadPrefabTemplateData(string assetPath)
        {
            templateDic.Clear();
            GameManager.GetGMComponent<ResComponentGM>().LoadAsset<GameObject>(assetPath, prefab =>
            {
                // 获取衣服模板的骨架, 索引1开始剔除身体那个骨架
                for (var i = 1; i < prefab.transform.childCount; i++)
                {
                    var boneTransform = prefab.transform.GetChild(i);
                    if (!templateDic.ContainsKey(boneTransform.name))
                    {
                        var clothesNames = new List<string>();

                        // 获取骨架下的所有衣服対象
                        for (var j = 0; j < boneTransform.childCount; j++)
                            if (!clothesNames.Contains(boneTransform.GetChild(j).name))
                                clothesNames.Add(boneTransform.GetChild(j).name);

                        // 骨架数据缓存进字典
                        templateDic.Add(boneTransform.name, clothesNames);
                    }
                }
            });
        }
        
        /// <summary>
        ///  获取模板中指定中骨骼下所有衣服名称数据
        /// </summary>
        public Dictionary<string, string> GetAllClothesNamesForBones(string boneName)
        {
            if (templateDic.ContainsKey(boneName) == false)
            {
                Debug.LogWarning("没有这个骨骼" + boneName);
                return null;
            }

            var res = new Dictionary<string, string>();
            var languageStrName = LanguageType.GetLanaguageString(GameManager.Instance.language);
            foreach (var c in templateDic[boneName]) res.Add(c, configHelper.GetClothesName(languageStrName, boneName, c));

            return res;
        }
        
        /// <summary>
        /// 获取已经穿上的衣服
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetWearingClothesData()
        {
            return wearingClothesDic;
        }
        
        /// <summary>
        /// 换上衣服
        /// </summary>
        /// <param name="boneName"></param>
        /// <param name="clothesName"></param>
        public void ChangingClothes(string boneName, string clothesName)
        {
            wearingClothesDic[boneName] = clothesName; // []
            
            ShowClothes();
        }

        private void ShowClothes()
        {
            // 根据数据显示衣服
            foreach (var pair in wearingClothesDic)
            {
                var clothesList = yaerClothesDic[pair.Key];

                // 失活对应骨架下的所有衣服
                for (var i = 0; i < clothesList.Count; i++) clothesList[i].SetActive(false);

                // 激活数据中穿上的衣服
                var clothes = clothesList.Find(x => x.name == pair.Value);
                if (clothes != null) clothes.SetActive(true);
            }
        }

        public void SaveWearingClothes()
        {
            // 保存数据
            var data = SceneManager.GetArchiveData<PlayerClothesData>();
            foreach (var pair in wearingClothesDic)
            {
                data.AddClothes(pair.Key, pair.Value);
            }
        }
    }
}