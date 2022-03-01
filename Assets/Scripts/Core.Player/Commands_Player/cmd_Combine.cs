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
        Item equiped = _inventory.UnEquipItem();

        var recipe = ItemDataBase.getRecipe(equiped.ID, target.ID);
        if (recipe.Result != ItemID.nulo)
        {
            if (recipe.combinationMethod == CombinationMethod.replace)
            {
                GameObject.Destroy(equiped.gameObject);
                if(target.destroyAfterPick)
                    GameObject.Destroy(target.gameObject);

                var resultItem = GameObject.Instantiate(ItemDataBase.getRandomItemPrefab(recipe.Result))
                                           .GetComponent<Item>()
                                           .SetData(ItemDataBase.getItemData(recipe.Result));

                AttachToArm(resultItem);
            }

            if (recipe.combinationMethod == CombinationMethod.changeSourceState)
            {
                equiped.ExecuteOperation(Core.Interaction.OperationType.Combine);
                _inventory.EquipItem(equiped);
            }
        }

        completed = true;
    }
    public override void Cancel() { }
}
