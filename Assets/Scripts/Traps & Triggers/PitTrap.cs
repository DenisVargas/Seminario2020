using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PitTrap : MonoBehaviour
{
    [SerializeField] UnityEvent OnActivate = new UnityEvent();
    [SerializeField] UnityEvent OnDeActivate = new UnityEvent();

    [SerializeField] Animator _anims = null;

    public void OnEnableTrap()
    {
        _anims.SetBool("Activate", true);
        OnActivate.Invoke();
    }

    public void OnDisableTrap()
    {
        _anims.SetBool("Activate", false);
        OnDeActivate.Invoke();
    }
}
