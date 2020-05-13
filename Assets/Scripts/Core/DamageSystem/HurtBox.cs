using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.DamageSystem
{
    public interface IDamageable<Input>
    {
        GameObject gameObject { get; }
        void Hit(Input damage);
    }

    //Esto es un componente que actua como Hurtbox
    //Hurtboxes solo colisionan con Hitboxes, y son Triggers.
    //Aqui podremos setear debilidades.

    [RequireComponent(typeof(Collider))]
    public class HurtBox : MonoBehaviour
    {
        [SerializeField] IDamageable<Damage> _body;
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
            _body = GetComponentInParent<IDamageable<Damage>>();
        }

        public void TransferDamage(Damage damage)
        {
            Debug.LogWarning(string.Format("{0} ha recibido un HIT", _body.gameObject.name));
            _body.Hit(damage);
            DetectIncomingDamage = false;
        }
    }
}

