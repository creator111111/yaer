using System;
using System.Collections.Generic;
using System.IO;
using Game.GameMgr.Component.Archive.ArchiveDataClass;
using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;
using Game.GameMgr.Manager.Path;
using GameFramework.CoreExtend.Serialiizer.Binary;
using UnityEngine;

namespace Game.GameMgr.Component.Archive
{
    /// <summary>
    ///     游戏存档数据管理器
    /// </summary>
    public class ArchiveManagerOld : MonoBehaviour, IArchiveManager
    {
        private bool tempArchive; // 当前是否使用临时存档
        private string archiveRootPath;
        private ArchiveInfo nowArchiveInfo; // 存档信息缓存
        private Dictionary<string, BaseArchiveData> archiveDataDic; // 所有存档数据缓存
        private List<ArchiveInfo> archiveInfosList; // 所有存档信息缓存

        public void Init()
        {
            archiveRootPath = PathManager.ARCHIVE_PATH;
            archiveInfosList = new List<ArchiveInfo>();
            archiveDataDic = new Dictionary<string, BaseArchiveData>();
        }

        public List<ArchiveInfo> GetAllArchiveInfo()
        {
            archiveInfosList.Clear();

            // 检查目录是否存在
            if (!Directory.Exists(archiveRootPath))
            {
                // 创建目录
                Directory.CreateDirectory(archiveRootPath);
                return archiveInfosList;
            }

            // 获取目录下的所有子目录
            var directories = Directory.GetDirectories(archiveRootPath);

            // 获取每个子目录下的存档信息文件
            foreach (var directory in directories)
            {
                var filesName = Directory.GetFiles(directory, "ArchiveInfo.ari");
                for (var i = 0; i < filesName.Length; i++)
                {
                    // 加载文件信息
                    var archiveInfo = BinarySystem.Instance.Load<ArchiveInfo>(filesName[i]);
                    archiveInfosList.Add(archiveInfo);
                }
            }

            if (archiveInfosList.Count > 0) archiveInfosList.Sort();

            return archiveInfosList;
        }

        public ArchiveInfo GetNowArchiveInfo()
        {
            return nowArchiveInfo;
        }

        public void SetNowArchiveInfo(ArchiveInfo archiveInfo)
        {
            nowArchiveInfo = archiveInfo;
        }

        public T GetArchiveData<T>() where T : BaseArchiveData, new()
        {
            var name = typeof(T).Name;
            
            // 临时存档直接new
            if (tempArchive)
            {
                if (archiveDataDic.ContainsKey(name))
                {
                    
                }
                var data = new T();
                archiveDataDic.Add(name, data);
                return new T();
            }
            
            // 返回信息文件
            if (typeof(T) == typeof(ArchiveInfo)) return nowArchiveInfo as T;


            // 判断是否已经加载过
            if (!archiveDataDic.ContainsKey(name))
                try
                {
                    // object data;
                    //
                    // // 没有正在使用的存档为新游戏
                    // if (nowArchiveInfo == null)
                    // {
                    //     data = new T();
                    //     Debug.Log("无存档数据，自动创建默认数据 " + typeof(T).Name);
                    // }
                    // else
                    // {
                    //     // 规定.ard为存档文件后缀
                    //     data = BinarySystem.Instance.Load<T>(archiveRootPath + nowArchiveInfo.guid + "/" + name + ".ard");
                    // }
                    //
                    // // 保存在字典
                    // archiveDataDic.Add(name, data as BaseArchiveData);
                }
                catch (Exception e)
                {
                    Debug.LogError("存档加载失败" + name);
                    Debug.LogError(e);
                    return default;
                }

            return archiveDataDic[name] as T;
        }


        #region 存档信息相关

        /// <summary>
        ///     获取指定存档信息文件
        /// </summary>
        /// <param name="guid"></param>
        private ArchiveInfo LoadArchiveInfo(string guid)
        {
            if (!File.Exists(archiveRootPath + $"/{guid}/ArchiveInfo.ari"))
            {
                Debug.Log($"{guid}无存档信息文件");
                return null;
            }

            var data = BinarySystem.Instance.Load<ArchiveInfo>(archiveRootPath + $"/{guid}/ArchiveInfo.ari");
            return data;
        }

        #endregion


        #region 保存相关

        /// <summary>
        ///     数据文件保存到硬盘
        /// </summary>
        private void SaveFiles()
        {
            // // 信息文件
            // BinarySystem.Instance.Save(archiveRootPath + $"{nowArchiveInfo.guid}/ArchiveInfo.ari", nowArchiveInfo);
            //
            // // 保存文件
            // foreach (var data in archiveDataDic)
            //     try
            //     {
            //         // 数据文件
            //         BinarySystem.Instance.Save(archiveRootPath + nowArchiveInfo.guid + "/" + data.Key + ".ard", data.Value);
            //     }
            //     catch (Exception e)
            //     {
            //         Debug.LogWarning($"{nameof(data.Key)}数据保存失败: {e}");
            //     }
        }

