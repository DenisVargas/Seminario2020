using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;
using Core.DamageSystem;

[RequireComponent(typeof(BoxCollider))]
public class PitTrap : MonoBehaviour
{
    [SerializeField] UnityEvent OnActivate = new UnityEvent();
    [SerializeField] UnityEvent OnDeActivate = new UnityEvent();

    [SerializeField] Animator _anims = null;

    BoxCollider _col;

    [SerializeField] List<Collider> OnTop = new List<Collider>();

    private void Awake()
    {
        _col = GetComponent<BoxCollider>();
        _col.isTrigger = true;
    }

    public void OnEnableTrap()
    {
        _anims.SetBool("Activate", true);
        OnActivate.Invoke();

        var FilteredOnTop = new List<Collider>();
        foreach (var coll in OnTop)
        {
            bool falling = false;
            var slug = coll.GetComponent<Baboso>();
            if (slug != null)
            {
                slug.ChangeStateTo(Baboso.BabosoState.falligTrap);
                falling = true;
            }

            var player = coll.GetComponent<NMA_Controller>();
            if (player != null)
            {
                //Le digo al player que valió verga :D
                player.FallInTrap();
                falling = true;
            }

            var grunt = coll.GetComponent<Grunt>();
            if (grunt != null)
            {
                //Accedo a grunt y le digo que pase a falling.
                grunt.ChangeStateTo(Grunt.BoboState.fallTrap);
                falling = true;
            }

            if (!falling)
            {
                FilteredOnTop.Add(coll);
            }
        }

        OnTop = FilteredOnTop;
    }

    public void OnDisableTrap()
    {
        _anims.SetBool("Activate", false);
        OnDeActivate.Invoke();
    }

    private void OnTriggerEnter(Collider other)
    {
        var agent = other.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            OnTop.Add(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        print("Salió: " + other.gameObject.name);
        if (OnTop.Contains(other))
            OnTop.Remove(other);
    }
}
