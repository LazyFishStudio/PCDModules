using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Bros.Utils;

public class FishShadow : MonoBehaviour
{
    static public List<FishShadow> allFishShadows = new List<FishShadow>();

    private void OnEnable() => allFishShadows.Add(this);
    private void OnDisable() => allFishShadows.Remove(this);

    public enum State { Idle, Find, Probe, Bite, Finish }
    public StateMachine<State> sm = new StateMachine<State>(State.Idle);
    public GameObject product;
    public float targetHoldTime = 2;
    public float swimSpeed = 1f;
    public float distToProbe = 1.25f;
    public float distToBite = 0.75f;
    public int probeTimes = 3;
    public float probeHalfDuration = 1f;
    public float finishDropDist = 2f;
    public float finishJumpHigh = 4f;
    public float finishDuration = 1f;

    public GameObject waterRipplePrefab;
    public GameObject waterSplashPrefab;
    public GameObject struggleWaterSplashPrefab;
    private GameObject struggleWaterSplashEffect;

    /* Runtime Status */
    public FishBobber fishBobber;
    private Vector3 probeStartPos;
    private Vector3 probeEndPos;
    private Quaternion biteRotation;

    private void Awake() {
        sm.GetState(State.Idle).Bind(onEnter: () => { fishBobber = null; });
        sm.GetState(State.Find).Bind(
            onEnter: () => { transform.DOLookAt(fishBobber.transform.position, 0.5f); },
            onUpdate: () => {
                Vector3 diff = (fishBobber.transform.position - transform.position).CopySetY(0f);
                float lookAngle = Mathf.Abs(Vector3.Angle(transform.forward.CopySetY(0f), diff));
                if (lookAngle > 10f)
                    return;

                float dist = diff.magnitude;
                if (dist <= distToProbe) {
                    sm.GotoState(State.Probe);
                } else {
                    Vector3 dir = diff.normalized.CopySetY(0f);
                    float moveDist = Mathf.Min(swimSpeed * Time.deltaTime, dist);
                    transform.position += dir * moveDist;
			    }
            },
            onExit: () => { transform.DOKill(); }
        );

        sm.GetState(State.Probe).Bind(
            onEnter: () => {
                Vector3 revDir = (transform.position - fishBobber.transform.position).normalized;
                probeStartPos = (fishBobber.transform.position + revDir * distToProbe).CopySetY(transform.position.y);
                probeEndPos = (fishBobber.transform.position + revDir * distToBite).CopySetY(transform.position.y);

                probedTimes = 0;
                TriggerProbeOnce();

                fishBobber.OnFishProbe();
            },
            onExit: () => { transform.DOKill(); }
        );

        sm.GetState(State.Bite).Bind(
            onEnter: () => {
                /* TODO: Play Animation and Effects */
                /*
                System.Action shakeAction = null;
                shakeAction = () => {
                    transform.DOShakeRotation(3f).OnComplete(() => { shakeAction?.Invoke(); });
                };
                */

                fishBobber.OnFishBite();

                SendMessage("PlayEvent", new string[2] {"FishingAction", "FishBite"}, SendMessageOptions.DontRequireReceiver);
                if (struggleWaterSplashPrefab) {
                    struggleWaterSplashEffect = GameObject.Instantiate(struggleWaterSplashPrefab, fishBobber.transform.position, Quaternion.identity);
                }

                biteRotation = transform.rotation;
                transform.DOShakeRotation(100f);

                fishBobber.fishRod.targetHoldTime = targetHoldTime;
                fishBobber.fishRod.curFishShadow = this;
            },
            onExit: () => { 
                transform.DOKill(); 
                Destroy(struggleWaterSplashEffect);
            }
        );

        sm.GetState(State.Finish).Bind(onEnter: () => {

            // SendMessage("PlayEvent", new string[2] {"FishingAction", "FishOutOfWater"}, SendMessageOptions.DontRequireReceiver);
            if (waterSplashPrefab) {
                GameObject.Instantiate(waterSplashPrefab, fishBobber.transform.position, Quaternion.identity);
            }

            Vector3 targetAngles = product.transform.rotation.eulerAngles.CopySetY(biteRotation.eulerAngles.y);
            Quaternion targetRot = Quaternion.Euler(targetAngles);
            GameObject fish = Instantiate(product, transform.position, targetRot, null);
            Vector3 playerPos = fishBobber.fishRod.PCDHuman.skeleton.humanBone.root.transform.position.CopyAddY(0.15f);
            Vector3 targetPos = playerPos + (playerPos - transform.position).CopySetY(playerPos.y).normalized * finishDropDist + Vector3.up * 0.5f;

            PCDJumpFish fishJump = fish.GetComponent<PCDJumpFish>();
            Collider fishCollider = fish.GetComponent<Collider>();
            Rigidbody fishRb = fish.GetComponent<Rigidbody>();
            fishJump.PauseJump();
            fishCollider.enabled = false;
            fishRb.interpolation = RigidbodyInterpolation.None;
            fishRb.useGravity = false;
            fish.GetComponent<Rigidbody>().DOJump(targetPos, finishJumpHigh, 1, finishDuration).SetEase(Ease.Linear).OnComplete(() => {
                fishRb.useGravity = true;
                fishRb.interpolation = RigidbodyInterpolation.Interpolate;
                fishCollider.enabled = true;
                fishJump.ContinueJump();
            });

            GameObject.Destroy(gameObject);
        });

        sm.Init();
    }

    private void Update() {
        sm.UpdateStateAction();
	}


    private int probedTimes = 0; 
    private void TriggerProbeOnce() {
        transform.DOJump(probeEndPos, 0f, 1, probeHalfDuration).OnComplete(() => {

            // SendMessage("PlayEvent", new string[2] {"FishingAction", "FishProbe"}, SendMessageOptions.DontRequireReceiver);
            if (waterRipplePrefab) {
                GameObject.Instantiate(waterRipplePrefab, fishBobber.transform.position, Quaternion.identity);
            }
            

            probedTimes++;
            if (probedTimes < probeTimes) {
                transform.DOJump(probeStartPos, 0f, 1, probeHalfDuration).OnComplete(TriggerProbeOnce);
            } else {
                sm.GotoState(State.Bite);
			}
        });
    }
}
