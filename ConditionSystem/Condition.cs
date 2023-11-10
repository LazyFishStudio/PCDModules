using System;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEditor;
using UnityEngine;


[System.Serializable]
public class ConditionData {
    public int hpMax = 3;
    public int hp = 3;
    public float saitityMax = 1.0f;
    public float saitity = 0f;
    public ConditionData(ConditionConfig config) {
        hpMax = config.hpMax;
        hp = config.hp;
        saitityMax = config.saitityMax;
        saitity = config.saitity;
    }
}

public class Condition : MonoBehaviour {
    public ConditionConfig dataConfig;
    public ConditionData data;

    void Awake() {
        data = new ConditionData(dataConfig);
    }
    
    public bool GetSaitity(float num) {
        if (data.saitity >= data.saitityMax) {
            return false;
        }
        data.saitity = Mathf.Min(data.saitity + num, data.saitityMax);
        return true;
    }

    public bool LoseSaitity(float num) {
        if (data.saitity <= 0) {
            return false;
        }
        data.saitity = Mathf.Max(data.saitity - num, 0);
        return true;
    }

}


