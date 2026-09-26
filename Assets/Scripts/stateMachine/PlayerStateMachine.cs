using UnityEngine;
using StarterAssets;
using UnityEngine.Scripting;
using Unity.VisualScripting;

//to make sure theses component are availble
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(StarterAssetsInputs))]
[RequireComponent(typeof(Animator))]

public class PlayerStateMachine : MonoBehaviour
{
    //refrence to component to get them
    [Header("component refernce")]
    public CharacterController controller;
    public StarterAssetsInputs inputs;
    public Animator animator;
    public Transform mainCamera;

    [Header("Camera Settings")]
    public GameObject CameraTarget; 
    
    // varibales for the rotation of camera:

    // Yaw is for horizntol rotation
    private float cameraYaw;
    // pitch is for vertical rotation
    private float cameraPitch;

    // player state
    [Header("State Varibales")]
    public PlayerBaseState CuurentState {get; private set;}

    public PlayerIdelState idelState{get; private set;}
    public PlayerWalkState walkState{get; private set;}
    public PlayerSprintState SprintState { get; private set; }
    public PlayerJumpState JumpState { get; private set; }

    // to get player data.
    [Header("Player Settings")]
    public PlayerData Data;

    // cuurent speed is for accerlation and deaccerlation.
    [HideInInspector] public float cuurentSpeed;

    // target speed is used to control the speed of transtion between animation from the start asset
    [HideInInspector] public float TargetSpeed;

    [HideInInspector] public float VerticalVelocity;
    private float animationBlend;

    // awake is used to initalized varibles.
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        inputs = GetComponent<StarterAssetsInputs>();
        animator = GetComponent<Animator>();
        mainCamera = Camera.main.transform;

        idelState =new PlayerIdelState(this);
        walkState = new PlayerWalkState(this);
        SprintState = new PlayerSprintState(this);
        JumpState = new PlayerJumpState(this);

    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // start function is used to change the state to idel state from the start.
    void Start()
    {
        SwitchState(idelState);
    }

    // Update is called once per frame

    void Update()
    {
        // if the cuurent state isn't null update the state
        CuurentState?.UpdateState();

        // to make the animation smoth instead off the snaping animation
        animationBlend = Mathf.Lerp(animationBlend, TargetSpeed, Time.deltaTime * 10f);
        animator.SetFloat("Speed", animationBlend);
    }

    void FixedUpdate()
    {
        CuurentState?.FixedUpdateState();
    }

    private void LateUpdate()
    {
        // If the player is moving the mouse or right stick
        if (inputs.look.sqrMagnitude >= 0.01f)
        {
            cameraYaw += inputs.look.x * Data.CameraSensitivity;
            cameraPitch += inputs.look.y * Data.CameraSensitivity;
        }

        // Clamp the vertical pitch so the camera cannot flip upside down
        cameraPitch = Mathf.Clamp(cameraPitch, -30f, 70f);

        // Rotate the camera root object
        if (CameraTarget != null)
        {
            CameraTarget.transform.rotation = Quaternion.Euler(cameraPitch, cameraYaw, 0.0f);
        }
    }

    private void OnFootstep(AnimationEvent animationEvent){}

    private void OnLand(AnimationEvent animationEvent){}

    // switch state functionalty is to transition between state safely
    public void SwitchState(PlayerBaseState newState)
    {
        //doing the exit state function first before switching 
        CuurentState?.ExitState();

        // getting the new state and invoke the enter state function for that state.
        CuurentState = newState;
        CuurentState.EnterState();
    }
}
