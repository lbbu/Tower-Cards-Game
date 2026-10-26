using System;
using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
    private bool _hasDoubleJumped;
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    private Vector3 horizontalVelocity;
    private float takeOffSpeed;

    public PlayerJumpState(PlayerStateMachine cuurentContext) : base(cuurentContext)
    {
    }

    private Vector3 GetInputDirection()
    {
        Vector3 cameraForword = manager.mainCamera.forward;
        Vector3 cameraRight = manager.mainCamera.right;

        cameraForword.y = 0f;
        cameraRight.y = 0f;
        cameraForword.Normalize();
        cameraRight.Normalize();

        return (cameraForword * manager.inputs.move.y + cameraRight * manager.inputs.move.x).normalized;
    }
    public override void EnterState()
    {
        _hasDoubleJumped = false;

        // استخدام المؤقتات الخاصة بالقفزة الأولى
        _jumpTimeoutDelta = manager.Data.DoubleJumpTimeout;
        _fallTimeoutDelta = manager.Data.FallTimeout;

        manager.animator.SetBool("Jump", true);
        manager.animator.SetBool("Grounded", false);

        manager.VerticalVelocity = Mathf.Sqrt(manager.Data.JumpHeight * -2f * manager.Data.Gravity);
        manager.inputs.jump = false;

        takeOffSpeed = manager.cuurentSpeed;

        horizontalVelocity = GetInputDirection() * takeOffSpeed;
    }

    public override void ExitState()
    {
        manager.animator.SetBool("Jump", false);
        manager.animator.SetBool("FreeFall", false);
    }

    public override void FixedUpdateState()
    {
    }

    public override void UpdateState()
    {
        // 1. التحقق من الهبوط وتفعيل التبريد
        if (manager.controller.isGrounded && manager.VerticalVelocity < 0f)
        {
            manager.JumpCooldownTimer = manager.Data.JumpCooldown;
            manager.inputs.jump = false; // <-- تنظيف فوري لأي إدخال معلق عند الهبوط

            if (manager.inputs.move == Vector2.zero)
            {
                manager.SwitchState(manager.idelState);
                return;
            }
            else
            {
                manager.SwitchState(manager.walkState);
                return;
            }
        }

        if (_jumpTimeoutDelta >= 0.0f)
        {
            _jumpTimeoutDelta -= Time.deltaTime;
        }

        // 2. التحقق من القفزة المزدوجة وتفريغ الإدخال
        if (manager.inputs.jump)
        {
            if (manager.Data.CanDoubleJump && !_hasDoubleJumped && _jumpTimeoutDelta <= 0.0f)
            {
                manager.VerticalVelocity = Mathf.Sqrt(manager.Data.DoubleJumpHeight * -2f * manager.Data.Gravity);
                _hasDoubleJumped = true;
                manager.animator.Play("JumpStart", -1, 0f);
                manager.animator.SetBool("FreeFall", false);
                _fallTimeoutDelta = manager.Data.DoubleFallTimeout;
            }

            // <-- إفراغ الإدخال دائماً بمجرد قراءته لمنع تخزينه
            manager.inputs.jump = false;
        }

        manager.VerticalVelocity += manager.Data.Gravity * Time.deltaTime;

        // if (manager.VerticalVelocity < -53.0f)
        // {
        //     manager.VerticalVelocity = -53.0f;
        // }

        Vector3 inputDirection = GetInputDirection();

        float maxAirSpeed = Mathf.Max(manager.Data.WalkSpeed,takeOffSpeed);

        Vector3 target = inputDirection * maxAirSpeed;

        horizontalVelocity = Vector3.MoveTowards(horizontalVelocity,target,manager.Data.AirAcceleration * Time.deltaTime);

        Vector3 moveVelocity = horizontalVelocity;
        moveVelocity.y = manager.VerticalVelocity;


        manager.controller.Move(moveVelocity * Time.deltaTime);

        if (horizontalVelocity.sqrMagnitude > 0.1f)
        {
            Quaternion look = Quaternion.LookRotation(horizontalVelocity.normalized);
            manager.controller.transform.rotation = Quaternion.Slerp(manager.controller.transform.rotation, look, 10f * Time.deltaTime);
        }

        // 3. تطبيق مؤقت السقوط
        if (manager.VerticalVelocity < 0.0f)
        {
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                manager.animator.SetBool("Jump", false);
                manager.animator.SetBool("FreeFall", true);
            }
        }
    }


}