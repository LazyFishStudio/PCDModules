using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PCDAnim", menuName = "Custom/PCDAnim")]
public class PCDAnim : ScriptableObject
{
    public List<KeyFrameItem> keyFrameList;
}

[System.Serializable]
public struct KeyFrameItem
{
    public string keyFrameName;
    public GameObject keyFramePrefab;
}
