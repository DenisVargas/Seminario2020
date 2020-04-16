using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable, CreateAssetMenu( fileName = "Command Menu Item", menuName = "Command Menu/Command Preset", order = 1)]
public class CommandMenuItemData : ScriptableObject
{
    public OperationOptions Operation;
    public Sprite Icon;
    public string CommandName;
    public string CommandDescription;
}
