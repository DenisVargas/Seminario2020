using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
public class GroundTrigger : MonoBehaviour
{
    UnityEvent OnActivate;
    UnityEvent OnDeActivate;
    //Action OnActivate = delegate { };
    //Action OnDeActivate = delegate { };

    [SerializeField] Animator _anims = null;
    [SerializeField] float _deactivationTime = 1f;

    Collider _col = null;

    private void Awake()
    {
        _col = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<NMA_Controller>();
        if (player != null)
        {
            _anims.SetBool("Pressed", true);
            OnActivate.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var player = other.GetComponent<NMA_Controller>();
        if (player != null)
        {
            _anims.SetBool("Pressed", false);
            StartCoroutine(releaseActivation());
        }
    }

    IEnumerator releaseActivation()
    {
        yield return new WaitForSeconds(_deactivationTime);
        OnDeActivate.Invoke();
    }
}
