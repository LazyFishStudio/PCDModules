using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using InteractSystem;
using MoreMountains.Tools;
using System.Linq;

namespace PCD.EventSystem
{
    public class InteractEventer : MonoBehaviour
	{
        static public InteractEventer Inst;
        private void Awake() {
            if (Inst != null)
                throw new System.Exception("More than one InteractEventer exists!");
            Inst = this;
        }

		private void OnEnable() {
            InteractLogger.interactLogListeners += HandleInteractSystemEvent;
        }
		private void OnDisable() {
            Inst = null;
            InteractLogger.interactLogListeners -= HandleInteractSystemEvent;
        }

		static public void RegisterEvent(InteractEventConfig config) {
            if (Inst == null)
                throw new System.Exception("InteractEventer doesn't exist!");
            Inst.eventConfigs.Add(config);
        }

        public List<InteractEventConfig> eventConfigs = new List<InteractEventConfig>();

        private void HandleInteractSystemEvent(InteractSystem.InteractEvent interactEvent) {
            // Debug.Log("Accept Event: " + interactEvent.eventType);

            foreach (var config in eventConfigs) {
                if (interactEvent.eventType != config.interactType.ToString())
                    continue;
                if (config.interactor != null && interactEvent.obj1 != config.interactor)
                    continue;
                if (config.target != null && interactEvent.obj2 != config.target)
                    continue;

                if (config.CheckConditions(interactEvent)) {
                    MMEventManager.TriggerEvent(new InteractEvent(config.eventName, config.interactType, interactEvent.obj1, interactEvent.obj2));
                }
			}
		}
    }

    [System.Serializable]
    public class InteractEventConfig
    {
        public string eventName;
        public InteractType interactType;
        public GameObject interactor;
        public GameObject target;

        private bool hookConditionsInstalled;
        public List<ConditionHookConfig> conditionHooks;
        private List<System.Func<InteractSystem.InteractEvent, bool>> conditions;

        public InteractEventConfig(string eventName, InteractType interactType, GameObject interactor, GameObject target) {
            this.eventName = eventName;
            this.interactType = interactType;
            this.interactor = interactor;
            this.target = target;

            hookConditionsInstalled = false;
        }

        public void AddCondition(System.Func<InteractSystem.InteractEvent, bool> condition) {
            if (conditions == null)
                conditions = new List<System.Func<InteractSystem.InteractEvent, bool>>();
            conditions.Add(condition);
		}

        public bool CheckConditions(InteractSystem.InteractEvent interactEvent) {
            if (!hookConditionsInstalled && conditionHooks != null) {
                foreach (var config in conditionHooks) {
                    InteractConditionHook.InstallConditionHook(this, config.conditionType, config.arg);
				}
                hookConditionsInstalled = true;
            }
            if (conditions == null)
                return true;

            foreach (var condition in conditions) {
                if (!condition.Invoke(interactEvent))
                    return false;
			}
            return true;
        }
    }

    public class InteractEvent
    {
        public string eventName;
        public InteractType interactType;
        public GameObject interactor;
        public GameObject target;

        public InteractEvent(string eventName, InteractType interactType, GameObject interactor, GameObject target) {
            this.eventName = eventName;
            this.interactType = interactType;
            this.interactor = interactor;
            this.target = target;
        }
    }

    [System.Serializable]
    public struct ConditionHookConfig
	{
        public InteractConditionType conditionType;
        public string arg;
    }

    public enum InteractType {
        Pick, Drop, Throw, StartPull, ResetPull, PulledOut, PulledOutAndPick,
        InteractStart, InteractFinish, InteractTerminate,
        FocusEnter, FocusStay, FocusExit,
        PlaceItem, RemoveItem
    }
}
