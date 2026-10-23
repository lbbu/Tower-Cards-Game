using System;
using UnityEditor;
using UnityEngine;

public class PlayerCrouchState : PlayerBaseState
{
    private float orginalHeight;
    private float orgianlCenter;

    public PlayerCrouchState(PlayerStateMachine cuurentContext) : base(cuurentContext)
    {
    }

    public override void EnterState()
    {
       orginalHeight = manager.controller.height;
       orgianlCenter = manager.controller.center.y;
       manager.animator.SetBool("Crouch",true);

       manager.controller.height = 1f;
       manager.controller.center = new Vector3(0f,0.5f,0f);


    }

    public override void ExitState()
    {
        manager.animator.SetBool("Crouch",false);

        manager.controller.height = orginalHeight;
        manager.controller.center = new Vector3(0f,orgianlCenter,0f);
    }

    public override void FixedUpdateState()
    {
    }

    public override void UpdateState()
    {
        Vector3 rayStart = manager.controller.transform.position+(Vector3.up*1.1f);
        bool isCelingAbovePlayer = Physics.Raycast(rayStart,Vector3.up,0.5f);
         // checking to change state
         if (!manager.controller.isGrounded)
        {
            manager.SwitchState(manager.fallState);
            return;
        }
        if(!manager.inputs.crouch && !isCelingAbovePlayer)
        {
            if (manager.inputs.move == Vector2.zero)
            {
                // when player isn't moving change to idel state
                manager.SwitchState(manager.idelState);
                return;
            }else
            {
                manager.SwitchState(manager.walkState);
                return;
            }
        }
        // if (manager.inputs.sprint)
        // {
        //    manager.Data.crouchSpeed = 1.6f;
        // }

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
        manager.cuurentSpeed = Mathf.Lerp(manager.cuurentSpeed,manager.Data.crouchSpeed,Time.deltaTime*manager.Data.SpeedChangeRate);

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