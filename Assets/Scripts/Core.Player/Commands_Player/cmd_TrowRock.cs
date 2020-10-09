using System;
using UnityEngine;
using Core.Interaction;
using Core.InventorySystem;
using IA.PathFinding;

public class cmd_ThrowEquipment : IQueryComand
{
    Action TriggerAnimation = delegate { };
    Func<bool, object[], Item> ReleaseEquipment = delegate { return null; };

    IInteractionComponent CommandTarget = null;
    Node targetNode = null;
    float time;
    Transform launchOrigin;
    TrowManagement tr;

    public bool completed { get; private set; } = false;
    public bool isReady { get; private set; } = false;
    public bool cashed => true;

    public cmd_ThrowEquipment(float time, Transform launchOrigin, Node targetNode, TrowManagement tr,Func<bool,object[], Item> ReleaseEquipment, Action TriggerAnimation)
    {
        this.CommandTarget = null;
        this.time = time;
        this.launchOrigin = launchOrigin;
        this.targetNode = targetNode;
        this.tr = tr;
        this.TriggerAnimation = TriggerAnimation;
        this.ReleaseEquipment = ReleaseEquipment;
    }

    public void SetUp()
    {
        TriggerAnimation();
        isReady = true;
    }
    public void Execute()
    {
        Vector3 origin = launchOrigin.position;
        Item released = ReleaseEquipment(true, new object[0]);//Lo desatacheo.
        released.ExecuteOperation(OperationType.Throw);
        //Utilizando trowManager le añado una fuerza.
        var rb = released.gameObject.GetComponent<Rigidbody>();
        if (rb != null)
            tr.ThrowObject(rb, origin, targetNode.transform.position, time);
        completed = true;
    }
    public void Cancel()
    {
        CommandTarget.CancelOperation(OperationType.Throw);
    }
}
