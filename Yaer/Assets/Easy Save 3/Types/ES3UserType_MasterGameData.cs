using System;
using Game.GameMgr.Component.Archive.ArchiveDataClass.BaseDataClass;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("version", "data")]
	public class ES3UserType_MasterGameData : ES3ObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_MasterGameData() : base(typeof(MasterGameData)){ Instance = this; priority = 1; }


		protected override void WriteObject(object obj, ES3Writer writer)
		{
			var instance = (MasterGameData)obj;
			
			writer.WriteProperty("version", instance.version, ES3Type_int.Instance);
			writer.WriteProperty("data", instance.data, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(System.Collections.Generic.Dictionary<System.String, System.Object>)));
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			var instance = (MasterGameData)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "version":
						instance.version = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "data":
						instance.data = reader.Read<System.Collections.Generic.Dictionary<System.String, System.Object>>();
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(ES3Reader reader)
		{
			var instance = new MasterGameData();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}


	public class ES3UserType_MasterGameDataArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_MasterGameDataArray() : base(typeof(MasterGameData[]), ES3UserType_MasterGameData.Instance)
		{
			Instance = this;
		}
	}
}