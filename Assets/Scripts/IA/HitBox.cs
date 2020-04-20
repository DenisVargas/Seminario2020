using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace core.DamageSystem
{
    public interface IAgressor<Input>
    {
        GameObject gameObject { get; }
        Input getDamageState();
    }

    [RequireComponent(typeof(Collider))]
    public class HitBox : MonoBehaviour
    {
        [Tooltip("El intervalo de tiempo entre la cual el daño es aplicado a los hurtboxes")]
        [SerializeField] float _detectionInterval = 1f;

        List<IDamageable<Damage>> _targets = new List<IDamageable<Damage>>();

        public bool active
        {
            get => _col.enabled;
            set => _col.enabled = value;
        }

        IAgressor<Damage> _body;
        [SerializeField] Collider _col;
        float _detectionTime;

        private void Awake()
        {
            _body = GetComponentInParent<IAgressor<Damage>>();
            _col = GetComponent<Collider>();
            _col.isTrigger = true;
        }

        private void Update()
        {
            if (_detectionTime > 0)
                _detectionTime -= Time.deltaTime;
            else
            {
                foreach (var target in _targets)
                {
                    //Aquí podriamos hacer que el hit retorne información!
                    target.Hit(_body.getDamageState());
                    _detectionTime = _detectionInterval;
                }
            }
        }

        void CheckCollision(Collider other)
        {
            var hiteable = other.gameObject.GetComponentInParent<IDamageable<Damage>>();

            if (hiteable != null && hiteable.gameObject == _body.gameObject)
            {
                print(string.Format("Me he detectado a mi mismo, descarto."));
                return;
            }

            if (hiteable != null && !_targets.Contains(hiteable))
            {
                print(string.Format("{0} Ha detectado un objetivo: {1}", _body.gameObject.name, hiteable.gameObject.name));
                _targets.Add(hiteable);
            }
        }


        private void OnTriggerEnter(Collider other)
        {
            CheckCollision(other);
        }

        //private void OnTriggerStay(Collider other)
        //{
        //    CheckCollision(other);
        //}

        private void OnTriggerExit(Collider other)
        {
            var hiteable = other.GetComponent<IDamageable<Damage>>();
            if (hiteable != null && _targets.Contains(hiteable))
                _targets.Add(hiteable);
        }
    }
}

