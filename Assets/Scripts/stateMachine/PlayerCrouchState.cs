using System;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

using Debug = UnityEngine.Debug;

public class PlayerCrouchState : PlayerBaseState
{
    private float orginalHeight;
    private Vector3 orgianlCenter;
    private bool lastCrouchInput;
    private const float crouchHeight = 1.0f;

    private static int obsticalMask => ~LayerMask.GetMask("Player");

    public PlayerCrouchState(PlayerStateMachine cuurentContext) : base(cuurentContext)
    {
    }

    public override void EnterState()
    {
       orginalHeight = manager.controller.height;
       orgianlCenter = manager.controller.center;
       manager.animator.SetBool("Crouch",true);

       manager.controller.height = crouchHeight;
       manager.controller.center = new Vector3(orgianlCenter.x,orgianlCenter.y - (orginalHeight-crouchHeight)/2f,orgianlCenter.z);


    }

    public override void ExitState()
    {
        manager.animator.SetBool("Crouch",false);

        manager.controller.height = orginalHeight;
        manager.controller.center = orgianlCenter;

        Debug.Log("[Crouch] ExitState");
        lastCrouchInput = false;
    }

    public override void FixedUpdateState()
    {
    }

    private bool CanStandUp()
    {
        var c = manager.controller;
        Bounds b = c.bounds;
        // top sphere of the crouched capsule, cast up by the height we'd gain
        Vector3 origin = new Vector3(b.center.x, b.max.y - c.radius, b.center.z);
        float distance = (orginalHeight - crouchHeight) + 0.05f;
        return !Physics.SphereCast(origin, c.radius * 0.9f, Vector3.up, out _, distance, obsticalMask, QueryTriggerInteraction.Ignore);
    }

    public override void UpdateState()
    {

        // top of UpdateState
        if (manager.inputs.crouch != lastCrouchInput)
        {
            lastCrouchInput = manager.inputs.crouch;
            Debug.Log($"[Crouch] input -> {lastCrouchInput}, canStand={CanStandUp()}, grounded={manager.controller.isGrounded}");
        }
         // checking to change state
         if (!manager.controller.isGrounded)
        {
            manager.SwitchState(manager.fallState);
            return;
        }
        if(!manager.inputs.crouch && CanStandUp())
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

        // face the camera direction (strafe style) so left/right/back clips make sense
        Quaternion facing = Quaternion.Euler(0f, manager.mainCamera.eulerAngles.y, 0f);
        manager.controller.transform.rotation =
            Quaternion.Slerp(manager.controller.transform.rotation, facing, 10f * Time.deltaTime);

        // feed the crouch blend tree
        manager.animator.SetFloat("MoveX", manager.inputs.move.x, 0.1f, Time.deltaTime);
        manager.animator.SetFloat("MoveY",manager.inputs.move.y, 0.1f, Time.deltaTime);

    }
    


}