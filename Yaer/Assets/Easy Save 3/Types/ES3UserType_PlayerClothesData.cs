using System;
using Game.GameMgr.Component.Archive.ArchiveDataClass.Player;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("clothesDataDic")]
	public class ES3UserType_PlayerClothesData : ES3ObjectType
	{
		public static ES3Type Instance = null;

		public ES3UserType_PlayerClothesData() : base(typeof(PlayerClothesData)){ Instance = this; priority = 1; }


		protected override void WriteObject(object obj, ES3Writer writer)
		{
			var instance = (PlayerClothesData)obj;
			
			writer.WritePrivateField("clothesDataDic", instance);
		}

		protected override void ReadObject<T>(ES3Reader reader, object obj)
		{
			var instance = (PlayerClothesData)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "clothesDataDic":
					instance = (PlayerClothesData)reader.SetPrivateField("clothesDataDic", reader.Read<System.Collections.Generic.Dictionary<System.String, System.String>>(), instance);
					break;
					default:
						reader.Skip();
						break;
				}
			}
		}

		protected override object ReadObject<T>(ES3Reader reader)
		{
			var instance = new PlayerClothesData();
			ReadObject<T>(reader, instance);
			return instance;
		}
	}


	public class ES3UserType_PlayerClothesDataArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_PlayerClothesDataArray() : base(typeof(PlayerClothesData[]), ES3UserType_PlayerClothesData.Instance)
		{
			Instance = this;
		}
	}
}