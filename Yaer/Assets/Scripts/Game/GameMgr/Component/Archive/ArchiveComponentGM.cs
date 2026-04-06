using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Game.GameMgr.Component.Archive.ArchiveDataClass;
using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using Game.GameMgr.Component.Base;
using GameFramework.FileSystem;
using GameFramework.UnityRuntime.FileSystem;
using UnityEngine;

namespace Game.GameMgr.Component.Archive
{
    /// <summary>
    /// 处理存档
    /// 每个存档使用一个独立的文件夹和对应的虚拟文件系统
    /// </summary>
    public class ArchiveComponentGM : BaseComponentGM
    {
        /// <summary>
        /// 当前存档数据版本
        /// </summary>
        public const int CurrentDataVersion = 1;

        private FileSystemComponent fileSystemComponent;

        // 虚拟文件系统文件名
        private const string FileSystemName = "SaveSystem.dat";

        // 存档数据在虚拟文件系统中的文件名
        private const string SaveFileName = "MasterSave.dat";

        // 存档基本信息文件名
        private const string ArchiveInfoFileName = "ArchiveInfo.dat";

        // 当前存档对应的物理文件夹路径
        private string currentArchiveFolder;

        private bool isTempArchive;
        private ArchiveInfo archiveInfo; // 存档基本信息
        private IFileSystem saveFileSystem; // 当前存档对应的虚拟文件系统
        private MasterGameData masterGameData; // 存档数据
        private Dictionary<Type, BaseArchiveData> archiveDataDic = new Dictionary<Type, BaseArchiveData>(); // 加载过缓存
        private List<ArchiveDirectoryInfo> archiveDirectoryInfoList = new List<ArchiveDirectoryInfo>(); // 存档文件夹信息

        private float CurrentArchivePlayTime = 0;

        public override void OnInit()
        {
            base.OnInit();

            // 获取 Game Framework 文件系统组件
            fileSystemComponent = GameManager.GetGFComponent<FileSystemComponent>();

            // 初始化所有存档的根目录（存放所有存档文件夹）
            string saveRootPath = Path.Combine(Application.persistentDataPath, "Save");
            if (!Directory.Exists(saveRootPath))
            {
                Directory.CreateDirectory(saveRootPath);
            }
        }

        public override void OnUpdate()
        {
            base.OnUpdate();
            if (archiveInfo != null)
            {
                CurrentArchivePlayTime += Time.deltaTime;
            }
        }

        /// <summary>
        /// 新游戏时未保存的临时存档
        /// </summary>
        public void CreateTempGameArchive()
        {
            isTempArchive = true;
            archiveInfo = new ArchiveInfo();
            masterGameData = new MasterGameData();
            CurrentArchivePlayTime = archiveInfo.playTime;
        }
        
        public void ClearNowArchive()
        {
            isTempArchive = false;
            archiveInfo = null;
            CurrentArchivePlayTime = 0;
            masterGameData = null;
            if (saveFileSystem != null)
            {
                fileSystemComponent.DestroyFileSystem(saveFileSystem, false);
                saveFileSystem = null;
            }
            archiveDataDic.Clear();
        }

        /// <summary>
        /// 加载存档
        /// </summary>
        public void LoadArchive(string guid)
        {
            var archiveDirectoryInfo = archiveDirectoryInfoList.Find(a => a.info.guid == guid);
            if (archiveDirectoryInfo == null) return;

            currentArchiveFolder = archiveDirectoryInfo.path;

            // 加载存档信息
            archiveInfo = LoadArchiveInfo(Path.Combine(currentArchiveFolder, ArchiveInfoFileName));
            CurrentArchivePlayTime = archiveInfo.playTime;

            // 加载文件系统
            InitArchive(currentArchiveFolder);

            // 加载存档数据
            masterGameData = LoadGameData();
            
            // 清除加载过的缓存
            archiveDataDic.Clear();
        }

        /// <summary>
        /// 临时存档持久化
        /// </summary>
        private void SaveTempGameArchive()
        {
            archiveInfo.id = GetArchiveID();

            archiveInfo.createTime = DateTime.Now;
            archiveInfo.playTime = CurrentArchivePlayTime;
            archiveInfo.currentSceneName = GetData<PlayerMapData>().GetNowPlace();
            // 保存存档信息
            SaveArchiveInfo(archiveInfo);

            // 创建新文件系统
            InitArchive(archiveInfo.GetCreateTimeStr());

            // 游戏数据写入存档
            SaveGameData();

            archiveDirectoryInfoList.Add(new ArchiveDirectoryInfo()
            {
                path = currentArchiveFolder,
                info = archiveInfo
            });
        }

        /// <summary>
        /// 保存新存档
        /// </summary>
        public void SaveNewArchive()
        {
            if (isTempArchive)
            {
                // 临时数据保存
                SaveTempGameArchive();
                isTempArchive = false;
            }
            else
            {
                // 保存为新存档
                SaveAsNewArchive();
            }
        }

