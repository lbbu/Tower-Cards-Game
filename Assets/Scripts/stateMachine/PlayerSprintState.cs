using UnityEngine;


public class PlayerSprintState : PlayerBaseState
{
    public PlayerSprintState(PlayerStateMachine cuurentContext) : base(cuurentContext)
    {
    }

    // setting animation paramtor
    public override void EnterState()
    {
        manager.TargetSpeed = manager.Data.SprintSpeed;
        manager.animator.SetFloat("MotionSpeed", 1f);
        manager.animator.SetBool("Grounded", true);
        manager.animator.SetBool("FreeFall", false);
    }  

    public override void ExitState()
    {
        
    }

    public override void FixedUpdateState()
    {
        
    }

    public override void UpdateState()
    {
        if (manager.inputs.jump){
            // when player input the jump key, switch to jump state
            manager.SwitchState(manager.JumpState);
            return;
        }else if(manager.inputs.move == Vector2.zero)
        {
            // when player stop moving switch to idel 
            manager.SwitchState(manager.idelState);
            return;
        } else if (!manager.inputs.sprint)
        {
            // when the player stop sprint and start walking, switch to walk state
            manager.SwitchState(manager.walkState);
            return;
        }

        // to know where the camera is looking
        Vector3 cameraForword = manager.mainCamera.forward;
        Vector3 cameraRight = manager.mainCamera.right;

        cameraForword.y = 0f;
        cameraRight.y = 0f;
        cameraForword.Normalize();
        cameraRight.Normalize();

        Vector3 sprintDirection = (cameraForword*manager.inputs.move.y + cameraRight*manager.inputs.move.x).normalized;

        // calucalte to apply speed smothly to the Sprint speed instead off snaping to it.
        manager.cuurentSpeed = Mathf.Lerp(manager.cuurentSpeed,manager.Data.SprintSpeed,Time.deltaTime*manager.Data.SpeedChangeRate);

        Vector3 sprintVelocity = sprintDirection*manager.cuurentSpeed;

        // applying gravity
        manager.VerticalVelocity = manager.Data.Gravity * Time.deltaTime;

        manager.controller.Move(sprintVelocity*Time.deltaTime);

        // to rotate the player model to were we look
        if (sprintDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(sprintDirection);
            manager.controller.transform.rotation = Quaternion.Slerp(manager.controller.transform.rotation,targetRotation,10f*Time.deltaTime);
        }
    }
}
