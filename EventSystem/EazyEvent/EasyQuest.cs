using System.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EasyQuest : SingletonMono<EasyQuest>
{
	private HashSet<string> activeQuests;
	private HashSet<string> finishedQuests;
	private void Awake() {
		activeQuests = new HashSet<string>();
		finishedQuests = new HashSet<string>();
	}
	/* Register EasyQuest, onUnlock can also start async function to track quest progesss  */
	static public void RegisterEasyQuest(string questName, Action onUnlock = null, Action onFinish = null, bool autoUnlock = false) {
		if (onUnlock != null)
			EasyEvent.RegisterOnceCallback(string.Format("Quest[{0}]Unlock", questName), onUnlock);
		if (onFinish != null)
			EasyEvent.RegisterOnceCallback(string.Format("Quest[{0}]Finish", questName), onFinish);
		if (autoUnlock)
			UnlockEasyQuest(questName);
	}
	static public void UnlockEasyQuest(string questName) {
		GetInstance().activeQuests.Add(questName);
		EasyEvent.TriggerEvent(string.Format("Quest[{0}]Unlock", questName));
	}
	static public void FinishEasyQuest(string questName) {
		GetInstance().finishedQuests.Add(questName);
		EasyEvent.TriggerEvent(string.Format("Quest[{0}]Finish", questName));
	}
	static public bool IsQuestActive(string questName) {
		return GetInstance().activeQuests.Contains(questName) && !GetInstance().finishedQuests.Contains(questName);
	}
	static public bool IsQuestFinished(string questName) {
		return GetInstance().finishedQuests.Contains(questName);
	}
	static public async Task WaitQuestUnlock(string questName) {
		await EasyEvent.WaitForEvent(string.Format("Quest[{0}]Unlock", questName));
	}
	static public async Task WaitQuestFinish(string questName) {
		await EasyEvent.WaitForEvent(string.Format("Quest[{0}]Finish", questName));
	}
}
