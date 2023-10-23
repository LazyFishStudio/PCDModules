using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;

namespace PCD.EventSystem
{
    public class MoveAreaEventer : MonoBehaviour {
        private class AreaCheckerComp : MonoBehaviour
		{
            private MoveAreaEventConfig config;

            public void SetupEventInfo(MoveAreaEventConfig config) {
                this.config = config;
            }

			private void OnTriggerEnter(Collider other) {
				if (config != null && config.moveAreaType == MoveAreaType.AreaEnter &&
                    (config.character == null || config.character == other)) {
                    MMEventManager.TriggerEvent(new MoveAreaEvent(config.eventName, config.moveAreaType, other, config.area));
                }
			}
			private void OnTriggerStay(Collider other) {
                if (config != null && config.moveAreaType == MoveAreaType.AreaStay &&
                    (config.character == null || config.character == other)) {
                    MMEventManager.TriggerEvent(new MoveAreaEvent(config.eventName, config.moveAreaType, other, config.area));
                }
            }
            private void OnTriggerExit(Collider other) {
                if (config != null && config.moveAreaType == MoveAreaType.AreaExit &&
                    (config.character == null || config.character == other)) {
                    MMEventManager.TriggerEvent(new MoveAreaEvent(config.eventName, config.moveAreaType, other, config.area));
                }
            }
        }

        public List<MoveAreaEventConfig> eventConfigs;

        public void Awake() {
            foreach (var config in eventConfigs) {
                RegisterEvent(config);
            }
		}

        static public void RegisterEvent(MoveAreaEventConfig config) {
            if (config.area == null)
                throw new System.Exception("Area of MoveAreaEvent " + config + " is null!");
            if (!config.area.isTrigger)
                throw new System.Exception("Area of MoveAreaEvent " + config + " is not trigger!");

            AreaCheckerComp areaChecker = config.area.gameObject.AddComponent<AreaCheckerComp>();
            areaChecker.SetupEventInfo(config);
        }
	}

    [System.Serializable]
    public class MoveAreaEventConfig
	{
        public string eventName;
        public MoveAreaType moveAreaType;
        public Collider character;
        public Collider area;

        public MoveAreaEventConfig(string eventName, MoveAreaType moveAreaType, Collider character, Collider area) {
            this.eventName = eventName;
            this.moveAreaType = moveAreaType;
            this.character = character;
            this.area = area;
        }
    }

    public class MoveAreaEvent
	{
        public string eventName;
        public MoveAreaType moveAreaType;
        public Collider character;
        public Collider area;

        public MoveAreaEvent(string eventName, MoveAreaType moveAreaType, Collider character, Collider area) {
            this.eventName = eventName;
            this.moveAreaType = moveAreaType;
            this.character = character;
            this.area = area;
		}
    }

    public enum MoveAreaType { AreaEnter, AreaStay, AreaExit }
}
