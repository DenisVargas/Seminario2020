using System;
using UnityEngine;
using Core.Interaction;
using Core.InventorySystem;
using IA.PathFinding;

public class cmd_ThrowEquipment : IQueryComand
{
    Action setAnimation = delegate { };
    Func<bool, object[], Item> ReleaseEquipment = delegate { return null; };

    Node targetNode = null; //Objetivo del tiro.
    float time;
    Transform launchOrigin;
    TrowManagement tr;

    public bool completed { get; protected set; } = false;
    public bool isReady { get; protected set; } = false;
    public bool needsPremovement { get; protected set; } = false;
    public bool cashed { get; protected set; } = false;

    public cmd_ThrowEquipment( Transform launchOrigin, Node targetNode, float time, TrowManagement tr,Func<bool,object[], Item> ReleaseEquipment, Action setAnimation)
    {
        this.launchOrigin = launchOrigin;
        this.targetNode = targetNode;
        this.time = time;
        this.tr = tr;
        this.ReleaseEquipment = ReleaseEquipment;
        this.setAnimation = setAnimation;
    }

    public void SetUp()
    {
        setAnimation();
        isReady = true;
    }
    public void UpdateCommand() { }
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
    public void Cancel() { }
}
