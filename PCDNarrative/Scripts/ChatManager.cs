using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeCanvas.DialogueTrees;

namespace PCD.Narrative
{
    /* Whole chat script -> script, single line of chat -> chat */
    public class ChatManager : SingletonMono<ChatManager> {
        public GameObject player;

        private void Awake() {
            if (!player)
                throw new System.Exception("Player of ChatManager is null!");

            DialogueTree.OnSubtitlesRequest += OnSubtitlesRequest;
            DialogueTree.OnMultipleChoiceRequest += OnMultipleChoiceRequest;

            EasyEvent.RegisterCallback("ShowChatBox", OnChatEnter);
            EasyEvent.RegisterCallback("HideChatBox", OnChatExit);
        }

        private DialogueActor sayingActor = null;
        private void OnSubtitlesRequest(SubtitlesRequestInfo info) {
            OnChatEnter();

            DialogueActor curActor = info.actor as DialogueActor;
            if (curActor != sayingActor && sayingActor != null) {
                ChatBubbleMgr.Instance.ActorStopSay(sayingActor);
            }
            sayingActor = curActor;
            ChatBubbleMgr.Instance.ActorSay(sayingActor, info.statement.text, null, () => info.Continue());
        }

        private void OnMultipleChoiceRequest(MultipleChoiceRequestInfo info) {
            info.SelectOption(0);
        }

        private bool isChatting = false;
        private void OnChatEnter() {
            if (!isChatting) {
                isChatting = true;
                SetPlayerLocker(true);
            }
        }
        private void OnChatExit() {
            if (isChatting) {
                isChatting = false;
                if (sayingActor != null)
                    ChatBubbleMgr.Instance.ActorStopSay(sayingActor);
                SetPlayerLocker(false);
            }
        }
        private void SetPlayerLocker(bool isLock) {
            if (!player) return;

            var locker = player.GetComponent<PCDActLocker>();
            locker.attackLocked = isLock;
            locker.interactionLocked = isLock;
            locker.movementLocked = isLock;
        }
    }
}
