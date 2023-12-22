using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PCDHumanConfig))]
[RequireComponent(typeof(PCDAnimator))]
[RequireComponent(typeof(PCDWalkMgr))]
[RequireComponent(typeof(PCDPoseMgr))]
[RequireComponent(typeof(PCDArchoringMgr))]
public class PCDHumanMgr : MonoBehaviour
{
    public PCDSkeleton skeleton;
    public PCDHumanConfig humanConfig;
    public PCDAnimator animator;
    public PCDWalkMgr walkMgr;
    public PCDPoseMgr poseMgr;
    public PCDArchoringMgr arhchoringMgr;
    void Awake() {
        skeleton = GetComponent<PCDSkeleton>();
        humanConfig = GetComponent<PCDHumanConfig>();
        animator = GetComponent<PCDAnimator>();
        walkMgr = GetComponent<PCDWalkMgr>();
        poseMgr = GetComponent<PCDPoseMgr>();
        arhchoringMgr = GetComponent<PCDArchoringMgr>();
    }
}