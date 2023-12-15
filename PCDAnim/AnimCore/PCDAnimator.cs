using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCDAnimator : MonoBehaviour
{
    public List<AnimItem> animList;
    private Dictionary<string, PCDAnimReader> animDict;

    private void Awake() {
        LoadAllAnim();
    }

    public void LoadAllAnim() {
        if (animDict == null) {
            animDict = new Dictionary<string, PCDAnimReader>();
            foreach (var item in animList) {
                animDict[item.animName] = new PCDAnimReader(item.animSO);
            }
        }
    }

    public PCDAnimReader GetAnimReader(string animName) {
        LoadAllAnim();
        return animDict[animName];
    }
}

[System.Serializable]
public struct AnimItem
{
    public string animName;
    public PCDAnim animSO;
}
