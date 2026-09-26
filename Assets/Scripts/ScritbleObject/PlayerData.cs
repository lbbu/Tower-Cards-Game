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

    [Header("Camera setting")]
    public float CameraSensitivity = 1f;
}
