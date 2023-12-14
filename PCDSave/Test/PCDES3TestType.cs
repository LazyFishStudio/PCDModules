using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PCD.SaveSystem.Test;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("saveValue", "loadValue")]
	public class ES3Type_SaveBase : PCDES3Type, IPCDAutoSave
	{
		public ES3Type_SaveBase() : base(typeof(SaveBase)) {
			Instance = this;
		}

		public override void WriteComponentImpl(object obj, ES3Writer writer) {
			var instance = (SaveBase)obj;
			writer.WriteProperty("saveValue", instance.saveValue);
			writer.WriteProperty("loadValue", instance.loadValue);
		}

		public override void ReadComponentImpl(ES3Reader reader, object obj) {
			var instance = (SaveBase)obj;

			foreach (string propertyName in reader.Properties) {
				switch (propertyName) {
					case "saveValue":
						instance.saveValue = reader.Read<int>();
						break;
					case "loadValue":
						instance.loadValue = reader.Read<int>();
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}

	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("saveValue", "loadValue")]
	public class ES3Type_SaveImpl : PCDES3Type, IPCDAutoSave
	{
		public ES3Type_SaveImpl() : base(typeof(SaveImpl)) {
			Instance = this;
		}

		public override void WriteComponentImpl(object obj, ES3Writer writer) {
			(ES3Internal.ES3TypeMgr.GetES3Type(type.BaseType) as ES3Type_SaveBase).WriteComponentImpl(obj, writer);
		}

		public override void ReadComponentImpl(ES3Reader reader, object obj) {
			(ES3Internal.ES3TypeMgr.GetES3Type(type.BaseType) as ES3Type_SaveBase).ReadComponentImpl(reader, obj);
		}
	}

	[UnityEngine.Scripting.Preserve]
	internal class ES3Type_SaveReflect : PCDReflectedComponentType, IPCDAutoSave
	{
		public ES3Type_SaveReflect() : base(typeof(SaveReflect)) { Instance = this; }
	}
}