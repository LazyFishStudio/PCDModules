using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InteractSystem;
using NodeCanvas.DialogueTrees;

namespace PCD.Narrative
{
    public class SimpleQuestNPC : SimpleNPC
    {
        public string questName;
        public DialogueTree hintTree;

		/* Must modify questState when finish quest. */
		public override bool OnInteract(InteractComp interactor) {
            var actor = GetComponent<DialogueActor>();
            if (actor == null)
                throw new System.Exception(string.Format("{0} has no DialogueActor found!", name));
            EasyEvent.TriggerEvent(string.Format("TalkWithNPC[{0}]", actor.name));

            switch (EasyQuest.GetQuestState(questName)) {
                case EasyQuest.QuestState.Locked: {
                    GetComponent<DialogueTreeController>().StartDialogue(dialogueTree, GetComponent<DialogueActor>(), null);
                    break;
                }
                case EasyQuest.QuestState.Active: {
                    GetComponent<DialogueTreeController>().StartDialogue(hintTree, GetComponent<DialogueActor>(), null);
                    break;
                }
                case EasyQuest.QuestState.Finished: {
                    break;
				}
            }
            return true;
        }
    }
}
