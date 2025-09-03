using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using GameFramework.CoreExtend.Serialiizer.Binary;
using UnityEngine;

namespace DebugScene
{
    public class TestArchiveUpdate : MonoBehaviour
    {
        private readonly string path = Application.streamingAssetsPath + "/Test/";

        // Start is called before the first frame update
        private void Start()
        {
            // TestData1 data = new TestData1();
            // data.name = "1";
            // data.age = 1;
            // data.height = 1.1f;
            // data.weight = 1.1f;
            // data.isMan = true;
            // Serialize(data);
            Deserialize();
        }


        private void Serialize(BaseTestData data)
        {
            BinarySystem.Instance.Save(path + "test.zy", data);
        }

        private BaseTestData Deserialize()
        {
            var nowData = new TestData();

            object archive;

            using (var fileStream = File.Open(path + "test.zy", FileMode.Open, FileAccess.Read))
            {
                var binaryFormatter = new BinaryFormatter(); // 序列化工具
                archive = binaryFormatter.Deserialize(fileStream);
                fileStream.Close();
            }

            var fieldInfo = archive.GetType().GetField("version");
            var version = (int)fieldInfo.GetValue(archive);

            // 
            if (nowData.version != version) return UpdateArchive<TestData>(archive);

            return archive as BaseTestData;
        }

        private T UpdateArchive<T>(object oldData) where T : class
        {
            var newData = new TestData();

            var fieldInfos = oldData.GetType().GetFields();
            foreach (var fieldInfo in fieldInfos)
            {
                var newFieldInfo = newData.GetType().GetField(fieldInfo.Name);

                if (newFieldInfo != null)
                {
                    if (newFieldInfo.Name == "version") continue;
                    newFieldInfo.SetValue(newData, fieldInfo.GetValue(oldData));
                }
            }

            // Serialize(newData);
            return newData as T;
        }
    }


    [Serializable]
    public class BaseTestData
    {
        public int version;
    }

    [Serializable]
    public class TestData1 : BaseTestData
    {
        public new int version = 1;
        public string name;
        public int age;
        public float height;
        public float weight;
        public bool isMan;
        public List<int> list;
        public Dictionary<int, string> dic;
    }

    [Serializable]
    public class TestData2 : BaseTestData
    {
        public new int version = 2;
        public string name = "2";
        public int age;
        public float weight;
        public List<int> list;
        public int id2;
        public Dictionary<int, string> dic;
    }


    [Serializable]
    public class TestData : BaseTestData
    {
        public new int version = 3;
        public string name = "3";
        public int id3 = 3;
        public float abc;
    }
}