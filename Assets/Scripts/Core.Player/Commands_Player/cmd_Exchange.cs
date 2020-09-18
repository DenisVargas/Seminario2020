using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.InventorySystem;
using UnityEngine;

public class cmd_Exchange : IQueryComand
{
    private Item target;
    private Inventory _inventory;
    private Func<bool, object[], Item> releaseEquipedItemFromHand = delegate { return null; };
    private Action<Item> attachItemToHand = delegate { };
    private Action triggerAnimation = delegate { };

    public bool completed { get; private set; } = false;
    public bool isReady { get; private set; } = false;
    public bool cashed => true;

    public cmd_Exchange(Item target, Inventory inventory, Func<bool, object[], Item> releaseEquipedItemFromHand, Action<Item> attachItemToHand, Action triggerAnimation)
    {
        this.target = target;
        _inventory = inventory;
        this.releaseEquipedItemFromHand = releaseEquipedItemFromHand;
        this.attachItemToHand = attachItemToHand;
        this.triggerAnimation = triggerAnimation;
    }

    public void SetUp()
    {
        triggerAnimation();
        isReady = true;
    }
    public void Cancel() { }

    public void Execute()
    {
        //Acá lo que hacemos es liberar el objeto equipado.
        releaseEquipedItemFromHand(true, new object[1] { target.transform.position });
        //Hacer el proceso contrario con el segundo item.
        attachItemToHand(target);
        completed = true;
    }
}
