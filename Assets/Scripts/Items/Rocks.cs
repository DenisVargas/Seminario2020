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
    [SerializeField] Renderer _renderer = null;
    [SerializeField] Material _stainedMaterial = null;
    [SerializeField] Material _defaultMaterial = null;

    [Header("Burning State Components")]
    [SerializeField] ParticleSystem _burningParticle = null;
    [SerializeField] ParticleSystem _ignitionParticle = null;
    [SerializeField] ParticleSystem _destroyParticle = null;

    [Header("State Labels")]
    [SerializeField] bool _isFlying = false;
    [SerializeField] bool _isStained = false;
    [SerializeField] bool _isBurning = false;
    bool _exploded = false;

    public bool IsAlive => true;
    public bool IsStained
    {
        get => _isStained;
        set
        {
            _isStained = value;
            if (_isStained)
                _renderer.material = _stainedMaterial;
            else
                _renderer.material = _defaultMaterial;
        }
    }
    public bool IsBurning
    {
        get => _isBurning;
        set
        {
            _isBurning = value;
            if (_isBurning)
            {
                _burningParticle.gameObject.SetActive(true);
                _burningParticle.Play();
                _ignitionParticle.gameObject.SetActive(true);
                _ignitionParticle.Play();
            }
            else
            {
                _burningParticle.gameObject.SetActive(false);
                _burningParticle.Stop();
                _ignitionParticle.gameObject.SetActive(false);
                _ignitionParticle.Stop();
            }
        }
    }

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

    private void OnTriggerEnter(Collider other)
    {
#if UNITY_EDITOR
        if (debugThisUnit)
            print($"{gameObject.name} colisionó con {other.gameObject.name}");
#endif

        if (_isFlying)
        {
            if (other == _owner) return;

            var damagecomponent = other.gameObject.GetComponent<IDamageable<Damage, HitResult>>();
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
                IsStained = true;
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
        if (_isBurning)
            return _burningDamage;

        return _normalDamage;
    }
    public HitResult GetHit(Damage damage)
    {
        HitResult result = new HitResult(true);
        if (!_exploded)
        {
            if (damage.type == DamageType.Fire && _isStained)
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
                Instantiate(_destroyParticle, transform.position + Vector3.up, Quaternion.identity);
                StartCoroutine(DelayedDestroy(2f));
                return result;
            }
        }
        else
        {
            result.exploded = true;
            result.fatalDamage = true;
            result.ignited = true;
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
