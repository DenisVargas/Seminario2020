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
    List<OperationOptions> GetSuportedOperations();
    void Operate(int opID, params object[] optionalParams);
}
