using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeCanvas.DialogueTrees;

namespace PCD.Narrative
{
    public class BoxQuest : MonoBehaviour
    {
        public DialogueTree dialogueTree;
        public DialogueActor actor;

        private void Awake() {
            EasyEvent.RegisterOnceCallback("PickQuestBox", () => {
                ChatManager.Instance.controller.StartDialogue(dialogueTree, actor, null);
                // actor.GetComponent<DialogueTreeController>().StartDialogue(dialogueTree, actor, null);
            });
        }
    }
}

