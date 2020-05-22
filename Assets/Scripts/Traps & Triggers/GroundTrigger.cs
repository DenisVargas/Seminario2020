using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
public class GroundTrigger : MonoBehaviour
{
    [SerializeField] UnityEvent OnActivate = new UnityEvent();
    [SerializeField] UnityEvent OnDeActivate = new UnityEvent();
    //Action OnActivate = delegate { };
    //Action OnDeActivate = delegate { };

    [SerializeField] Animator _anims = null;
    [SerializeField] float _deactivationTime = 0f;

    Collider _col = null;
    [SerializeField] List<Collider> OnTop = new List<Collider>();

    private void Awake()
    {
        _col = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        print("Hay una collision: " + other.gameObject.name);
        OnTop.Add(other);
        _anims.SetBool("Pressed", true);
        OnActivate.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (OnTop.Contains(other))
            OnTop.Remove(other);

        if (OnTop.Count <= 0 )
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
