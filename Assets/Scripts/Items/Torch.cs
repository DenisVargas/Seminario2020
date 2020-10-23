using System.Collections;
using UnityEngine;
using Core.InventorySystem;
using Core.DamageSystem;

public class Torch : Item, IDamageable<Damage, HitResult>
{
    [SerializeField] GameObject _burningComponent = null;
    [SerializeField] Collider _interactionCollider = null;
    [SerializeField] bool _isOn;

    bool _toDestroy_FLAG = false;

    public bool isBurning
    {
        get => _isOn;
        set
        {
            _isOn = value;
            _burningComponent.SetActive(_isOn);
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

    //Colisiones. Si este objeto colisiona con un igniteable. Ejecuta su comando ignite, y luego se destruye.
    public Damage GetDamageStats()
    {
        return new Damage()
        {
            Ammount = 10f,
            type = DamageType.Fire
        };
    }
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

    public void FeedDamageResult(HitResult result) { }
    public void GetStun(Vector3 AgressorPosition, int PosibleKillingMethod) { }

    IEnumerator destroyAtEnd()
    {
        yield return new WaitForEndOfFrame();
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isBurning) return;

        var igniteable = other.GetComponentInChildren<IgnitableObject>();
        if (igniteable != null)
        {
            //print("Colisioné con un igniteable.");
            igniteable.ExecuteOperation(Core.Interaction.OperationType.Ignite);

            _toDestroy_FLAG = true;
            StartCoroutine(destroyAtEnd());
        }
    }
}
