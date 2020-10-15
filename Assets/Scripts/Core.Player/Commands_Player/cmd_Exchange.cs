using System;
using Core.InventorySystem;
using UnityEngine;
using IA.PathFinding;

public class cmd_Exchange : BaseQueryCommand
{
    Item target;
    Inventory _inventory;
    Func<bool, object[], Item> releaseEquipedItemFromHand = delegate { return null; };
    Action<Item> attachItemToHand = delegate { };

    Action<int, bool> setAnimation = delegate { };
    Func<int, bool> getAnimation = delegate { return true; };

    public cmd_Exchange(Item target, Inventory inventory, Func<bool, object[], Item> releaseEquipedItemFromHand, Action<Item> attachItemToHand, Action<int, bool> setAnimation, Func<int, bool> getAnimation, Transform body, PathFindSolver solver, Func<Node, bool> moveFunction, Action dispose, Action OnChangePath)
        :base(body, solver, moveFunction, dispose, OnChangePath)
    {
        this.target = target;
        _inventory = inventory;
        this.releaseEquipedItemFromHand = releaseEquipedItemFromHand;
        this.attachItemToHand = attachItemToHand;
        this.setAnimation = setAnimation;
        this.getAnimation = getAnimation;
    }

    public override void SetUp()
    {
        _ObjectiveNode = target.getInteractionParameters(_body.position).safeInteractionNode;
        base.SetUp();

        isReady = true;
    }
    public override void UpdateCommand()
    {
        //Lógica de movmiento pre-ejecución.
        if (completed) return;

        if (!isInRange(_ObjectiveNode))
        {
            if (!getAnimation(0))
                setAnimation(0, true);

            if (moveFunction(_nextNode) && _solver.currentPath.Count > 0)
                _nextNode = _solver.currentPath.Dequeue();
        }
        else
        {
            setAnimation(0, false);
            setAnimation(1, true);
        }
    }
    public override void Execute()
    {
        lookTowards(target);
        //Acá lo que hacemos es liberar el objeto equipado.
        releaseEquipedItemFromHand(true, new object[1] { target.transform.position });
        //Hacer el proceso contrario con el segundo item.
        attachItemToHand(target);
        completed = true;
    }
    public override void Cancel() { }
}
