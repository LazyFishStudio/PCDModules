using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ES3Internal;
using PCD.SaveSystem;


namespace PCD.SaveSystem
{
	public static class PCDSaveHelper
	{
		static public void WriteSimpleGameObjects(string key, GameObject[] gameObjects, ES3Settings settings) {
			using (var writer = ES3Writer.Create(settings)) {
				writer.StartWriteProperty(key);
				writer.StartWriteObject(key);
				writer.WriteType(typeof(GameObject[]));
				writer.WriteProperty("value", gameObjects, ES3Types.ES3Type_PCDSimpleGOArray.Instance, ES3.ReferenceMode.ByRefAndValue);
				writer.EndWriteObject(key);
				writer.EndWriteProperty(key);
				writer.MarkKeyForDeletion(key);

				writer.Save();
			}
		}

		static public void WriteActualGameObjects(string key, GameObject[] gameObjects, ES3Settings settings) {
			using (var writer = ES3Writer.Create(settings)) {
				writer.StartWriteProperty(key);
				writer.StartWriteObject(key);
				writer.WriteType(typeof(GameObject[]));
				writer.WriteProperty("value", gameObjects, ES3Types.ES3Type_PCDActualGOArray.Instance, ES3.ReferenceMode.ByRefAndValue);
				writer.EndWriteObject(key);
				writer.EndWriteProperty(key);
				writer.MarkKeyForDeletion(key);

				writer.Save();
			}
		}
	}
}


#region PCDSimpleActualGO
/* Mock class to save simple and actual GameObject */
public class PCDSimpleGO : UnityEngine.Object { }
public class PCDActualGO : UnityEngine.Object { }

/* Override save method for PCDSimpleGO */
namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("layer", "isStatic", "tag", "name", "hideFlags", "children", "components")]
	public class ES3Type_PCDSimpleGO : ES3UnityObjectType
	{
		private const string prefabPropertyName = "es3Prefab";
		private const string transformPropertyName = "transformID";

		public static ES3Type Instance = null;
		public ES3Type_PCDSimpleGO() : base(typeof(PCDSimpleGO)) { Instance = this; }

		public override void WriteObject(object obj, ES3Writer writer, ES3.ReferenceMode mode) {
			var instance = (UnityEngine.GameObject)obj;

			if (mode != ES3.ReferenceMode.ByValue) {
				writer.WriteRef(instance);

				if (mode == ES3.ReferenceMode.ByRef)
					return;

				var es3Prefab = instance.GetComponent<ES3Prefab>();
				if (es3Prefab != null)
					writer.WriteProperty(prefabPropertyName, es3Prefab, ES3Type_ES3PrefabInternal.Instance);

				// Write the ID of this Transform so we can assign it's ID when we load.
				writer.WriteProperty(transformPropertyName, ES3ReferenceMgrBase.Current.Add(instance.transform));
			}

			writer.WriteProperty("layer", instance.layer, ES3Type_int.Instance);
			writer.WriteProperty("tag", instance.tag, ES3Type_string.Instance);
			writer.WriteProperty("name", instance.name, ES3Type_string.Instance);
			writer.WriteProperty("hideFlags", instance.hideFlags);
			writer.WriteProperty("active", instance.activeSelf);
			// writer.WriteProperty("children", GetChildren(instance), ES3.ReferenceMode.ByRefAndValue);

			/* Save ByRefAndValue components: registed and Transform / RectTransform */
			List<Component> saveObjs = instance.GetComponent<PCDSaveGameObject>().components.ToList();
			saveObjs = saveObjs.Where((comp) => (comp != null)).ToList();
			List<Component> saveByRefAndValue = saveObjs.Where((comp) => (comp is Transform || comp is RectTransform)).ToList();
			List<Component> saveByRef = saveObjs.Except(saveByRefAndValue).ToList();
			if (saveByRefAndValue.Count > 0) {
				writer.WriteProperty("components", saveByRefAndValue.ToArray(), ES3.ReferenceMode.ByRefAndValue);
			}
			if (saveByRef.Count > 0) {
				writer.WriteProperty("components", saveByRef.ToArray(), ES3.ReferenceMode.ByRef);
			}
		}

		// These are not used as we've overridden the ReadObject methods instead.
		protected override void WriteUnityObject(object obj, ES3Writer writer) { }
		protected override void ReadUnityObject<T>(ES3Reader reader, object obj) { }
		protected override object ReadUnityObject<T>(ES3Reader reader) { return null; }
	}


	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("layer", "isStatic", "tag", "name", "hideFlags", "children", "components")]
	public class ES3Type_PCDActualGO : ES3UnityObjectType
	{
		private const string prefabPropertyName = "es3Prefab";
		private const string transformPropertyName = "transformID";

		public bool saveChildren;
		public static ES3Type Instance = null;
		public ES3Type_PCDActualGO() : base(typeof(PCDActualGO)) { Instance = this; }

		public override void WriteObject(object obj, ES3Writer writer, ES3.ReferenceMode mode) {
			var instance = (UnityEngine.GameObject)obj;

			if (mode != ES3.ReferenceMode.ByValue) {
				writer.WriteRef(instance);

				if (mode == ES3.ReferenceMode.ByRef)
					return;

				var es3Prefab = instance.GetComponent<ES3Prefab>();
				if (es3Prefab != null)
					writer.WriteProperty(prefabPropertyName, es3Prefab, ES3Type_ES3PrefabInternal.Instance);

				// Write the ID of this Transform so we can assign it's ID when we load.
				writer.WriteProperty(transformPropertyName, ES3ReferenceMgrBase.Current.Add(instance.transform));
			}

			writer.WriteProperty("layer", instance.layer, ES3Type_int.Instance);
			writer.WriteProperty("tag", instance.tag, ES3Type_string.Instance);
			writer.WriteProperty("name", instance.name, ES3Type_string.Instance);
			writer.WriteProperty("hideFlags", instance.hideFlags);
			writer.WriteProperty("active", instance.activeSelf);

			PCDSaveGameObject saveGO = instance.GetComponent<PCDSaveGameObject>();
			// writer.WriteProperty("children", GetChildren(instance), ES3.ReferenceMode.ByRefAndValue);

			/* Save ByRefAndValue components: registed and Transform / RectTransform */
			List<Component> saveObjs = saveGO.components.Where((comp) => (comp != null)).ToList();
			if (saveObjs.Count > 0) {
				writer.WriteProperty("components", saveObjs.ToArray(), ES3.ReferenceMode.ByRefAndValue);
			}
		}

		// These are not used as we've overridden the ReadObject methods instead.
		protected override void WriteUnityObject(object obj, ES3Writer writer) { }
		protected override void ReadUnityObject<T>(ES3Reader reader, object obj) { }
		protected override object ReadUnityObject<T>(ES3Reader reader) { return null; }
	}

	public class ES3Type_PCDSimpleGOArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3Type_PCDSimpleGOArray() : base(typeof(PCDSimpleGO[]), ES3Type_PCDSimpleGO.Instance) {
			Instance = this;
		}
	}

	public class ES3Type_PCDActualGOArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3Type_PCDActualGOArray() : base(typeof(PCDActualGO[]), ES3Type_PCDActualGO.Instance) {
			Instance = this;
		}
	}
}
#endregion
