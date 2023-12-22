using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using InteractSystem;

public class PCDHintManager : MonoBehaviour, IPCDActionHandler
{
    public bool useMobileHint = false;
    public PCDPlayerInteractionManager manager;
    public List<GameObject> hintBodyList;
    public List<GameObject> mobileHintList;

    static private Dictionary<string, InteractHintType> keyTypeDict = new Dictionary<string, InteractHintType> {
        { "FirstInteract", InteractHintType.MouseLeft },
        { "SecondInteract", InteractHintType.MouseRight },
        { "F", InteractHintType.KeyF }
    };

    private void ShowHintUI(GameObject hintUI, InteractHintType type, string text) {
        hintUI.SetActive(true);
        hintUI.GetComponent<HintTypeSelector>().RefreshHintType(type);
        var mesh = hintUI.GetComponentInChildren<TextMeshProUGUI>();
        if (mesh != null) {
            mesh.text = text;
        }
	}

    private void ShowMobileHintUI(GameObject hintUI, InteractHintType type, string text) {
        hintUI.SetActive(true);
        // hintUI.GetComponent<HintTypeSelector>().RefreshHintType(type);
        var mesh = hintUI.GetComponentInChildren<TextMeshProUGUI>();
        if (mesh != null) {
            mesh.text = text;
        }
    }

    private void HideHintUI(GameObject hintUI) {
        hintUI.SetActive(false);
	}

    private void HideMobileHintUI(GameObject hintUI) {
        hintUI.SetActive(false);
    }

    private void Start() {
        PCDPlayerActionManager actionManager = PCDPlayerActionManager.GetInstance();
        actionManager.RegisterLateActionHandler(this as IPCDActionHandler);
    }

	private void Update() {
		if (Input.GetKeyDown(KeyCode.L)) {
            foreach (var ui in mobileHintList) {
                ui.SetActive(!ui.activeSelf);
			}
		}
	}

	public void RegisterActionOnUpdate() {
        PCDPlayerActionManager actionManager = PCDPlayerActionManager.GetInstance();
        
        if (!useMobileHint) {
            for (int i = 0; i < actionManager.actionDescList.Count; i++) {
                if (i >= hintBodyList.Count) {
                    Debug.Log(actionManager.actionDescList[i].ToString() + ": " + actionManager.actionDescList[i].keyDesc);
                    Debug.LogError(string.Format("Can't show more than {0} hints!", hintBodyList.Count));
                    break;
                }
                var desc = actionManager.actionDescList[i];
                ShowHintUI(hintBodyList[i], keyTypeDict[desc.keyName], desc.keyDesc);
            }
            for (int i = actionManager.actionDescList.Count; i < hintBodyList.Count; i++) {
                HideHintUI(hintBodyList[i]);
            }
        } else {
            bool hideFirst = true;
            bool hideSecond = true;
            for (int i = 0; i < actionManager.actionDescList.Count; i++) {
                var desc = actionManager.actionDescList[i];
                if (desc.keyName == "FirstInteract") {
                    hideFirst = false;
                    ShowMobileHintUI(mobileHintList[0], keyTypeDict[desc.keyName], desc.keyDesc);
                } else if (desc.keyName == "SecondInteract") {
                    hideSecond = false;
                    ShowMobileHintUI(mobileHintList[1], keyTypeDict[desc.keyName], desc.keyDesc);
                }
            }
            if (hideFirst) {
                HideMobileHintUI(mobileHintList[0]);
            }
            if (hideSecond) {
                HideMobileHintUI(mobileHintList[1]);
            }
        }
    }
}
