using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class PlayerFallState : PlayerBaseState
    {
        public PlayerFallState(PlayerStateMachine cuurentContext) : base(cuurentContext)
    {
    }

    public override void EnterState()
    {
     manager.animator.SetBool("Grounded", false);
     manager.animator.SetBool("FreeFall", true);

     manager.VerticalVelocity = Mathf.Min(manager.VerticalVelocity,0f);
    }

    public override void ExitState()
    {
        manager.animator.SetBool("FreeFall", false);
    }

    public override void FixedUpdateState()
    {
    }

    public override void UpdateState()
    {
        if (manager.controller.isGrounded)
        {
            manager.VerticalVelocity = -2f;
            if (manager.inputs.move == Vector2.zero)
            {
                manager.SwitchState(manager.idelState);
                return;
            }else if (manager.inputs.sprint)
            {
                manager.SwitchState(manager.SprintState);
                return;
            }
            else
            {
                manager.SwitchState(manager.walkState);
                return;
            }
        }

        // gravity accumulates, capped at terminal velocity
        manager.VerticalVelocity += manager.Data.Gravity * Time.deltaTime;
        manager.VerticalVelocity = Mathf.Max(manager.VerticalVelocity, -53f);

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
        moveVelocity.y = manager.VerticalVelocity;

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