using UnityEngine;

public class PlayerNormalState : PlayerState
{
    public void EnterState(PlayerMain player)
    {
        Debug.Log("start walk");
    }

    public void UpdateState(PlayerMain player)
    {
        player.HandleMovement();
        player.HandleRotate();
    }

    public void ExitState(PlayerMain player)
    {
    }
}