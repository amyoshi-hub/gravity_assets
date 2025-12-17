using UnityEngine;

public interface PlayerState
{
    void EnterState(PlayerMain player);
    void UpdateState(PlayerMain player);
    void ExitState(PlayerMain player);
}