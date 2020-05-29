using UnityEngine;

namespace Core.DamageSystem
{
    //Esto es un componente que actua como Hurtbox
    //Hurtboxes solo colisionan con Hitboxes, y son Triggers.

    [RequireComponent(typeof(Collider))]
    public class HurtBox : MonoBehaviour
    {
        [SerializeField] IDamageable<Damage, HitResult> _body;
        public bool DetectIncomingDamage
        {
            get => _col.enabled;
            set => _col.enabled = value;
        }
        Collider    _col;

        private void Awake()
        {
            _col = GetComponent<Collider>();
            _col.isTrigger = true;
            _body = GetComponentInParent<IDamageable<Damage, HitResult>>();
        }

        public void TransferDamage(Damage damage)
        {
            //Debug.LogWarning(string.Format("{0} ha recibido un HIT", _body.gameObject.name));
            _body.GetHit(damage);
            DetectIncomingDamage = false;
        }
    }
}

