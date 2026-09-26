using UnityEngine;


// an abstract class used is the templet for the Player State.
public abstract class PlayerBaseState
{
    // the manager varibale is used to connect the player, player data, and player state together.
    protected PlayerStateMachine manager;

    // this constructor is used to make every state know the manager varibale.
    public PlayerBaseState(PlayerStateMachine playerContext)
    {
        manager = playerContext;
    }
    // when player Enter the new State
    public abstract void EnterState();
    // what to do when an every frame when the player it that state
    public abstract void UpdateState();
    // what to do an a fixed interval
    public abstract void FixedUpdateState();
    // what to do when player exit the state to another.
    public abstract void ExitState();
}
