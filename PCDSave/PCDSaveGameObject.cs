using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ES3Types;
using ES3Internal;


namespace PCD.SaveSystem
{
    public class PCDSaveGameObject : ES3GameObject
    {
        public bool saveForDestory = true;
        public ComponentSaveDictionary compSaveDict = new ComponentSaveDictionary();

        public bool GetDefaultSaveState(Component component) {
            if (component is Transform || component is RectTransform)
                return true;
            ES3Type es3Type = ES3TypeMgr.GetES3Type(component.GetType());
            return es3Type is IPCDAutoSave;
        }

        private void Awake() {
            if (Application.isPlaying)
                PCDSaveMgr.AddPCDSaveGameObject(this);
        }
        private bool isQuitting = false;
        public void OnApplicationQuit() {
            isQuitting = true;
        }
        public void OnDestroy() {
            // If this is being destroyed, but not because the application is quitting,
            // remove the AutoSave from the manager.
            if (!isQuitting && Application.isPlaying)
                PCDSaveMgr.RemovePCDSaveGameObject(this);
        }

        static public bool SafetyCheckDerivedType(System.Type type) {
#if UNITY_EDITOR
            if (ES3TypeMgr.GetES3Type(type) != null)
                return true;
            var curType = type;
            while (curType != typeof(MonoBehaviour) && curType != null) {
                ES3Type es3Type = ES3TypeMgr.GetES3Type(curType);
                if (es3Type is IPCDAutoSave) {
                    Debug.LogError(string.Format("ES3Type of {0} must be explicitly created for derived type of {1}!", type.ToString(), curType.ToString()));
                    return false;
                }
                curType = curType.BaseType;
			}
#endif
            return true;
        }
    }


    [CustomEditor(typeof(PCDSaveGameObject))]
    public class PCDSaveGameObjectEditor : Editor
    {
        public override void OnInspectorGUI() {
            if (target == null)
                return;

            var es3Go = (PCDSaveGameObject)target;

            EditorGUILayout.HelpBox("This Component allows you to choose which Components are saved when this GameObject is saved using code.", MessageType.Info);

            if (es3Go.GetComponent<ES3AutoSave>() != null) {
                EditorGUILayout.HelpBox("This Component cannot be used on GameObjects which are already managed by Auto Save.", MessageType.Error);
                return;
            }

            var newSaveForDestory = EditorGUILayout.Toggle("SaveForDestory?", es3Go.saveForDestory);
            if (newSaveForDestory != es3Go.saveForDestory) {
                Undo.RecordObject(es3Go, "Change SaveForDestory");
                es3Go.saveForDestory = newSaveForDestory;
            }

            foreach (var component in es3Go.GetComponents<Component>()) {
                if (component is PCDSaveGameObject || component is ES3Prefab)
                    continue;

                bool markedToBeSaved = false;
                bool foundInDict = es3Go.compSaveDict.TryGetValue(component, out markedToBeSaved);
                if (foundInDict) {
                    /* Consistence check. */
                    var markedInLists = es3Go.components.Contains(component);
                    if (markedInLists != markedToBeSaved)
                        Debug.LogError("Incorrect State!");
                } else {
                    markedToBeSaved = es3Go.GetDefaultSaveState(component);
                }

                var newMarkedToBeSaved = EditorGUILayout.Toggle(component.GetType().Name, markedToBeSaved);
                newMarkedToBeSaved = newMarkedToBeSaved && PCDSaveGameObject.SafetyCheckDerivedType(component.GetType());
                if (!foundInDict) {
                    if (newMarkedToBeSaved) {
                        Undo.RecordObject(es3Go, "Unmarked Component to save");
                        es3Go.compSaveDict[component] = true;
                        es3Go.components.Add(component);
                    } else {
                        Undo.RecordObject(es3Go, "Unmarked Component to save");
                        es3Go.compSaveDict[component] = false;
                    }
                } else {
                    if (markedToBeSaved && !newMarkedToBeSaved) {
                        Undo.RecordObject(es3Go, "Marked Component to save");
                        es3Go.compSaveDict[component] = false;
                        es3Go.components.Remove(component);
                    }

                    if (!markedToBeSaved && newMarkedToBeSaved) {
                        Undo.RecordObject(es3Go, "Unmarked Component to save");
                        es3Go.compSaveDict[component] = true;
                        es3Go.components.Add(component);
                    }
                }
            }
        }
    }


    [System.Serializable]
    public class ComponentSaveDictionary : ES3SerializableDictionary<Component, bool>
    {
        protected override bool KeysAreEqual(Component a, Component b) {
            return a == b;
        }

        protected override bool ValuesAreEqual(bool a, bool b) {
            return a == b;
        }
    }
}