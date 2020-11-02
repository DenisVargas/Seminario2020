using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.DamageSystem;
using IA.PathFinding;

struct transformState
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
}

public class Destroyable : MonoBehaviour, IDamageable<Damage, HitResult>
{
    public Action<GameObject> onDestroy = delegate { };

    [Header("Destroyable Parts")]
    [SerializeField] protected GameObject _normalObject     = null;
    [SerializeField] protected GameObject _destroyedObject  = null;
    [SerializeField] protected Collider _mainCollider       = null;
    [SerializeField] protected float _timeToDestroy         = 4f;

    [SerializeField] Transform[] _destroyedParts = new Transform[0];
    transformState[] _originalState = new transformState[0];

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

        //Esto probablemente se mas fácil reemplazarlo por un instantiate.
        //Seria un trade de procesamiento vs memoria.
        SetDestroyedPartsOriginalStates();
    }

    private void SetDestroyedPartsOriginalStates()
    {
        if (_destroyedParts.Length > 0)
        {
            _originalState = new transformState[_destroyedParts.Length];
            for (int i = 0; i < _destroyedParts.Length; i++)
            {
                var part = _destroyedParts[i];
                transformState state = new transformState();
                state.position = part.position;
                state.rotation = part.rotation;
                state.scale = part.localScale;

                _originalState[i] = state;
            }
        }
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

    protected void ReplaceToDestroyedMesh()
    {
        if (_destroyedObject)
            _destroyedObject.SetActive(true);
        if (_normalObject)
            _normalObject.SetActive(false);
        if (_mainCollider)
            _mainCollider.enabled = false;
        onDestroy(gameObject);
        onDestroy = delegate { };
    }

    protected IEnumerator delayedDestroy(float timeToDestroy)
    {
        yield return new WaitForSeconds(timeToDestroy);
        Destroy(gameObject);
    }
}
