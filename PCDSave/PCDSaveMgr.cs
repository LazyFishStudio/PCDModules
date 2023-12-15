using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;


/* ES3Type with this interface will be automaticlly saved by PCDSaveMgr */
public interface IPCDAutoSave { }


namespace PCD.SaveSystem
{
    public class PCDSaveMgr : MonoBehaviour
    {
        public static PCDSaveMgr _current = null;
        public static PCDSaveMgr Current {
            get {
                if (_current == null /* || _current.gameObject.scene != SceneManager.GetActiveScene() */) {
                    var scene = SceneManager.GetActiveScene();
                    var roots = scene.GetRootGameObjects();

                    // First, look for Easy Save 3 Manager in the top-level.
                    foreach (var root in roots)
                        if (root.name == "Easy Save 3 Manager")
                            return _current = root.GetComponent<PCDSaveMgr>();

                    // If the user has moved or renamed the Easy Save 3 Manager, we need to perform a deep search.
                    foreach (var root in roots)
                        if ((_current = root.GetComponentInChildren<PCDSaveMgr>()) != null)
                            return _current;
                }
                return _current;
            }
        }

        // Included for backwards compatibility.
        public static PCDSaveMgr Instance {
            get { return Current; }
        }

        public HashSet<PCDSaveGameObject> saveObjects = new HashSet<PCDSaveGameObject>();

        static public void AddPCDSaveGameObject(PCDSaveGameObject gameObject) {
            if (PCDSaveMgr.Current != null)
                PCDSaveMgr.Current.saveObjects.Add(gameObject);
        }
        static public void RemovePCDSaveGameObject(PCDSaveGameObject gameObject) {
            if (PCDSaveMgr.Current != null)
                PCDSaveMgr.Current.saveObjects.Remove(gameObject);
        }
        static public void PCDSaveAll() { Current.PCDSaveAllImpl(); }
        static public void PCDLoadAll() {
            loadWhenAwake = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void ForceSyncSaveObjects() {
            saveObjects.Clear();
            foreach (var gameObject in FindObjectsOfType<PCDSaveGameObject>()) {
                saveObjects.Add(gameObject);
            }
        }

        static public bool loadWhenAwake = false;
        static public bool loadWhenStart = false;
        private void Awake() {
            if (loadWhenAwake) {
                loadWhenAwake = false;
                PCDLoadAllImpl();
                ForceSyncSaveObjects();
            } else {
                ForceSyncSaveObjects();
            }
        }

        private void Start() {
            if (loadWhenStart) {
                loadWhenStart = false;
                PCDLoadAllImpl();
                ForceSyncSaveObjects();
            }
		}

        public void PCDSaveAllImpl() {
            var gameObjects = new List<GameObject>();
            foreach (var autoSave in saveObjects) {
                gameObjects.Add(autoSave.gameObject);
            }
            // Save in the same order as their depth in the hierarchy.
            var sortedObjects = gameObjects.OrderBy(x => GetDepth(x.transform)).ToArray();
            PCDSaveHelper.WriteSimpleGameObjects(ES3AutoSaveMgr.Current.key + "-Simple", sortedObjects, ES3AutoSaveMgr.Current.settings);
            PCDSaveHelper.WriteActualGameObjects(ES3AutoSaveMgr.Current.key, sortedObjects, ES3AutoSaveMgr.Current.settings);

            int GetDepth(Transform t) {
                int depth = 0;
                while (t.parent != null) {
                    t = t.parent;
                    depth++;
                }
                return depth;
            }
        }
        public void PCDLoadAllImpl() {
            ForceSyncSaveObjects();
            PCDSaveGameObject[] originalObjects = saveObjects.ToArray();

            ES3.Load<GameObject[]>(ES3AutoSaveMgr.Current.key + "-Simple", ES3AutoSaveMgr.Current.settings);
            var result = ES3.Load<GameObject[]>(ES3AutoSaveMgr.Current.key, ES3AutoSaveMgr.Current.settings);

            HashSet<GameObject> loadedGOs = new HashSet<GameObject>(result);
            foreach (var comp in originalObjects) {
                if (comp.saveForDestory && !loadedGOs.Contains(comp.gameObject))
                    Destroy(comp.gameObject);
			}
        }
	}
}