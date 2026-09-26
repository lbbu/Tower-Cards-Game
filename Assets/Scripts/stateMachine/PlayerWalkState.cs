using Unity.Mathematics;
using UnityEngine;


public class PlayerWalkState : PlayerBaseState
{
    
    public PlayerWalkState(PlayerStateMachine cuurentContext) : base(cuurentContext)
    {
    }

    // setting animation paramtor
    public override void EnterState()
    {
        manager.TargetSpeed = manager.Data.WalkSpeed;
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
        // checking to change state
        if(manager.inputs.move == Vector2.zero)
        {
            // when player isn't moving change to idel state
            manager.SwitchState(manager.idelState);
            return;
        }
        else if (manager.inputs.sprint)
        {
            // when the player input for sprint is entered, change to sprint state
            manager.SwitchState(manager.SprintState);
            return;
        }
        else if (manager.inputs.jump)
        {
            // when the player input for jump is entered, change to jump state.
            manager.SwitchState(manager.JumpState);
            return;
        }
        
        // to know where the camera is looking
        // forward is for W/S inputs
        Vector3 cameraForword = manager.mainCamera.forward;
        // Right is for A/D inputs
        Vector3 cameraRight = manager.mainCamera.right;

        // Flatten the Y axis so we don't accidentally walk into the sky
        cameraForword.y = 0f;
        cameraRight.y = 0f;
        cameraRight.Normalize();
        cameraForword.Normalize();

        // Calculate the direction based on the player input
        Vector3 moveDirection = (cameraForword*manager.inputs.move.y + cameraRight*manager.inputs.move.x).normalized;



        // calucalte to apply speed smothly to the walking speed instead off snaping to it.
        manager.cuurentSpeed = Mathf.Lerp(manager.cuurentSpeed,manager.Data.WalkSpeed,Time.deltaTime*manager.Data.SpeedChangeRate);

        // Calculate the velocity to move the player
        Vector3 moveVelocity = moveDirection * manager.cuurentSpeed;
        moveVelocity.y = -2f; // applying gravity

        // move the player using controller character
        manager.controller.Move(moveVelocity * Time.deltaTime);

        // rotating the player model to face the direction we walk to
        if (moveDirection != Vector3.zero)
        {
            // calculate the angle that play look it 
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(moveDirection.x,0f,moveDirection.z));
            // rotate player model smothly using slerp
            manager.controller.transform.rotation = Quaternion.Slerp(manager.controller.transform.rotation,targetRotation,10f * Time.deltaTime);
        }
    }
}
