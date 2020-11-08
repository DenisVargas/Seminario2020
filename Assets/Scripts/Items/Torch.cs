using System.Collections;
using UnityEngine;
using Core.InventorySystem;
using Core.DamageSystem;

public class Torch : Item, IDamageable<Damage, HitResult>
{
    [Header("================ Torch =======================")]
    [SerializeField] GameObject _burningComponent = null;
    [SerializeField] Damage hitDamage = new Damage();
    [SerializeField] bool _isOn;

    bool _toDestroy_FLAG = false;

    public bool isBurning
    {
        get => _isOn;
        set
        {
            _isOn = value;
            _burningComponent.SetActive(_isOn);
            if (_isOn)
                hitDamage.type = DamageType.Fire;
            else
                hitDamage.type = DamageType.hit;
        }
    }
    public bool IsAlive => true;

    protected override void Awake()
    {
        base.Awake();

        if (_isOn == false)
            isBurning = false;
    }

    protected override void Use(params object[] optionalParams)
    {
        base.Use(optionalParams); //Printea que se ha utilizado el objeto.
        //Deberíamos castear optional params para determinar que acción queremos hacer con este objeto.
        //testemaos un bool para saber si queremos prender la antorcha.
        if (optionalParams.Length > 1)
            isBurning = (bool) optionalParams[0];
    }
    protected override void OnTake()
    {
        base.OnTake();
        if (_interactionCollider)
            _interactionCollider.enabled = false;
    }
    protected override void Drop(params object[] optionalParams)
    {
        base.Drop(optionalParams);
        if (_interactionCollider)
            _interactionCollider.enabled = true;
    }


    public Damage GetDamageStats()
    {
        return hitDamage;
    }
    /// <summary>
    /// Este Objeto recive daño.
    /// </summary>
    /// <param name="damage">Estructura de Daño recibida.</param>
    /// <returns>Una estructura con información respecto al resultado de daño recibido.</returns>
    public HitResult GetHit(Damage damage)
    {
        HitResult result = new HitResult()
        {
            conected = true,
            fatalDamage = true
        };

        if (_toDestroy_FLAG) return result;

        if (damage.type == DamageType.Fire)
        {
            _toDestroy_FLAG = true;
            StartCoroutine(destroyAtEnd());
        }

        return result;
    }

    public void FeedDamageResult(HitResult result)
    {
        //if (result.exploded)
        //    print($" LA WEA EXPLOTO");

        if (result.conected && (result.exploded || result.ignited))
        {
            _toDestroy_FLAG = true;
            StartCoroutine(destroyAtEnd());
        }
    }
    public void GetStun(Vector3 AgressorPosition, int PosibleKillingMethod) { }

    IEnumerator destroyAtEnd()
    {
        yield return new WaitForEndOfFrame();
        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        var col = collision.collider;

        var damageable = col.GetComponent<IDamageable<Damage, HitResult>>();
        if (damageable != null)
        {
            //print($"{gameObject.name} Golpeó a un Damageable: {col.gameObject.name}");
            FeedDamageResult(damageable.GetHit(hitDamage));
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var igniteable = other.GetComponentInChildren<Slime>();
        if (igniteable != null && isBurning)
        {
            //print("Colisioné con un igniteable.");
            igniteable.ExecuteOperation(Core.Interaction.OperationType.Ignite);

            _toDestroy_FLAG = true;
            StartCoroutine(destroyAtEnd());
            return;
        }
    }
}
