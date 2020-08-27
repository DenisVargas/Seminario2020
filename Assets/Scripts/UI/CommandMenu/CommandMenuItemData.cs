using System;
using UnityEngine;
using Core.Interaction;

[Serializable, CreateAssetMenu( fileName = "Command Menu Item", menuName = "Command Menu/Command Preset", order = 1)]
public class CommandMenuItemData : ScriptableObject
{
    public OperationType Operation;
    public Sprite Icon;
    public string CommandName;
    public string CommandDescription;
}
