using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;
using Febucci.UI.Core;
using TMPro;

namespace NodeCanvas.DialogueTrees.UI.Examples
{
    public class ChatGUI : MonoBehaviour, IPointerClickHandler
    {
        public Transform player;
        public Transform talkTarget;

        [System.Serializable]
        public class SubtitleDelays
        {
            public float characterDelay = 0.05f;
            public float sentenceDelay = 0.5f;
            public float commaDelay = 0.1f;
            public float finalDelay = 1.2f;
        }

        //Options...
        [Header("Input Options")]
        public bool skipOnInput;
        public bool waitForInput;

        //Group...
        [Header("Subtitles")]
        public RectTransform subtitlesGroup;
        public TextMeshProUGUI actorName;
        public TypewriterCore typewriter;
        public Text actorSpeech;
        public Image actorPortrait;
        public RectTransform waitInputIndicator;
        public SubtitleDelays subtitleDelays = new SubtitleDelays();
        public List<AudioClip> typingSounds;
        private AudioSource playSource;

        //Group...
        [Header("Multiple Choice")]
        public RectTransform optionsGroup;
        public Button optionButton;
        private Dictionary<Button, int> cachedButtons;
        private Vector2 originalSubsPosition;
        private bool isWaitingChoice;

        private AudioSource _localSource;
        private AudioSource localSource {
            get { return _localSource != null ? _localSource : _localSource = gameObject.AddComponent<AudioSource>(); }
        }


        private bool anyKeyDown;
        public void OnPointerClick(PointerEventData eventData) => anyKeyDown = true;
        void LateUpdate() => anyKeyDown = false;

        void Awake() {
            EasyEvent.RegisterCallback("ShowChatBox", ShowChatBox);
            EasyEvent.RegisterCallback("HideChatBox", HideChatBox);

            Subscribe(); Hide(); }
        void OnEnable() { UnSubscribe(); Subscribe(); }
        void OnDisable() { UnSubscribe(); }

        void Subscribe() {
            DialogueTree.OnDialogueStarted += OnDialogueStarted;
            DialogueTree.OnDialoguePaused += OnDialoguePaused;
            DialogueTree.OnDialogueFinished += OnDialogueFinished;
            DialogueTree.OnSubtitlesRequest += OnSubtitlesRequest;
            DialogueTree.OnMultipleChoiceRequest += OnMultipleChoiceRequest;
        }

        void UnSubscribe() {
            DialogueTree.OnDialogueStarted -= OnDialogueStarted;
            DialogueTree.OnDialoguePaused -= OnDialoguePaused;
            DialogueTree.OnDialogueFinished -= OnDialogueFinished;
            DialogueTree.OnSubtitlesRequest -= OnSubtitlesRequest;
            DialogueTree.OnMultipleChoiceRequest -= OnMultipleChoiceRequest;
        }

        void Hide() {
            Debug.Log("Hide");
            subtitlesGroup.gameObject.SetActive(false);
            optionsGroup.gameObject.SetActive(false);
            optionButton.gameObject.SetActive(false);
            waitInputIndicator.gameObject.SetActive(false);
            originalSubsPosition = subtitlesGroup.transform.position;
        }

        void OnDialogueStarted(DialogueTree dlg) {
            //nothing special...
        }

        void OnDialoguePaused(DialogueTree dlg) {
            Debug.Log("OnDialoguePaused");
            StopAllCoroutines();
            if (playSource != null)
                playSource.Stop();
        }

        void OnDialogueFinished(DialogueTree dlg) {
            Debug.Log("OnDialoguePaused");
            if (cachedButtons != null) {
                foreach (var tempBtn in cachedButtons.Keys) {
                    if (tempBtn != null) {
                        Destroy(tempBtn.gameObject);
                    }
                }
                cachedButtons = null;
            }
            StopAllCoroutines();
            if (playSource != null)
                playSource.Stop();
        }

        private bool isShowingChatBox = false;
        public void ShowChatBox() {
            if (!isShowingChatBox) {
                isShowingChatBox = true;
                OnChatBoxShow();
            }
        }
        public void HideChatBox() {
            if (isShowingChatBox) {
                isShowingChatBox = false;
                OnChatBoxHide();
            }
		}

        public void OnChatBoxShow() {
            var locker = player.GetComponent<PCDActLocker>();
            locker.attackLocked = true;
            locker.interactionLocked = true;
            locker.movementLocked = true;
        }

        public void OnChatBoxHide() {
            var locker = player.GetComponent<PCDActLocker>();
            locker.attackLocked = false;
            locker.interactionLocked = false;
            locker.movementLocked = false;
        }

