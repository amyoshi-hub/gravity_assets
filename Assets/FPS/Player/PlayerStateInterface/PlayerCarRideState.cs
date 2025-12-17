using UnityEngine;

public class PlayerRideCarState : PlayerState
{
    private CarRigid _targetCar;

    public PlayerRideCarState(CarRigid car)
    {
        _targetCar = car;
    }

    public void EnterState(PlayerMain player)
    {
        Debug.Log("車に搭乗：歩行物理をオフにします");
        player._characterController.enabled = false;
        player.transform.SetParent(_targetCar.transform);
        player.transform.localPosition = _targetCar.GetDriverSeatLocal(); ;
    }

    public void UpdateState(PlayerMain player)
    {
        Vector2 input = player.InputMove;
        _targetCar.Drive(input);
    }

    public void ExitState(PlayerMain player)
    {
        Debug.Log("車から降車：歩行物理を再開します");
        player.transform.SetParent(null);
        player._characterController.enabled = true;
    }
}