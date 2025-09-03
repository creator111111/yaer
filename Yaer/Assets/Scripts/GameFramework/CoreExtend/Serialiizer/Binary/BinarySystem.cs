using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using GameFramework.CoreExtend.Generic;
using UnityEngine;

namespace GameFramework.CoreExtend.Serialiizer.Binary
{
    public class BinarySystem : BaseSingleton<BinarySystem>
    {
        public static string EXCELFILE_PATH = Application.dataPath + "/GameInfo/Excel/";
        public static string INFOCLASS_PATH = Application.dataPath + "/Scripts/Data/";
        public static string BINARYFILE_PATH = Application.streamingAssetsPath + "/";

        // 存放读取出来的Excel二进制文件数据
        public Dictionary<string, object> tableDic = new Dictionary<string, object>();

        /// <summary>
        ///     读取Excel的二进制文件
        /// </summary>
        /// <typeparam name="T">数据信息类</typeparam>
        /// <typeparam name="K">信息容器类</typeparam>
        public K LoadExcelBinary<T, K>() where K : class
        {
            // 判断是否已经读取过
            if (tableDic.ContainsKey(typeof(T).Name)) return tableDic[typeof(T).Name] as K;

            // 判断文件是否存在
            if (!File.Exists(BINARYFILE_PATH + typeof(T).Name + ".zy")) return default;

            using (var fileStream = File.Open(BINARYFILE_PATH + typeof(T).Name + ".zy", FileMode.Open, FileAccess.Read))
            {
                // 定义文件字节大小的字节数组
                var fileBuffer = new byte[fileStream.Length];

                // 读取真实数据的行数
                var index = 0;
                fileStream.Read(fileBuffer, index, sizeof(int));
                var count = BitConverter.ToInt32(fileBuffer, 0);
                index += sizeof(int);

                // 读取主键字符串
                // 长度
                fileStream.Read(fileBuffer, index, sizeof(int));
                var primaryKeyNameLength = BitConverter.ToInt32(fileBuffer, index);
                index += sizeof(int);
                // 字符串
                fileStream.Read(fileBuffer, index, primaryKeyNameLength);
                var primaryKeyName = Encoding.UTF8.GetString(fileBuffer, index, primaryKeyNameLength);
                index += primaryKeyNameLength; // 位移字符串长度

                var type = typeof(T);
                var fieldInfos = type.GetFields(); // 获取所有字段

                // 创建容器
                var newContainer = Activator.CreateInstance<K>();

                // 读取每一行数据
                for (var i = 0; i < count; i++)
                {
                    // 实例化新的信息类
                    var newInfoClass = Activator.CreateInstance<T>();

                    // 读取每一列
                    for (var j = 0; j < fieldInfos.Length; j++)
                    {
                        // 读取int字段
                        if (fieldInfos[j].FieldType == typeof(int))
                        {
                            fileStream.Read(fileBuffer, index, sizeof(int));
                            fieldInfos[j].SetValue(newInfoClass, BitConverter.ToInt32(fileBuffer, index));
                            index += sizeof(int);
                        }

                        if (fieldInfos[j].FieldType == typeof(float))
                        {
                            fileStream.Read(fileBuffer, index, sizeof(float));
                            fieldInfos[j].SetValue(newInfoClass, BitConverter.ToSingle(fileBuffer, index));
                            index += sizeof(float);
                        }

                        if (fieldInfos[j].FieldType == typeof(bool))
                        {
                            fileStream.Read(fileBuffer, index, sizeof(bool));
                            fieldInfos[j].SetValue(newInfoClass, BitConverter.ToBoolean(fileBuffer, index));
                            index += sizeof(bool);
                        }

                        // 读取string字段
                        if (fieldInfos[j].FieldType == typeof(string))
                        {
                            // 先获取字符串长度
                            fileStream.Read(fileBuffer, index, sizeof(int));
                            var strLength = BitConverter.ToInt32(fileBuffer, index);
                            index += sizeof(int);

                            fileStream.Read(fileBuffer, index, strLength);
                            var str = Encoding.UTF8.GetString(fileBuffer, index, strLength);
                            fieldInfos[j].SetValue(newInfoClass, str); // 设置字段值
                            index += strLength;
                        }
                    }

                    // 添加进容器
                    // 获取
                    var dic = newContainer.GetType().GetFields()[0];
                    var methodInfo = dic.FieldType.GetMethod("Add");
                    methodInfo?.Invoke(dic.GetValue(newContainer),
                        new[] { newInfoClass.GetType().GetField(primaryKeyName).GetValue(newInfoClass), newInfoClass });
                }

                // 存入tableDic
                tableDic.Add(typeof(T).Name, newContainer);

                fileStream.Close();
            }

            return tableDic[typeof(T).Name] as K;
        }