        ///----------------------------------------------------------------------------------------------
        private SubtitlesRequestInfo curInfo;

        void OnSubtitlesRequest(SubtitlesRequestInfo info) {
            Debug.Log("OnSubtitlesRequest");
            ShowChatBox();

            var text = info.statement.text;
            var audio = info.statement.audio;
            var actor = info.actor;

            subtitlesGroup.gameObject.SetActive(true);
            subtitlesGroup.position = originalSubsPosition;

            actorName.text = actor.name;
            actorPortrait.gameObject.SetActive(actor.portraitSprite != null);
            actorPortrait.sprite = actor.portraitSprite;

            // audio
            // actorSpeech.color = actor.dialogueColor;
            // skipOnInput

            curInfo = info;
            typewriter.ShowText(text);
        }

        private void Update() {
            if (typewriter.gameObject.activeInHierarchy && !typewriter.isShowingText && Input.GetKeyDown(KeyCode.Mouse0)) {
                if (curInfo != null) {
                    Debug.Log("MyContinue");
                    subtitlesGroup.gameObject.SetActive(false);
                    optionsGroup.gameObject.SetActive(false);
                    curInfo.Continue();
                }
            }
		}

        void PlayTypeSound() {
            if (typingSounds.Count > 0) {
                var sound = typingSounds[Random.Range(0, typingSounds.Count)];
                if (sound != null) {
                    localSource.PlayOneShot(sound, Random.Range(0.6f, 1f));
                }
            }
        }

        IEnumerator CheckInput(System.Action Do) {
            while (!anyKeyDown) {
                yield return null;
            }
            Do();
        }

        IEnumerator DelayPrint(float time) {
            var timer = 0f;
            while (timer < time) {
                timer += Time.deltaTime;
                yield return null;
            }
        }

        ///----------------------------------------------------------------------------------------------

        void OnMultipleChoiceRequest(MultipleChoiceRequestInfo info) {
            Debug.Log("OnMultipleChoiceRequest");
            ShowChatBox();
            curInfo = null;

            optionsGroup.gameObject.SetActive(true);
            var buttonHeight = optionButton.GetComponent<RectTransform>().rect.height;
            optionsGroup.sizeDelta = new Vector2(optionsGroup.sizeDelta.x, (info.options.Values.Count * buttonHeight) + 20);

            cachedButtons = new Dictionary<Button, int>();
            int i = 0;

            foreach (KeyValuePair<IStatement, int> pair in info.options) {
                var btn = (Button)Instantiate(optionButton);
                btn.gameObject.SetActive(true);
                btn.transform.SetParent(optionsGroup.transform, false);
                btn.transform.localPosition = (Vector3)optionButton.transform.localPosition - new Vector3(0, buttonHeight * i, 0);
                btn.GetComponentInChildren<Text>().text = pair.Key.text;
                cachedButtons.Add(btn, pair.Value);
                btn.onClick.AddListener(() => { Finalize(info, cachedButtons[btn]); });
                i++;
            }

            if (info.showLastStatement) {
                subtitlesGroup.gameObject.SetActive(true);
                var newY = optionsGroup.position.y + optionsGroup.sizeDelta.y + 1;
                subtitlesGroup.position = new Vector3(subtitlesGroup.position.x, newY, subtitlesGroup.position.z);
            }

            if (info.availableTime > 0) {
                StartCoroutine(CountDown(info));
            }
        }

        IEnumerator CountDown(MultipleChoiceRequestInfo info) {
            isWaitingChoice = true;
            var timer = 0f;
            while (timer < info.availableTime) {
                if (isWaitingChoice == false) {
                    yield break;
                }
                timer += Time.deltaTime;
                SetMassAlpha(optionsGroup, Mathf.Lerp(1, 0, timer / info.availableTime));
                yield return null;
            }

            if (isWaitingChoice) {
                Finalize(info, info.options.Values.Last());
            }
        }

        void Finalize(MultipleChoiceRequestInfo info, int index) {
            isWaitingChoice = false;
            SetMassAlpha(optionsGroup, 1f);
            optionsGroup.gameObject.SetActive(false);
            subtitlesGroup.gameObject.SetActive(false);
            foreach (var tempBtn in cachedButtons.Keys) {
                Destroy(tempBtn.gameObject);
            }
            info.SelectOption(index);
        }

        void SetMassAlpha(RectTransform root, float alpha) {
            foreach (var graphic in root.GetComponentsInChildren<CanvasRenderer>()) {
                graphic.SetAlpha(alpha);
            }
        }
    }
}
