using Game.GameMgr;
using Game.GameMgr.Component.Archive;
using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using System;
using System.Reflection;

namespace Game.GameRuntime.Story.Node
{
    [Category("Archive")]
    public abstract class SetArchiveFieldActionTask<ArchiveDataType, TargetDataType> : ActionTask where ArchiveDataType : BaseArchiveData
    {
        public BBParameter<TargetDataType> TargetValue;

        public BBParameter<string> FieldName;

        protected override void OnExecute()
        {
            base.OnExecute();
            var archiveMgr = GameManager.GetGMComponent<ArchiveComponentGM>();
            Type archiveMgrType = archiveMgr.GetType();
            MethodInfo GetDataMethod = archiveMgrType.GetMethod("GetData");
            var archiveData = GetDataMethod.MakeGenericMethod(typeof(ArchiveDataType)).Invoke(archiveMgr, null) as ArchiveDataType;

            FieldInfo fieldInfo = typeof(ArchiveDataType).GetField(FieldName.value);
            fieldInfo.SetValue(archiveData, TargetValue.value);
            EndAction();
        }

        protected override string info => string.Format("<i>' ÉèÖÃ´æµµÊý¾Ý: {0}.{1} -> {2}'</i>", typeof(ArchiveDataType).Name, FieldName, TargetValue);
    }
}