using UnityEngine;


public class PlayerJumpState : PlayerBaseState
{
    public PlayerJumpState(PlayerStateMachine cuurentContext) : base(cuurentContext)
    {
    }

    // setting animation paramtor
    public override void EnterState()
    {
        // triger the jump animation
        manager.animator.SetBool("Jump",true);
        manager.animator.SetBool("Grounded",false);

        manager.VerticalVelocity = Mathf.Sqrt(manager.Data.JumpHeight*-2f*manager.Data.Gravity);
        
        manager.inputs.jump = false;
    }

    public override void ExitState()
    {
        // to turn off the jump animation
        manager.animator.SetBool("Jump",false);
    }

    public override void FixedUpdateState()
    {
        

    }

    public override void UpdateState()
    {
        // checking weather the player is on the ground and is faling
        if (manager.controller.isGrounded && manager.VerticalVelocity < 0f)
        {
            
            if (manager.inputs.move == Vector2.zero)
            {
                // if the it not moving after the jump, switch to idel state
                manager.SwitchState(manager.idelState);
                return;
            }else
            {
                // else switch to walking state
                manager.SwitchState(manager.walkState);
                return;
            }
        }

            // to apply gravity over time
            manager.VerticalVelocity += manager.Data.Gravity*Time.deltaTime;

            

            Vector3 cameraForword = manager.mainCamera.forward;
            Vector3 cameraRight = manager.mainCamera.right;

            cameraForword.y = 0f;
            cameraRight.y = 0f;
            cameraForword.Normalize();
            cameraRight.Normalize();

            Vector3 moveDirection = (cameraForword*manager.inputs.move.y + cameraRight*manager.inputs.move.x).normalized;

            Vector3 moveVelocity = moveDirection * manager.Data.WalkSpeed; 
            moveVelocity.y = manager.VerticalVelocity;

            manager.controller.Move(moveVelocity * Time.deltaTime);

            if (manager.VerticalVelocity < 0.0f)
        {
            manager.animator.SetBool("Jump", false);
            manager.animator.SetBool("FreeFall", true);
        }
    }
}
