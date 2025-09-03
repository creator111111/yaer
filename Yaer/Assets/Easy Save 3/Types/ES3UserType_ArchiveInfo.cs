using System;
using Game.GameMgr.Component.Archive.ArchiveDataClass;
using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("data", "guid", "name", "createTime", "playTime")]
	public class ES3UserType_ArchiveInfo : ES3ObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_ArchiveInfo() : base(typeof(ArchiveInfo)){ Instance = this; priority = 1; }


		protected override void WriteObject(object obj, ES3Writer writer)
		{
			var instance = (ArchiveInfo)obj;
			
			writer.WriteProperty("data", instance.data, ES3UserType_MasterGameData.Instance);
			writer.WriteProperty("guid", instance.guid, ES3Type_string.Instance);
			writer.WriteProperty("name", instance.name, ES3Type_string.Instance);
			writer.WriteProperty("createTime", instance.createTime, ES3Type_DateTime.Instance);
			writer.WriteProperty("playTime", instance.playTime, ES3Type_float.Instance);
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			var instance = (ArchiveInfo)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "data":
						instance.data = reader.Read<MasterGameData>(ES3UserType_MasterGameData.Instance);
						break;
					case "guid":
						instance.guid = reader.Read<System.String>(ES3Type_string.Instance);
						break;
					case "name":
						instance.name = reader.Read<System.String>(ES3Type_string.Instance);
						break;
					case "createTime":
						instance.createTime = reader.Read<System.DateTime>(ES3Type_DateTime.Instance);
						break;
					case "playTime":
						instance.playTime = reader.Read<System.Single>(ES3Type_float.Instance);
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(ES3Reader reader)
		{
			var instance = new ArchiveInfo();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}


	public class ES3UserType_ArchiveInfoArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_ArchiveInfoArray() : base(typeof(ArchiveInfo[]), ES3UserType_ArchiveInfo.Instance)
		{
			Instance = this;
		}
	}
}