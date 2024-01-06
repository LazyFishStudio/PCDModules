using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Febucci.UI.Core;
using UnityEngine.UI;
using DG.Tweening;
using System.Threading.Tasks;

[RequireComponent(typeof(UIFollow))]
public class ChatBubble : MonoBehaviour
{
    public RectTransform body;
    public Image background;
    public TypewriterCore typeWriter;
    public TextMeshProUGUI textMesh;
    public Vector2 paddingChar = new Vector2(2f, 2f);
    public bool debugWithU = false;

    private System.Action _textShowedCallback;
    private System.Action _continueNextCallback;

    private void OnEnable() => typeWriter.onTextShowed.AddListener(OnTextShowed);
    private void OnDisable() => typeWriter.onTextShowed.RemoveListener(OnTextShowed);

    public void ResetText() {
        textShowed = false;
        _textShowedCallback = null;
        _continueNextCallback = null;
        textMesh.SetText("");
        textMesh.ForceMeshUpdate(true);
    }

    public void ShowText(string text, System.Action textShowedCallback, System.Action continueNextCallback) {
        ResetText();

        textMesh.SetText(text);
        textMesh.ForceMeshUpdate(true);
        Vector2 rawTextSize = textMesh.GetRenderedValues(false);
        Vector2 textSize = rawTextSize + paddingChar * textMesh.fontSize;

        textMesh.SetText(""); 
        Vector2 newBodyPos = new Vector2(0f, (rawTextSize.y - textMesh.fontSize) * 0.5f);
        if (!gameObject.activeInHierarchy) {
            body.anchoredPosition = newBodyPos;
            background.rectTransform.sizeDelta = textSize;
            gameObject.SetActive(true);
        } else {
            body.DOAnchorPos(newBodyPos, 0.05f);
            background.rectTransform.DOSizeDelta(textSize, 0.05f);
        }

        System.Action delayShowText = async () => {
            await Task.Delay((int)(0.05f * 1000f));
            _textShowedCallback = textShowedCallback;
            _continueNextCallback = continueNextCallback;
            typeWriter.ShowText(text);
        };
        delayShowText.Invoke();
    }

    private bool textShowed = false;
    private void OnTextShowed() {
        Debug.Log("OnTextShowed");
        textShowed = true;
        var prevCallback = _textShowedCallback;
        _textShowedCallback = null;
        prevCallback?.Invoke();
    }

    private void OnContinueNext() {
        Debug.Log("OnContinueNext");
        textShowed = false;
        var prevCallback = _continueNextCallback;
        _continueNextCallback = null;
        prevCallback?.Invoke();
    }

	public void Update() {
        if (Input.anyKeyDown) {
            if (typeWriter.isShowingText) {
                typeWriter.SkipTypewriter();
            } else if (textShowed) {
                OnContinueNext();
            }
        }
	}

	/*
    private int idx = -1;
    private string[] textStr = { "你好", "什么东西", "很高兴见到你，早上好，晚上好，下午好" };
    private void Update() {
        if (debugWithU && Input.GetKeyDown(KeyCode.U)) {
            idx = (idx + 1) % textStr.Length;
            ShowText(textStr[idx], null);
		}
	}
    */
}