        /// <summary>
        /// 保存为新存档
        /// </summary>
        private void SaveAsNewArchive()
        {
            ArchiveInfo newArchiveInfo = new ArchiveInfo();
            newArchiveInfo.createTime = DateTime.Now;
            newArchiveInfo.guid = Guid.NewGuid().ToString();
            newArchiveInfo.id = GetArchiveID();
            newArchiveInfo.name = archiveInfo.name;
            newArchiveInfo.playTime = CurrentArchivePlayTime;
            newArchiveInfo.currentSceneName = GetData<PlayerMapData>().GetNowPlace();
            SaveArchiveInfo(newArchiveInfo);

            // 创建新文件系统
            InitArchive(newArchiveInfo.GetCreateTimeStr());

            // 写入存档
            SaveGameData();

            archiveDirectoryInfoList.Add(new ArchiveDirectoryInfo()
            {
                path = currentArchiveFolder,
                info = newArchiveInfo
            });

            // 使用新存档信息
            archiveInfo = newArchiveInfo;
        }

        /// <summary>
        /// 旧存继续保存
        /// </summary>
        public void SaveOldArchive()
        {
            archiveInfo.currentSceneName = GetData<PlayerMapData>().GetNowPlace();
            archiveInfo.playTime = CurrentArchivePlayTime;
            SaveArchiveInfo(archiveInfo);
            SaveGameData();
        }

        /// <summary>
        /// 覆盖存档
        /// </summary>
        /// <param name="coveredGuid">覆盖的目标存档guid</param>
        public void CoverArchive(string coveredGuid)
        {
            var covered = archiveDirectoryInfoList.Find(a => a.info.guid == coveredGuid);
            if (covered != null)
            {
                ArchiveInfo newArchiveInfo = new ArchiveInfo();
                newArchiveInfo.createTime = DateTime.Now;
                newArchiveInfo.guid = Guid.NewGuid().ToString();
                newArchiveInfo.id = covered.info.id; // 继承id
                newArchiveInfo.name = archiveInfo.name;
                newArchiveInfo.playTime = CurrentArchivePlayTime;
                newArchiveInfo.currentSceneName = GetData<PlayerMapData>().GetNowPlace();

                // 删除文件夹
                DeleteArchive(covered.Guid);

                // 新保存
                SaveArchiveInfo(newArchiveInfo);

                // 创建新文件系统
                InitArchive(newArchiveInfo.GetCreateTimeStr());

                // 写入存档
                SaveGameData();
                archiveDirectoryInfoList.Add(new ArchiveDirectoryInfo()
                {
                    path = currentArchiveFolder,
                    info = newArchiveInfo
                });

                // 使用新存档信息
                archiveInfo = newArchiveInfo;
                CurrentArchivePlayTime = archiveInfo.playTime;
            }
        }

        public void DeleteArchive(string guid)
        {
            var deleteArchive = archiveDirectoryInfoList.Find(a => a.info.guid == guid);
            if (deleteArchive != null)
            {
                archiveDirectoryInfoList.Remove(deleteArchive);
                Directory.Delete(deleteArchive.path, true);
            }
        }

        public T GetData<T>() where T : BaseArchiveData, new()
        {
            if (archiveDataDic.ContainsKey(typeof(T)))
            {
                return archiveDataDic[typeof(T)] as T;
            }

            var data = BaseArchiveData.Parse<T>(masterGameData);
            archiveDataDic.Add(typeof(T), data);
            return data;
        }

        /// <summary>
        /// 初始化指定名称的存档，每个存档使用一个独立的文件夹和对应的虚拟文件系统
        /// </summary>
        /// <param name="archiveName">存档名称，作为文件夹名称</param>
        private void InitArchive(string archiveName)
        {
            // 构造当前存档的文件夹路径
            currentArchiveFolder = Path.Combine(Application.persistentDataPath, "Save", archiveName);
            if (!Directory.Exists(currentArchiveFolder))
            {
                Directory.CreateDirectory(currentArchiveFolder);
            }

            // 构造虚拟文件系统的完整路径，该文件将存放在当前存档文件夹下
            string fileSystemFullPath = Path.Combine(currentArchiveFolder, FileSystemName);

            // 关闭旧文件系统
            if (saveFileSystem != null)
            {
                fileSystemComponent.DestroyFileSystem(saveFileSystem, false);
            }

            // 如果存在对应的虚拟文件系统，则加载，否则创建新的
            if (File.Exists(fileSystemFullPath))
            {
                saveFileSystem = fileSystemComponent.LoadFileSystem(fileSystemFullPath, FileSystemAccess.ReadWrite);
                return;
            }

            // 根据需求设定容量
            int maxFileCount = 1024;
            int maxBlockCount = 1024;
            saveFileSystem = fileSystemComponent.CreateFileSystem(fileSystemFullPath, FileSystemAccess.ReadWrite, maxFileCount, maxBlockCount);
        }

