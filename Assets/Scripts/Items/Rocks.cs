using System.Collections;
using UnityEngine;
using Core.DamageSystem;
using Core.InventorySystem;
using Core.Interaction;

[RequireComponent(typeof(InteractionHandler))]
public class Rocks : Item, IDamageable<Damage, HitResult>
{
    [Header("================ Rocks ========================")]
    [SerializeField] LayerMask Hiteables = ~0;
    [SerializeField] Damage _normalDamage = Damage.defaultDamage();
    [SerializeField] Damage _burningDamage = Damage.defaultDamage();

    [Header("Stained State Components")]
    [SerializeField] Material _stainedMaterial = null;
    [SerializeField] Renderer _renderer = null;

    [Header("Burning State Components")]
    [SerializeField] ParticleSystem _burningParticle = null;

    [Header("State Labels")]
    [SerializeField] bool _isFlying = false;
    public bool IsStained = false;
    public bool IsBurning = false;

    public bool IsAlive => true;

    //================================== Unity Funcs ================================================

    protected override void Awake()
    {
        base.Awake();

        //El owner de un item es ignorado cuando ocurre una colisión!
        OnSetOwner += (owner) => { _owner = owner; };
        //Al ejecutarse Throw en un item, el estado pasa a flying.
        OnThrowItem += () => { _isFlying = true; };
    }

    //================================ Member Funcs =================================================

    public void StainWithSlime()
    {
        IsStained = true;
        _renderer.material = _stainedMaterial;
    }

    //================================= Damage Dealing ==============================================

    private void OnCollisionEnter(Collision collision)
    {
#if UNITY_EDITOR
        if (debugThisUnit)
            print($"{gameObject.name} colisionó con {collision.gameObject.name}");
#endif

        if (_isFlying)
        {
            if (collision.collider == _owner) return;

            var damagecomponent = collision.gameObject.GetComponent<IDamageable<Damage, HitResult>>();
            if (damagecomponent != null)
            {
                Damage enemyCollisionDamage = new Damage() { type = DamageType.hit };
                GetHit(enemyCollisionDamage); //Recibo daño por choque.
                damagecomponent.GetHit(_normalDamage); //Causo daño por choque.
            } 
        }
    }

    //================================= Interaction Handling ========================================

    public override void ExecuteOperation(OperationType operation, params object[] optionalParams)
    {
        switch (operation)
        {
            case OperationType.Take:
                OnTake();
                break;

            case OperationType.Throw:
                OnThrow();
                break;

            case OperationType.inspect:
                InspectionMenu.main.DisplayText(new string[] { data.Description },
                                                () => { Debug.Log("Display Completado. "); });
                break;

            case OperationType.Combine:
                StainWithSlime();
                break;
            case OperationType.Exchange:
                break;

            case OperationType.Drop:
                break;

            default:
                break;
        }
    }

    //================================= Damage Dealing ==============================================

    public Damage GetDamageStats()
    {
        if (IsBurning)
            return _burningDamage;

        return _normalDamage;
    }
    public HitResult GetHit(Damage damage)
    {
        HitResult result = new HitResult(true);

        if (damage.type == DamageType.Fire && IsStained)
        {
            result.ignited = true;
            IsBurning = true;
        }

        if (damage.type == DamageType.explotion || damage.type == DamageType.hit)
        {
            _interactionCollider.enabled = false;
            result.exploded = true;
            result.fatalDamage = true;
        }

        if (result.fatalDamage)
        {
            StartCoroutine(DelayedDestroy(2f));
            return result;
        }

        return result;
    }
    public void FeedDamageResult(HitResult result) { }
    public void GetStun(Vector3 AgressorPosition, int PosibleKillingMethod){ }

    //=============================== Corrutines ====================================================

    IEnumerator DelayedDestroy(float delay)
    {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }
}
