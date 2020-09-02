using IA.FSM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadState : State
{
    public Action<GameObject> OnDead = delegate { };
    public Action Reset = delegate { };

    [SerializeField] float _desapearEffectDelay = 4f;
    [SerializeField] float _timeToDesapear = 2f;

    Rigidbody _rb = null;

    public override void Begin()
    {
        base.Begin();

        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
        _rb.isKinematic = true;

        _anims.SetBool("Dead", true);

        OnDead(gameObject);
        StartCoroutine(FallAndDestroyGameObject());
    }

    public override void End()
    {
        _anims.SetBool("Dead", false);
        Reset();
    }

    IEnumerator FallAndDestroyGameObject()
    {
        yield return new WaitForSeconds(_desapearEffectDelay);
        float fallEffectTime = 0;
        _rb.isKinematic = true;

        while (fallEffectTime < _timeToDesapear)
        {
            transform.position += (Vector3.down * Time.deltaTime);
            yield return null;
            fallEffectTime += Time.deltaTime;
        }

        Destroy(gameObject);
    }
}
