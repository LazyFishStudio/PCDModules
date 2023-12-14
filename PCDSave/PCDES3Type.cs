using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	public abstract class PCDES3Type : ES3ComponentType
	{
		public static ES3Type Instance = null;

		public PCDES3Type(System.Type type) : base(type) {
			Instance = this;
		}
		protected override void WriteComponent(object obj, ES3Writer writer) {
			WriteComponentImpl(obj, writer);
		}
		protected override void ReadComponent<T>(ES3Reader reader, object obj) {
			ReadComponentImpl(reader, obj);
		}

		public abstract void WriteComponentImpl(object obj, ES3Writer writer);
		public abstract void ReadComponentImpl(ES3Reader reader, object obj);
	}

	[UnityEngine.Scripting.Preserve]
	internal class PCDReflectedComponentType : ES3ReflectedComponentType
	{
		public static ES3Type Instance = null;

		public PCDReflectedComponentType(System.Type type) : base(type) {
			Instance = this;
		}
	}
}
