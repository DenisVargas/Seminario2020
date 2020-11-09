using System;
using UnityEngine;
using Core.DamageSystem;
using Core.Interaction;
using Core.InventorySystem;
using IA.PathFinding;
using System.Collections.Generic;

[RequireComponent(typeof(InteractionHandler))]
public class ExplosivePot : Item, IDamageable<Damage, HitResult>
{
    public Action<Collider> onDestroy = delegate { };

    [Header("============ Explosive Pot ===============")]
    public bool exploded = false;

    [Header("Settings")]
    [SerializeField] LayerMask explotionAffects = ~0;
    [SerializeField] Damage MyDamage = new Damage();
    [SerializeField] float _explotionForce = 10f;
    [SerializeField] float _explotionRadius = 4f;

    [Header("Components")]
    [SerializeField] ParticleSystem Explotion = null;
    [SerializeField] GameObject _normalObject = null;
    [SerializeField] GameObject _destroyedObject = null;
    [SerializeField] Collider _mainCollider = null;

    [SerializeField] Transform[] _destroyedParts = new Transform[0];
    [SerializeField] Node[] AffectedNodes = new Node[0];

    transformState[] _originalState = new transformState[0];

    //=============================== DEBBUGING =================================================

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.matrix = Matrix4x4.Scale(new Vector3(1, 0, 1));
        Gizmos.DrawWireSphere(transform.position, _explotionRadius);
    }
#endif

    //============================= Unity Functions =============================================

    protected override void Awake()
    {
        base.Awake();

        _normalObject.SetActive(true);
        if (_destroyedObject.activeSelf)
            _destroyedObject.SetActive(false);

        foreach (var node in AffectedNodes)
            node.ChangeNodeState(NavigationArea.blocked);

        //Esto probablemente se mas fácil reemplazarlo por un instantiate.
        //Seria un trade de procesamiento vs memoria.
        SetDestroyedPartsOriginalStates();
    }

    //============================== Member Functions ===========================================

    public void Explode()
    {
        //Exploto y destruyo cosas a mi alrededor.
        print("EXPLOTO A LA CHUCHA");
        exploded = true;
        var explotionParticle = Instantiate(Explotion, transform.position, Quaternion.identity);
        explotionParticle.gameObject.SetActive(true);
        explotionParticle.Play();

        //Creo un Damage con la explosión.
        var toApplyDamage = Damage.defaultDamage();
        toApplyDamage.Ammount = 100f;
        toApplyDamage.instaKill = true;
        toApplyDamage.KillAnimationType = 0;
        toApplyDamage.type = DamageType.explotion;
        toApplyDamage.explotionForce = _explotionForce;
        toApplyDamage.explotionOrigin = transform.position;

        //Acá tengo tengo que buscar todos los objetivos en un radio y transmitirles daño de explosión.
        var hits = Physics.OverlapSphere(transform.position, _explotionRadius, explotionAffects);
        if (hits.Length > 0)
        {
            foreach (var collider in hits)
            {
                if (collider == _mainCollider) continue;

                var explosivePot = collider.GetComponent<ExplosivePot>();
                if (explosivePot)
                {
                    if (!explosivePot.exploded)
                        explosivePot.GetHit(toApplyDamage);
                    continue;
                }

                //Chequeo si tiene un componente damageable.
                var damageable = collider.GetComponent<IDamageable<Damage, HitResult>>();

                if (damageable != null)
                    damageable.GetHit(toApplyDamage);
            }
        }

        IsAlive = false;
        Destroy(gameObject);
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
    protected void ReplaceToDestroyedMesh()
    {
        if (_destroyedObject)
            _destroyedObject.SetActive(true);
        if (_normalObject)
            _normalObject.SetActive(false);
        if (_mainCollider)
            _mainCollider.enabled = false;
        onDestroy(_mainCollider);
        onDestroy = delegate { };
    }

    //============================= Collision handling ==========================================

    private void OnCollisionEnter(Collision collision)
    {
        var col = collision.collider;
        var damagecomponent = col.GetComponent<IDamageable<Damage, HitResult>>();
        if (damagecomponent != null)
        {
            //print($"{gameObject.name} Golpeó a un Damageable: {col.gameObject.name}");
            damagecomponent.GetHit(GetDamageStats());
            GetHit(damagecomponent.GetDamageStats());
        }
    }

    //============================ Damage dealing ===============================================

    public bool IsAlive { get; protected set; } = true;

    public void FeedDamageResult(HitResult result) { }
    public Damage GetDamageStats()
    {
        return MyDamage;
    }
    public HitResult GetHit(Damage damage)
    {
        //Recivo daño de una fuente externa.
        var result = new HitResult(true);

#if UNITY_EDITOR
        if (debugThisUnit)
            print("Reciví Daño.");
#endif

        if (damage.type == DamageType.Fire || damage.type == DamageType.explotion)
        {
            result.exploded = true;
            Explode();
        }

        return result;
    }
    public void GetStun(Vector3 AgressorPosition, int PosibleKillingMethod) { }

    //============================ Interaction System ===========================================

    public override List<Tuple<OperationType, IInteractionComponent>> GetAllOperations(Inventory CurrentInventory = null, bool ignoreInventory = false)
    {
        //Operaciones dinamicas son aquellas que dependen del inventario actual.
        List<Tuple<OperationType, IInteractionComponent>> _myOperations = new List<Tuple<OperationType, IInteractionComponent>>();

        if (CurrentInventory != null)//Si el inventario está específicado.
        {
            if (CurrentInventory.equiped == null)
            {
                //Take es condicional de acuerdo al inventario del jugador.
                _myOperations.Add(new Tuple<OperationType, IInteractionComponent>(OperationType.Take, this));
            }
            else
            {
                _myOperations.Add(new Tuple<OperationType, IInteractionComponent>(OperationType.Exchange, this));

                // Es combinable? Añado la operación si el equipado tiene una combinación con este item.

                if (CurrentInventory.equiped.ID == ItemID.Piedra)
                {
                    var piedra = (Rocks)CurrentInventory.equiped;
                    if (!piedra.IsBurning && !piedra.IsStained)
                    {
                        var recipe = ItemDataBase.getRecipe(ID, CurrentInventory.equiped.ID);
                        if (recipe.combinationMethod == CombinationMethod.changeSourceState && recipe.Result == ItemID.PiedraBaba)
                            _myOperations.Add(new Tuple<OperationType, IInteractionComponent>(OperationType.Combine, this));
                    }
                }
            }
        }
        else //Si no se específica un inventario, se añaden todas las operaciones por defecto.
        {
            _myOperations.Add(new Tuple<OperationType, IInteractionComponent>(OperationType.Take, this));
        }

        //Es consumible?.
        if (data.isConsumable)
            _myOperations.Add(new Tuple<OperationType, IInteractionComponent>(OperationType.use, this));

        return _myOperations;
    }
}
