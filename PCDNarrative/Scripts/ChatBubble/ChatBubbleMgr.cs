using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NodeCanvas.DialogueTrees;

public class ChatBubbleMgr : SingletonMono<ChatBubbleMgr>
{
    static public Dictionary<DialogueActor, ChatBubble> bubbleDict = new Dictionary<DialogueActor, ChatBubble>();

    public GameObject chatBubblePrefab;

    private void Awake() {
        bubbleDict.Clear();
    }

    public void ActorSay(DialogueActor actor, string text, System.Action textShowedCallback, System.Action continueNextCallback) {
        ChatBubble chatBubble = null;
        if (!bubbleDict.TryGetValue(actor, out chatBubble) || !chatBubble) {
            chatBubble = Instantiate(chatBubblePrefab, null).GetComponentInChildren<ChatBubble>();
            bubbleDict[actor] = chatBubble;
        }
        chatBubble.GetComponent<UIFollow>().target = actor.transform;
        chatBubble.GetComponent<UIFollow>().bias = actor.dialoguePosition;
        chatBubble.ShowText(text, textShowedCallback, continueNextCallback);
    }

    public void ActorStopSay(DialogueActor actor) {
        ChatBubble chatBubble = null;
        if (bubbleDict.TryGetValue(actor, out chatBubble) && chatBubble) {
            chatBubble.ResetText();
            chatBubble.gameObject.SetActive(false);
        }
    }

    /*
    public GameObject testObject;
    private void Update() {
        if (testObject && Input.GetKeyDown(KeyCode.U)) {
            ActorSay(testObject.GetComponent<DialogueActor>(), "阿萨的啊实打实发生发生广东省高峰对话请问如何i日日u也日趋我以前我有看就看哈克好看就好看看看艰苦好看就好看", null);
        } 
	}
    */
}
