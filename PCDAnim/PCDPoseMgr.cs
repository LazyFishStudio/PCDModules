using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCDPoseMgr : MonoBehaviour
{
    private PCDAnimator animator;
    private PCDHumanConfig config;

    public PCDSkeleton skeleton => config.skeleton;
	public PCDHuman.AnimSetting animSetting => config.animSetting;
	public PCDHuman.PoseInfo poseInfo => config.poseInfo;

    private Dictionary<string, PCDBoneDriver> nameDict;
    private void Awake() {
        animator = GetComponent<PCDAnimator>();
        config = GetComponent<PCDHumanConfig>();

        body = new PCDBoneDriver(skeleton.GetBone("Body"), false);
        lFoot = new PCDBoneDriver(skeleton.GetBone("LFoot"), false);
        rFoot = new PCDBoneDriver(skeleton.GetBone("RFoot"), false);
        lHand = new PCDBoneDriver(skeleton.GetBone("LHand"), false);
        rHand = new PCDBoneDriver(skeleton.GetBone("RHand"), false);

        nameDict = new Dictionary<string, PCDBoneDriver> {
            {"Body", body},
            {"LFoot", lFoot},
            {"RFoot", rFoot},
            {"LHand", lHand},
            {"RHand", rHand}
        };
    }

    [SerializeField] private PCDBoneDriver body;
    [SerializeField] private PCDBoneDriver lFoot;
	[SerializeField] private PCDBoneDriver rFoot;
	[SerializeField] private PCDBoneDriver lHand;
	[SerializeField] private PCDBoneDriver rHand;

    
    private void Update() {
        if (Input.GetKeyUp(KeyCode.P)) {
            FadeToKeyFrame(animator.GetAnimReader("Sit").GetKeyFrameReader("Sit"));
        }
        if (Input.GetKeyUp(KeyCode.O)) {
            ResetPose();
        }
    }
   

    public void FadeToKeyFrame(PCDKFReader kfReader, bool ctlBody = true, bool ctlLFoot = true, bool ctlRFoot = true, bool ctlLHand = true, bool ctlRHand = true) {
        if (ctlBody)
            ctlBody = ctlBody && body.TryGetOwnership();
        if (ctlLFoot)
            ctlLFoot = ctlLFoot && lFoot.TryGetOwnership();
        if (ctlRFoot)
            ctlRFoot = ctlRFoot && rFoot.TryGetOwnership();
        if (ctlLHand)
            ctlLHand = ctlLHand && lHand.TryGetOwnership();
        if (ctlRHand)
            ctlRHand = ctlRHand && rHand.TryGetOwnership();

        foreach (var boneName in nameDict.Keys) {
            PCDBoneDriver driver = nameDict[boneName];
            driver.FadeBoneToKeyFrame(kfReader);
        }
    }

    public void ResetPose() {
        foreach (var driver in nameDict.Values) {
            driver.ReturnOwnership();
        }
    }
}
