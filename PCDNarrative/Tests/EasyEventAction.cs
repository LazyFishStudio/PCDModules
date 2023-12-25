using UnityEngine;
using NodeCanvas.Framework;
using PCD.Narrative;
using NodeCanvas.DialogueTrees;

public class EasyEventAction : ActionTask
{
	public string eventName;

	protected override void OnExecute() {
		EasyEvent.TriggerEvent(eventName);
		EndAction(true);
	}
}

public class EasyEventWaitAction : ActionTask
{
	public string eventName;

	protected override void OnExecute() {
		EasyEvent.RegisterOnceCallback(eventName, () => {
			EndAction(true);
		});
	}
}

public class TriggerNarrQuestPhaseAction : ActionTask
{
	public string phaseName;

	protected override void OnExecute() {
		string questName = blackboard.GetVariable<string>("QuestName").GetValue();
		EasyQuest.FinishQuestPhase(questName, phaseName);
		EndAction(true);
	}
}

public class WaitForNarrQuestPhaseAction : ActionTask
{
	public string phaseName;

	protected override void OnExecute() {
		string questName = blackboard.GetVariable<string>("QuestName").GetValue();
		EasyQuest.RegisterQuestPhaseCallback(questName, phaseName, () => {
			EndAction(true);
		});
	}
}

public class NarrQuestAccept : ActionTask
{
	protected override void OnExecute() {
		string questName = blackboard.GetVariable<string>("QuestName").GetValue();
		EasyQuest.UnlockQuest(questName);
		EndAction(true);
	}
}

public class NarrQuestFinish : ActionTask
{
	public string givenQuestName;

	protected override void OnExecute() {
		string questName = givenQuestName;
		if (questName == null || questName == "")
			questName = blackboard.GetVariable<string>("QuestName").GetValue();
		EasyQuest.FinishQuest(questName);
		EndAction(true);
	}
}

public class WaitForNarrQuestFinish : ActionTask
{
	protected override void OnExecute() {
		string questName = blackboard.GetVariable<string>("QuestName").GetValue();
		EasyQuest.RegisterQuest(questName, onFinish: () => {
			EndAction(true);
		});
	}
}

public class WaitForTalkWithNPC : ActionTask
{
	public string NPCName;

	protected override void OnExecute() {
		string eventName = string.Format("TalkWithNPC[{0}]", NPCName);
		EasyEvent.RegisterOnceCallback(eventName, () => {
			EndAction(true);
		});
	}
}

public class ShowChatBox : ActionTask
{
	protected override void OnExecute() {
		EasyEvent.TriggerEvent("ShowChatBox");
		EndAction(true);
	}
}

public class HideChatBox : ActionTask
{
	protected override void OnExecute() {
		EasyEvent.TriggerEvent("HideChatBox");
		EndAction(true);
	}
}