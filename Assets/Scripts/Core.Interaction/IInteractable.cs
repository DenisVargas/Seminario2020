using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum OperationType : int
{
    Take = 0,
    Ignite,
    Activate,
    Equip,
    TrowRock
}

public struct InteractionParameters
{
    //Si las operaciones se muestran indefinidamente o no.
    public bool LimitedDisplay;
    //El tiempo en el que se expone las operaciones.
    public float ActiveTime;
    //Los tipos de acciones que son soportadas.
    public List<OperationType> SuportedOperations;
}

public interface IInteractor
{
    Vector3 position { get; }
}

public interface IInteractable
{
    bool IsCurrentlyInteractable { get; }
    int InteractionsAmmount { get; }

    Transform transform { get; }
    Vector3 LookToDirection { get; }

    Vector3 requestSafeInteractionPosition(IInteractor requester);
    InteractionParameters GetSuportedInteractionParameters();

    void OnConfirmInput(OperationType selectedOperation, params object[] optionalParams);
    void OnOperate(OperationType selectedOperation, params object[] optionalParams);
    void OnCancelOperation(OperationType operation, params object[] optionalParams);
}
