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

    bool isActive = false;
    BoxCollider _col;

    [SerializeField] List<Collider> OnTop = new List<Collider>();

    private void Awake()
    {
        _col = GetComponent<BoxCollider>();
        _col.isTrigger = true;
    }
    private void Update()
    {
        if (isActive)
            killOnTopEntities();
    }

    public void killOnTopEntities()
    {
        var FilteredOnTop = new List<Collider>();
        foreach (var coll in OnTop)
        {
            bool falling = false;
            var slug = coll.GetComponent<Baboso>();
            if (slug != null)
            {
                slug.FallInTrap();
                falling = true;
            }

            var player = coll.GetComponent<Controller>();
            if (player != null)
            {
                //Le digo al player que valió verga :D
                player.FallInTrap();
                falling = true;
            }

            var grunt = coll.GetComponent<Grunt>();
            if (grunt != null)
            {
                grunt.ChangeStateTo(CommonState.fallTrap);
                falling = true;
            }

            if (!falling)
                FilteredOnTop.Add(coll);
        }

        OnTop = FilteredOnTop;
    }
    public void OnEnableTrap()
    {
        _anims.SetBool("Activate", true);
        OnActivate.Invoke();

        isActive = true;
        killOnTopEntities();
    }
    public void OnDisableTrap()
    {
        _anims.SetBool("Activate", false);
        OnDeActivate.Invoke();

        isActive = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        //print($"{other.gameObject.name} entro a la trampa");
        var grunt = other.GetComponent<Grunt>();
        var Baboso = other.GetComponent<Baboso>();

        if (grunt != null || Baboso != null)
        {
            OnTop.Add(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //print($"{other.gameObject.name} Salió de la trampa");
        if (OnTop.Contains(other))
            OnTop.Remove(other);
    }
}
