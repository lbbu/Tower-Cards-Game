using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Character Controller/PlayerData")]
/*
an Scriptable Object that hold Data used for the player movemnet, phyisc, and Camera.
*/
public class PlayerData : ScriptableObject
{
    [Header("Movemnet setting")]
    public float WalkSpeed = 2f;
    public float SprintSpeed = 5.335f;
    public float SpeedChangeRate = 10f;

    [Header("Phyics settings")]
    public float JumpHeight = 1.2f;
    public float Gravity = -15f;

    [Header("Double Jump Settings")]
    public bool CanDoubleJump = true;
    public float DoubleJumpHeight = 1.5f; // ارتفاع مستقل للقفزة الثانية لتشكيلها براحتك

    [Header("Timeout Settings")]
    [Tooltip("Cooldown after landing before the player can jump again (Prevents Bunny Hopping).")]
    public float JumpCooldown = 0.15f; // فترة التبريد بعد ملامسة الأرض

    [Tooltip("Time required to pass before double jump is allowed.")]
    public float DoubleJumpTimeout = 0.1f; // الوقت المطلوب قبل السماح بالقفزة الثانية في الهواء

    [Tooltip("Time required to pass before entering the fall state for the FIRST jump.")]
    public float FallTimeout = 0.15f;

    [Tooltip("Time required to pass before entering the fall state for the DOUBLE jump.")]
    public float DoubleFallTimeout = 0.15f; // المتغير الجديد الخاص بالنطة الثانية

    [Header("Camera setting")]
    public float CameraSensitivity = 1f;
}
