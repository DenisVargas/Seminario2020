using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum OperationOptions : int
{
    Take = 0,
    Ignite,
    Activate,
    Equip
}

public interface IInteractable
{
    Vector3 position { get; }

    List<OperationOptions> GetSuportedOperations();
    void Operate(OperationOptions selectedOperation, params object[] optionalParams);
}