        /// <summary>
        ///     非泛型读取Excel二进制数据
        /// </summary>
        /// <param name="infoType">数据信息类Type</param>
        /// <param name="containerType">信息容器类Type</param>
        /// <returns></returns>
        public object LoadExcelBinary(Type infoType, Type containerType)
        {
            if (tableDic.ContainsKey(infoType.Name)) return tableDic[infoType.Name];

            if (!File.Exists(BINARYFILE_PATH + infoType.Name + ".zy")) return null;

            using (var fileStream = File.Open(BINARYFILE_PATH + infoType.Name + ".zy", FileMode.Open, FileAccess.Read))
            {
                // 定义文件字节大小的字节数组
                var fileBuffer = new byte[fileStream.Length];

                // 读取真实数据的行数
                var index = 0;
                fileStream.Read(fileBuffer, index, sizeof(int));
                var count = BitConverter.ToInt32(fileBuffer, 0);
                index += sizeof(int);

                // 读取主键字符串
                // 长度
                fileStream.Read(fileBuffer, index, sizeof(int));
                var primaryKeyNameLength = BitConverter.ToInt32(fileBuffer, index);
                index += sizeof(int);
                // 字符串
                fileStream.Read(fileBuffer, index, primaryKeyNameLength);
                var primaryKeyName = Encoding.UTF8.GetString(fileBuffer, index, primaryKeyNameLength);
                index += primaryKeyNameLength; // 位移字符串长度

                var fieldInfos = infoType.GetFields(); // 获取所有字段

                // 创建容器
                var newContainer = Activator.CreateInstance(containerType);

                // 读取每一行数据
                for (var i = 0; i < count; i++)
                {
                    // 实例化新的信息类
                    var newInfoClass = Activator.CreateInstance(infoType);

                    // 读取每一列
                    for (var j = 0; j < fieldInfos.Length; j++)
                    {
                        // 读取int字段
                        if (fieldInfos[j].FieldType == typeof(int))
                        {
                            fileStream.Read(fileBuffer, index, sizeof(int));
                            fieldInfos[j].SetValue(newInfoClass, BitConverter.ToInt32(fileBuffer, index));
                            index += sizeof(int);
                        }

                        if (fieldInfos[j].FieldType == typeof(float))
                        {
                            fileStream.Read(fileBuffer, index, sizeof(float));
                            fieldInfos[j].SetValue(newInfoClass, BitConverter.ToSingle(fileBuffer, index));
                            index += sizeof(float);
                        }

                        if (fieldInfos[j].FieldType == typeof(bool))
                        {
                            fileStream.Read(fileBuffer, index, sizeof(bool));
                            fieldInfos[j].SetValue(newInfoClass, BitConverter.ToBoolean(fileBuffer, index));
                            index += sizeof(bool);
                        }

                        // 读取string字段
                        if (fieldInfos[j].FieldType == typeof(string))
                        {
                            // 先获取字符串长度
                            fileStream.Read(fileBuffer, index, sizeof(int));
                            var strLength = BitConverter.ToInt32(fileBuffer, index);
                            index += sizeof(int);

                            fileStream.Read(fileBuffer, index, strLength);
                            var str = Encoding.UTF8.GetString(fileBuffer, index, strLength);
                            fieldInfos[j].SetValue(newInfoClass, str); // 设置字段值
                            index += strLength;
                        }
                    }

                    // 添加进容器
                    // 获取
                    var dic = newContainer.GetType().GetFields()[0];
                    var methodInfo = dic.FieldType.GetMethod("Add");
                    methodInfo?.Invoke(dic.GetValue(newContainer),
                        new[] { newInfoClass.GetType().GetField(primaryKeyName).GetValue(newInfoClass), newInfoClass });
                }

                // 存入tableDic
                tableDic.Add(infoType.Name, newContainer);

                fileStream.Close();
            }