        #endregion


        #region 存档操作

        /// <summary>
        /// 创建一个临时存档
        /// </summary>
        public void CreateTempArchive()
        {
            tempArchive = true;
        }

        /// <summary>
        ///     卸载存档
        /// </summary>
        public void UnloadArchive()
        {
            nowArchiveInfo = null;
            archiveDataDic.Clear();
        }

        /// <summary>
        ///     删除存档
        /// </summary>
        public void DeleteArchive(string guid)
        {
            if (!Directory.Exists(archiveRootPath + $"/{guid}")) return;

            Directory.Delete(archiveRootPath + $"/{guid}", true);
        }

        /// <summary>
        ///     创建新存档
        /// </summary>
        public void CreateNewArchive()
        {
            // // 生成唯一标识符得到存档Guid
            // var guid = Guid.NewGuid();
            //
            // // 创建文件夹
            // Directory.CreateDirectory(archiveRootPath + guid);
            //
            // // 计算id
            // var id = 1;
            // var archiveInfos = GetAllArchiveInfo();
            // if (archiveInfos.Count > 0)
            // {
            //     archiveInfos.Sort();
            //     id = archiveInfos[archiveInfos.Count - 1].id + 1;
            // }
            //
            // // 创建存档信息文件
            // nowArchiveInfo = new ArchiveInfo
            // {
            //     guid = guid.ToString(),
            //     id = id,
            //     chapterId = 0,
            //     createDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            //     gameDuration = "0:00:00"
            // };
            //
            // archiveInfosList.Add(nowArchiveInfo);
            // archiveInfosList.Sort();
        }

        /// <summary>
        ///     加载指定存档
        /// </summary>
        public void LoadArchive(string guid)
        {
            // 清空缓存
            archiveDataDic.Clear();
            nowArchiveInfo = null;

            // 检查目录是否存在
            if (!Directory.Exists(archiveRootPath + guid))
            {
                Debug.LogError("不存在该目录" + archiveRootPath + guid);
                return;
            }

            // 获取目录下的所有存档文件
            var filesName = Directory.GetFiles(archiveRootPath + guid + "/", "*.ard");
            for (var i = 0; i < filesName.Length; i++)
                try
                {
                    // 加载原始文件信息
                    var rawData = BinarySystem.Instance.LoadRaw(filesName[i]);

                    if (!archiveDataDic.ContainsKey(rawData.GetType().Name))
                    {
                        Debug.Log($"load data success at {rawData.GetType().Name}");
                        archiveDataDic.Add(rawData.GetType().Name, rawData as BaseArchiveData);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError("存档加载失败 " + filesName[i]);
                    Debug.LogError(e);
                }

            // 获取存档信息文件
            nowArchiveInfo = LoadArchiveInfo(guid);
        }

        /// <summary>
        ///     保存存档
        /// </summary>
        public void SaveArchive()
        {
            SaveFiles();
        }

        /// <summary>
        ///     另存为存档
        /// </summary>
        public void SaveAsNewArchive()
        {
            // if (nowArchiveInfo == null)
            // {
            //     // 新建存档
            //     CreateNewArchive();
            //     return;
            // }
            //
            // // 生成唯一标识符得到存档Guid
            // var guid = Guid.NewGuid();
            //
            // // 创建文件夹
            // Directory.CreateDirectory(archiveRootPath + guid);
            //
            // // 计算id
            // var id = archiveInfosList.Count > 0 ? archiveInfosList[archiveInfosList.Count - 1].id + 1 : 1;
            //
            // // 复制存档信息文件
            // var newArchiveInfo = new ArchiveInfo
            // {
            //     guid = guid.ToString(),
            //     id = id,
            //     chapterId = nowArchiveInfo.chapterId,
            //     createDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            //     gameDuration = nowArchiveInfo.gameDuration
            // };
            //
            // nowArchiveInfo = newArchiveInfo;
            // archiveInfosList.Add(nowArchiveInfo);
            // archiveInfosList.Sort();
            // SaveFiles();
        }

        /// <summary>
        ///     覆盖存档
        /// </summary>
        public void CoverArchive(string toGuid)
        {
        //     // 获取被覆盖的存档信息文件
        //     var toArchiveInfo = LoadArchiveInfo(toGuid);
        //
        //     // 覆盖存档信息文件
        //     toArchiveInfo.gameDuration = nowArchiveInfo.gameDuration;
        //     toArchiveInfo.chapterId = nowArchiveInfo.chapterId;
        //     toArchiveInfo.createDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        //
        //     // 使用被覆写的存档信息文件
        //     nowArchiveInfo = toArchiveInfo;
        //     SaveFiles();
        }

        #endregion
    }
}