using System;

namespace Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass
{
    /// <summary>
    ///     存档数据
    /// </summary>
    [Serializable]
    public abstract class BaseArchiveData: IArchiveData
    {
        /// <summary>
        /// 子类实现此方法，从 MasterGameData 中解析自身数据。
        /// </summary>
        /// <param name="masterData">主存档数据</param>
        public abstract void ParseInternal(MasterGameData masterData);

        /// <summary>
        ///  子类实现此方法，将自身数据序列化到 MasterGameData 中。
        /// </summary>
        /// <param name="masterData"></param>
        public abstract void SerializeInternal(MasterGameData masterData);

        /// <summary>
        /// 静态泛型方法，创建 T 实例并调用解析。
        /// </summary>
        public static T Parse<T>(MasterGameData masterData) where T : BaseArchiveData, new()
        {
            T data = new T();
            data.ParseInternal(masterData);
            return data;
        }
    }
}