            return tableDic[infoType.Name];
        }

        public object LoadRaw(string fullName)
        {
            if (!File.Exists(fullName))
            {
                Debug.Log("not found binary file!");
                return null;
            }

            try
            {
                object data;

                using (var fileStream = File.Open(fullName, FileMode.Open, FileAccess.Read))
                {
                    var binaryFormatter = new BinaryFormatter(); // 序列化工具
                    data = binaryFormatter.Deserialize(fileStream);
                    fileStream.Close();
                }

                return data;
            }
            catch
            {
                Debug.Log("binary file is damage!");
                return null;
            }
        }

        /// <summary>
        ///     读取二进制文件
        /// </summary>
        /// <param name="fileName">全路径文件名</param>
        /// <typeparam name="T">返回的类类型</typeparam>
        /// <returns></returns>
        public T Load<T>(string fullFileName) where T : class, new()
        {
            // 判断有无后缀
            if (!fullFileName.Contains(".") || !File.Exists(fullFileName))
            {
                Debug.Log($"not found binary file! return {nameof(T)} default value!");
                return new T();
            }


            try
            {
                T obj;
                using (var fileStream = File.Open(fullFileName, FileMode.Open, FileAccess.Read))
                {
                    var binaryFormatter = new BinaryFormatter(); // 序列化工具
                    obj = binaryFormatter.Deserialize(fileStream) as T;
                    fileStream.Close();
                }

                return obj;
            }
            catch
            {
                Debug.Log($"binary file is damage! return default value! at  {nameof(T)} ");
                return new T();
            }
        }

        /// <summary>
        ///     非泛型方法加载二进制
        /// </summary>
        /// <param name="type"></param>
        /// <param name="fullFileName"></param>
        /// <returns></returns>
        public object Load(Type type, string fullFileName)
        {
            if (!fullFileName.Contains(".") || !File.Exists(fullFileName))
            {
                Debug.Log($"not found binary file! return default value! at  {type.Name}");
                return Activator.CreateInstance(type);
            }

            object data;
            try
            {
                using (var fileStream = File.Open(fullFileName, FileMode.Open, FileAccess.Read))
                {
                    var binaryFormatter = new BinaryFormatter(); // 序列化工具
                    data = binaryFormatter.Deserialize(fileStream);
                    fileStream.Close();
                }

                return data;
            }
            catch
            {
                Debug.Log($"binary file is damage! return {type.Name} default value!");
                return Activator.CreateInstance(type);
            }
        }

        public void Save(string fullFileName, object data)
        {
            if (!fullFileName.Contains("."))
            {
                Debug.LogWarning("二进制文件保存失败!");
                return;
            }

            // 不存在该类的二进制文件则创建
            if (!File.Exists(fullFileName)) File.Create(fullFileName).Close();

            using (var fileStream = File.Open(fullFileName, FileMode.Open, FileAccess.Write))
            {
                var binaryFormatter = new BinaryFormatter();
                binaryFormatter.Serialize(fileStream, data);
                fileStream.Flush(); // 强制刷新缓存区立刻写入文件
                fileStream.Close();
            }
        }
    }
}