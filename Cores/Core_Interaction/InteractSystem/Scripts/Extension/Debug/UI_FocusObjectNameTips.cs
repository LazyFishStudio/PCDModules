using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class UI_FocusObjectNameTips : SingletonMono<UI_FocusObjectNameTips> {
    public TextMeshProUGUI text;
    public string content;
    public GameObject background;

    void Update() {
        // content = "文字测试文字测试文字测试文字测试文字测试\n文字测试文字测试文字测试文字测试文字测试文字测试";
        background.SetActive(content != "");
        text.SetText(content);
        content = "";
    }

}
