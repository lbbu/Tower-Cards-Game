using UnityEngine;

public class PlayerJumpState : PlayerBaseState
{
    private bool _hasDoubleJumped;
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    public PlayerJumpState(PlayerStateMachine cuurentContext) : base(cuurentContext)
    {
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
                manager.animator.Play("Jump", -1, 0f);
                manager.animator.SetBool("FreeFall", false);
                _fallTimeoutDelta = manager.Data.DoubleFallTimeout;
            }

            // <-- إفراغ الإدخال دائماً بمجرد قراءته لمنع تخزينه
            manager.inputs.jump = false;
        }

        manager.VerticalVelocity += manager.Data.Gravity * Time.deltaTime;

        Vector3 cameraForword = manager.mainCamera.forward;
        Vector3 cameraRight = manager.mainCamera.right;

        cameraForword.y = 0f;
        cameraRight.y = 0f;
        cameraForword.Normalize();
        cameraRight.Normalize();

        Vector3 moveDirection = (cameraForword * manager.inputs.move.y + cameraRight * manager.inputs.move.x).normalized;
        Vector3 moveVelocity = moveDirection * manager.Data.WalkSpeed;
        moveVelocity.y = manager.VerticalVelocity;

        manager.controller.Move(moveVelocity * Time.deltaTime);

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