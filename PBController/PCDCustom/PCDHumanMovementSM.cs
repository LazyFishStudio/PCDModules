#if false

using System.Collections;
using System.Collections.Generic;
using PhysicsBasedCharacterController;
using UnityEngine;

[RequireComponent(typeof(CharacterManager))]
[RequireComponent(typeof(InputReader))]
public class PCDHumanMovementSM : MonoBehaviour {
    public enum State {
        Idle, Walk, Run,
    }
    public float walkSpeed = 6.0f;
    public float runSpeed = 7.0f;
    public MoveInput moveInput;
    [SerializeField]
    private string walkPoseLayerName = "Walk";
    [SerializeField]
    private string runPoseLayerName = "Run";
    [SerializeField]
    private StateMachine<State> sm;
    private CharacterManager charaM;
    private InputReader inputReader;
    // private PCDHuman[] humans;
    private PCDWalkMgr[] walkMgrs;

    void Awake() {
        charaM = GetComponent<CharacterManager>();
        inputReader = GetComponent<InputReader>();
        // humans = GetComponentsInChildren<PCDHuman>();
        walkMgrs = GetComponentsInChildren<PCDWalkMgr>();

        // InitMoveSM();
    }

    void Start() {
        InitMoveSM();
    }
    
    void InitMoveSM() {

        sm = new StateMachine<State>(State.Idle);

        sm.GetState(State.Idle).Bind(
            () => {
                // foreach (PCDHuman human in humans)
                foreach (PCDWalkMgr human in walkMgrs)
                    human.SetAnim(walkPoseLayerName);
            },
            () => {
                SetControllerInput_Idle();
            },
            () => {}
        );
        
        sm.GetState(State.Walk).Bind(
            () => {
                charaM.movementSpeed = walkSpeed;
                // foreach (PCDHuman human in humans)
                foreach (PCDWalkMgr human in walkMgrs)
                    human.SetAnim(walkPoseLayerName);
            },
            () => {
                SetControllerInput_Move();
            },
            () => {}
        );

        sm.GetState(State.Run).Bind(
            () => {
                // foreach (PCDHuman human in humans)
                foreach (PCDWalkMgr human in walkMgrs)
                    human.SetAnim(runPoseLayerName);
                charaM.sprintSpeed = runSpeed;
                inputReader.sprint = true;
            },
            () => {
                SetControllerInput_Move();
            },
            () => {
                inputReader.sprint = false;
            }
        );

        sm.Init();

    }

    public void Update() {
        HandleMovementInput();
        sm.UpdateStateAction();
    }

    void LateUpdate() {
        // moveInput.Init();
    }

    [System.Serializable]
    public class MoveInput {
        public Vector3 moveAxis;
        public bool run;
        public bool haveMoveInput => moveAxis.magnitude > 0.001f;
        
        public void Init() {
            moveAxis = Vector2.zero;
            run = false;
        }
    }

    private void HandleMovementInput() {

        if (!moveInput.haveMoveInput) {
            sm.GotoState(State.Idle);
            return;
        }

        if (moveInput.run) {
            sm.GotoState(State.Run);
            return;
        }
        
        sm.GotoState(State.Walk);

    }

    private void SetControllerInput_Idle() {
        inputReader.axisInput = Vector2.zero;
    }

    private void SetControllerInput_Move() {
        inputReader.axisInput = moveInput.moveAxis;
    }

}

#endif