using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class GroundTrigger : MonoBehaviour
{
    UnityEvent OnActivate;
    UnityEvent OnDeActivate;
    //Action OnActivate = delegate { };
    //Action OnDeActivate = delegate { };

    [SerializeField] Animator _anims;
    [SerializeField] float _deactivationTime;

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
