using System;
using Core.InventorySystem;
using UnityEngine;
using IA.PathFinding;

public class cmd_Combine : BaseQueryCommand
{
    Item target;
    Inventory _inventory;
    Action<Item> AttachToArm = delegate { };

    Action<int, bool> setAnimation = delegate { };
    Func<int, bool> getAnimation = delegate { return false; };

    public cmd_Combine(Item target, Inventory inventory, Action<Item> AttachToArm,Action<int, bool> setAnimation, Func<int, bool> getAnimation, Transform body, PathFindSolver solver, Func<Node, bool> moveFunction, Action dispose, Action OnChangePath)
        : base(body, solver, moveFunction, dispose, OnChangePath)
    {
        this.target = target;
        _inventory = inventory;
        this.AttachToArm = AttachToArm;
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
        if (completed) return;

        if (!isInRange(_ObjectiveNode))
        {
            //Se entiende que el índice 0 es walk, mientras que el índice 1 es PullLever.
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
        Item equiped = _inventory.UnEquipItem();//Desequipo el objeto equipado.
        //Tomamos item a y b, guardamos sus indexes.
        int AID = target.ID;
        int BID = equiped.ID;

        ItemData resultData = ItemDataBase.Combine(AID, BID);//Obtengo la data del objeto resultante.
        if (resultData)
        {
            //Destruyo a y b.
            GameObject.Destroy(equiped.gameObject);
            GameObject.Destroy(target.gameObject);
            //Instancio el prefab resultante.
            GameObject prefab = resultData.GetRandomInGamePrefab();//Obtengo un prefab spawneable.
            var newItem = GameObject.Instantiate(prefab);
            var itemSettings = newItem.GetComponent<Item>();
            itemSettings.SetData(resultData);
            //Lo equipo en el inventario.
            AttachToArm(itemSettings);
        }
        completed = true;
    }
    public override void Cancel() { }
}
