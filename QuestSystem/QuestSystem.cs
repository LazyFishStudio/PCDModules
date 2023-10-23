using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PCD.QuestSystem
{
    public class QuestSystem : SingletonMono<QuestSystem>
    {
		public GameObject debugQuestUI;
        public List<PCDQuest> questList = new List<PCDQuest>();

		static public void RegisterQuest(PCDQuest quest) {
			GetInstance().questList.Add(quest);
		}

		static public void RemoveQuest(PCDQuest quest) {
			GetInstance().questList.Remove(quest);
		}

		private void Start() {
			foreach (var obj in FindObjectsOfType<MonoBehaviour>()) {
				if (obj is PCDQuest quest)
					RegisterQuest(quest);
			}
		}

		private void Update() {
			/*
			if (debugQuestUI != null && Input.GetKeyDown(KeyCode.Q))
				debugQuestUI.SetActive(!debugQuestUI.activeInHierarchy);
			*/
		}
	}
}
