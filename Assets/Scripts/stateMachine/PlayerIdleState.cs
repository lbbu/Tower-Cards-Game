using UnityEditor.Timeline;
using UnityEngine;


public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(PlayerStateMachine cuurentContext) : base(cuurentContext)
    {
    }

    // setting animation paramtor
    public override void EnterState()
    {

        manager.TargetSpeed = 0f;
        manager.animator.SetFloat("MotionSpeed", 1f);
        manager.animator.SetBool("Grounded", true); // Tells the animatorator to stop falling
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
        // checking to weather update the state or not.
        // if the player was moving change to walk state
        if(manager.inputs.move != Vector2.zero)
        {
            manager.SwitchState(manager.walkState);
            return;
        } else if (manager.inputs.jump)
        {
            // if the player input for jump was true change to jump state
            manager.SwitchState(manager.JumpState);
            return;
        }

        // lerp here is used for deaccerlation of the speed to 0
        manager.cuurentSpeed = Mathf.Lerp(manager.cuurentSpeed,0f,Time.deltaTime * manager.Data.SpeedChangeRate);

        
        Vector3 moveVelocity = manager.controller.transform.forward * manager.cuurentSpeed;
        moveVelocity.y = -2f; // gravity

        manager.controller.Move(moveVelocity * Time.deltaTime);
    }
}
