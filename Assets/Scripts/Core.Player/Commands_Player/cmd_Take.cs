using Core.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Core.InventorySystem;

public class cmd_Take : IQueryComand
{
    Action TriggerAnimation = delegate { };
    Action<Item> equipItem = delegate { };

    Item target;

    public cmd_Take(Item target, Action<Item> equipItem, Action TriggerAnimation)
    {
        this.TriggerAnimation = TriggerAnimation;
        this.equipItem = equipItem;
        this.target = target;
    }
    public bool completed { get; private set; } = false;
    public bool isReady  { get; private set; } = false;
    public bool cashed  => true;

    public void SetUp()
    {
        TriggerAnimation();
        isReady = true;
    }
    public void Execute()
    {
        equipItem(target);
        completed = true;
    }
    public void Cancel() { }
}

