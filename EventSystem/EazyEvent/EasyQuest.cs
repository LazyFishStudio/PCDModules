using System.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 1. 帮忙拼接字符串 - Quest[{0}]Unlock，有 Unlock/Finish 但是不分 Phase
 * 2. 帮忙注册对应的回调函数
 * 3. 记录哪些任务正在进行，哪些任务已经完成
 */
public class EasyQuest : SingletonMono<EasyQuest>
{
	public enum QuestState { Locked, Active, Finished }

	private HashSet<string> activeQuests;
	private HashSet<string> finishedQuests;
	private HashSet<string> questPhases;
	private void Awake() {
		activeQuests = new HashSet<string>();
		finishedQuests = new HashSet<string>();
		questPhases = new HashSet<string>();
	}
	/* Register EasyQuest, onUnlock can also start async function to track quest progesss  */
	static public void RegisterQuest(string questName, Action onUnlock = null, Action onFinish = null, bool autoUnlock = false) {
		if (onUnlock != null)
			RegisterQuestPhaseCallback(questName, "Unlock", onUnlock);
		if (onFinish != null)
			RegisterQuestPhaseCallback(questName, "Finish", onFinish);
		if (autoUnlock)
			UnlockQuest(questName);
	}
	static public void FinishQuestPhase(string questName, string phaseName) {
		string phaseEventName = string.Format("Quest[{0}]{1}", questName, phaseName);
		GetInstance().questPhases.Add(phaseEventName);
		EasyEvent.TriggerEvent(phaseEventName);
	}
	static public void RegisterQuestPhaseCallback(string questName, string phaseName, Action callback) {
		EasyEvent.RegisterOnceCallback(string.Format("Quest[{0}]{1}", questName, phaseName), callback);
	}
	static public void UnlockQuest(string questName) {
		GetInstance().activeQuests.Add(questName);
		FinishQuestPhase(questName, "Unlock");
	}
	static public void FinishQuest(string questName) {
		GetInstance().finishedQuests.Add(questName);
		FinishQuestPhase(questName, "Finish");
	}
	static public QuestState GetQuestState(string questName) {
		if (!GetInstance().activeQuests.Contains(questName))
			return QuestState.Locked;
		if (GetInstance().finishedQuests.Contains(questName))
			return QuestState.Finished;
		return QuestState.Active;
	}
	static public bool CheckQuestPhase(string questName, string phaseName) {
		string phaseEventName = string.Format("Quest[{0}]{1}", questName, phaseName);
		return GetInstance().questPhases.Contains(phaseEventName);
	}
	static public bool IsQuestActive(string questName) {
		return GetInstance().activeQuests.Contains(questName) && !GetInstance().finishedQuests.Contains(questName);
	}
	static public bool IsQuestFinished(string questName) {
		return GetInstance().finishedQuests.Contains(questName);
	}
	static public async Task WaitQuestPhase(string questName, string phaseName) {
		await EasyEvent.WaitForEvent(string.Format("Quest[{0}]{1}", questName, phaseName));
	}
	static public async Task WaitQuestUnlock(string questName) {
		await WaitQuestPhase(questName, "Unlock");
	}
	static public async Task WaitQuestFinish(string questName) {
		await WaitQuestPhase(questName, "Finish");
	}
}
