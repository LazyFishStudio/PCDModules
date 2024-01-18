using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshUI : MonoBehaviour {
    public bool isBillboard;
    public GameObject meshUI;
    public bool showOnStart;
    private bool isUIActive;

    void Awake() {
        if (!showOnStart) {
            HideUI();
        }
    }

    void Update() {
        if (isBillboard) {
            meshUI.transform.rotation = Quaternion.LookRotation(Camera.main.transform.position - transform.position);
        }
    }

    public void ShowUI() {
        meshUI.SetActive(true);
    }

    public void HideUI() {
        meshUI.SetActive(false);
    }

}
