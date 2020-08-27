using System;
using UnityEngine;

public interface IPlayerController
{
    bool IsAlive { get; }
    Vector3 position { get; }

    event Action ImDeadBro;

    void ClonSpawn();
    void finishClon();
    void ClonDeactivate();
    void FallInTrap();
    void GetStun(Vector3 AgressorPosition, int PosibleKillingMethod);
    void PlayBlood();
    void QuerySelectedOperation(OperationType operation, IInteractable target);
}