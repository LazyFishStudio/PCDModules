using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCDArchoringMgr : MonoBehaviour
{
    private PCDAnimator animator;
    private PCDHumanConfig config;

    public PCDHuman.PCDHumanBoneSetting humanBone => config.humanBone;
	public PCDHuman.AnimSetting animSetting => config.animSetting;
	public PCDHuman.PoseInfo poseInfo => config.poseInfo;

    private Dictionary<string, PCDBoneDriver> nameDict;
    private void Awake() {
        animator = GetComponent<PCDAnimator>();
        config = GetComponent<PCDHumanConfig>();

        body = new PCDBoneDriver(humanBone.body.GetComponent<PCDBone>(), false);
        lFoot = new PCDBoneDriver(humanBone.lFoot.GetComponent<PCDBone>(), false);
        rFoot = new PCDBoneDriver(humanBone.rFoot.GetComponent<PCDBone>(), false);
        lHand = new PCDBoneDriver(humanBone.lHand.GetComponent<PCDBone>(), false);
        rHand = new PCDBoneDriver(humanBone.rHand.GetComponent<PCDBone>(), false);

        nameDict = new Dictionary<string, PCDBoneDriver> {
            {"Body", body},
            {"LFoot", lFoot},
            {"RFoot", rFoot},
            {"LHand", lHand},
            {"RHand", rHand}
        };
        targetDict = new Dictionary<string, Transform>();
        lerpRemTimeDict = new Dictionary<string, float>();
    }

    [SerializeField] private PCDBoneDriver body;
    [SerializeField] private PCDBoneDriver lFoot;
	[SerializeField] private PCDBoneDriver rFoot;
	[SerializeField] private PCDBoneDriver lHand;
	[SerializeField] private PCDBoneDriver rHand;

    public Transform testTrans;
    public string testPartName = "LHand";

    private void Update() {
        /*
        if (Input.GetKeyUp(KeyCode.P)) {
            BoneArchoringToTransform(testPartName, testTrans);
        }
        if (Input.GetKeyUp(KeyCode.O)) {
            ResetBoneFromArchoring(testPartName);
        }
        */

        foreach (var partName in nameDict.Keys) {
            var driver = nameDict[partName];
            if (!driver.CheckBoneOwnership())
                continue;

            if (targetDict.TryGetValue(partName, out Transform target) && target != null) {
                float lerpTime = lerpRemTimeDict[partName];
                if (lerpTime > 0.001f) {
                    lerpRemTimeDict[partName] -= Time.deltaTime;
                    float posVelocity = (target.position - driver.attachedBone.transform.position).magnitude / lerpTime;
                    Vector3 posRes = Vector3.Lerp(driver.attachedBone.transform.position, target.position, Time.deltaTime * posVelocity);
                    driver.SetGlobalPosition(posRes);
                } else {
                    driver.SetGlobalPosition(target.position);
                }
                driver.SetGlobalRotation(target.rotation);
            }
        }
    }

    private Dictionary<string, Transform> targetDict;
    private Dictionary<string, float> lerpRemTimeDict;
    public void BoneArchoringToTransform(string partName, Transform target, float lerpTime = 0.06f) {
        nameDict[partName].TryGetOwnership();
        targetDict[partName] = target;
        lerpRemTimeDict[partName] = lerpTime;
    }

    public void ResetBoneFromArchoring(string partName) {
        targetDict[partName] = null;
        nameDict[partName].ReturnOwnership();
    }
}
