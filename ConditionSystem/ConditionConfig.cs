using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "MySO/ConditionDataConfig")]
public class ConditionConfig : ScriptableObject {
    public int hpMax = 3;
    public int hp = 3;
    public float saitityMax = 1.0f;
    public float saitity = 0f;

}
