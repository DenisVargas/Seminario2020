using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.DamageSystem;
using IA.PathFinding;

public class destroyable : MonoBehaviour, IDamageable<Damage, HitResult>
{
    public Action<GameObject> onDestroy = delegate { };

    [Header("Destroyable Parts")]
    [SerializeField] protected GameObject _normalObject;
    [SerializeField] protected GameObject _destroyedObject;
    [Header("Optional Parameters")]
    [SerializeField] protected Damage MyDamage = new Damage();
    [SerializeField] protected Node[] AffectedNodes = new Node[0];

    protected virtual void Awake()
    {
        _normalObject.SetActive(true);
        if (_destroyedObject.activeSelf)
            _destroyedObject.SetActive(false);

        foreach (var node in AffectedNodes)
            node.ChangeNodeState(NavigationArea.blocked);
    }

    public bool IsAlive { get; protected set; } = true;

    public virtual void FeedDamageResult(HitResult result)
    {
        print("Succesfully made Damage");
    }
    public virtual Damage GetDamageStats()
    {
        return MyDamage;
    }
    public virtual HitResult GetHit(Damage damage)
    {
        throw new NotImplementedException();
    }
    public void GetStun(Vector3 AgressorPosition, int PosibleKillingMethod)
    {
        throw new NotImplementedException();
    }
}
