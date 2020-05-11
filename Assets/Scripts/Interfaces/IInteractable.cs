using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum OperationOptions : int
{
    Take = 0,
    Ignite,
    Activate,
    Equip,
    TrowRock
}

public interface IInteractable
{
    Vector3 position { get; }
    Vector3 LookToDirection { get; }

    List<OperationOptions> GetSuportedOperations();
    void Operate(OperationOptions selectedOperation, params object[] optionalParams);
}