        /// <summary>
        /// 保存存档数据到虚拟文件系统
        /// </summary>
        private void SaveGameData()
        {
            try
            {
                // 确保数据版本为当前版本
                masterGameData.version = CurrentDataVersion;

                // 背包数据是药水数量的唯一来源，保存前强制写回一次，避免因缓存时机导致漏存。
                var bagData = GetData<PlayerBagData>();
                bagData.SerializeInternal(masterGameData);

                // 序列化所有缓存数据
                foreach (var cache in archiveDataDic.Values)
                {
                    cache.SerializeInternal(masterGameData);
                }

                // 使用 Easy Save 3 序列化为二进制数据
                byte[] dataBytes = ES3.Serialize(masterGameData);

                // 写入到虚拟文件系统中
                bool result = saveFileSystem.WriteFile(SaveFileName, dataBytes);
                if (result)
                {
                    Debug.Log("存档成功！");
                }
                else
                {
                    Debug.LogError("存档失败！");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("保存游戏时出错：" + ex);
            }
        }

        // 保存某个指定的数据类型到存档中
        public void SaveSpcData<T>()
        {
            if (!archiveDataDic.ContainsKey(typeof(T)))
            {
                Debug.Log("=============指定类型的数据不存在"+ typeof(T));
                return;
            }
            var cache = archiveDataDic[typeof(T)];
            // 确保数据版本为当前版本
            masterGameData.version = CurrentDataVersion;
            cache.SerializeInternal(masterGameData);
            // 使用 Easy Save 3 序列化为二进制数据
            byte[] dataBytes = ES3.Serialize(masterGameData);
            // 写入到虚拟文件系统中
            if (saveFileSystem == null) {
                Debug.Log("===============当前还没有存档, 不能保存");
                return; 
            }
            bool result = saveFileSystem.WriteFile(SaveFileName, dataBytes);
            if (result)
            {
                Debug.Log("保存指定类型数据成功！类型：" + typeof(T));
            }
            else
            {
                Debug.LogError("保存指定类型失败！类型：" + typeof(T));
            }
        }

        /// <summary>
        /// 从虚拟文件系统中加载存档数据，并自动升级旧数据
        /// </summary>
        private MasterGameData LoadGameData()
        {
            try
            {
                if (!saveFileSystem.HasFile(SaveFileName))
                {
                    Debug.LogWarning("虚拟文件系统中存档不存在！");
                    return null;
                }

                byte[] bytes = saveFileSystem.ReadFile(SaveFileName);
                MasterGameData data = ES3.Deserialize<MasterGameData>(bytes);

                if (data.version < CurrentDataVersion)
                {
                    Debug.Log($"存档数据版本 {data.version} 低于当前版本 {CurrentDataVersion}，开始数据迁移...");
                    data = MigrateData(data);
                }

                return data;
            }
            catch (Exception ex)
            {
                Debug.LogError("加载游戏时出错：" + ex);
                return null;
            }
        }

        private void SaveArchiveInfo(ArchiveInfo archiveInfo)
        {
            var path = Path.Combine(Application.persistentDataPath, "Save", archiveInfo.GetCreateTimeStr(), ArchiveInfoFileName);

            // 执行序列化
            archiveInfo.SerializeInternal();

            // 持久化
            var bytes = ES3.Serialize(archiveInfo);

            if (!Directory.Exists(Path.GetDirectoryName(path)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }

            ES3.SaveRaw(bytes, path);

            // 重新反序列化数据
            archiveInfo.ParseInternal();
        }

        private ArchiveInfo LoadArchiveInfo(string path)
        {
            var bytes = ES3.LoadRawBytes(path);
            ArchiveInfo data = ES3.Deserialize<ArchiveInfo>(bytes);
            data.ParseInternal();
            return data;
        }

        public ArchiveInfo GetNowArchiveInfo() => archiveInfo;

        public List<ArchiveDirectoryInfo> LoadAllArchiveInfo()
        {
            if (archiveDirectoryInfoList.Count == 0)
            {
                var path = Path.Combine(Application.persistentDataPath, "Save");
                var directories = Directory.GetDirectories(path);
                foreach (var directory in directories)
                {
                    var files = Directory.GetFiles(directory);
                    foreach (var file in files)
                    {
                        if (file.EndsWith("ArchiveInfo.dat"))
                        {
                            var info = LoadArchiveInfo(file);
                            archiveDirectoryInfoList.Add(new ArchiveDirectoryInfo()
                            {
                                path = directory,
                                info = info
                            });
                        }
                    }
                }
            }

            archiveDirectoryInfoList.Sort((a, b) => a.info.id.CompareTo(b.info.id));
            return archiveDirectoryInfoList;
        }

        /// <summary>
        /// 数据迁移：将旧版本数据升级到当前版本
        /// </summary>
        private MasterGameData MigrateData(MasterGameData oldData)
        {
            if (oldData.version < 2)
            {
                // 示例升级逻辑
                oldData.version = 2;
            }

            // 如果未来有更多版本，则继续添加升级逻辑
            oldData.version = CurrentDataVersion;
            Debug.Log("数据迁移完成，当前版本：" + oldData.version);
            return oldData;
        }

        private int GetArchiveID()
        {
            // 如果列表有数据，取出最大 id；否则设为 0，然后新 id 为 max + 1
            return archiveDirectoryInfoList.Count > 0 ? archiveDirectoryInfoList.Max(x => x.info.id) + 1 : 1;
        }
    }
}