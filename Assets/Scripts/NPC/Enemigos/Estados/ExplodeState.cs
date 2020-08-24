using System;
using UnityEngine;
using IA.FSM;

[RequireComponent(typeof(Trail))]
public class ExplodeState : State
{
    public Action DeactivateComponents = delegate { };

    [SerializeField] GameObject ExplotionParticle = null;
    [SerializeField] LayerMask _StaineableMask = ~0;
    [SerializeField] float _explodeRange = 5f;

    #region DEBUG
    [SerializeField] Color DEBUG_ExplodeRangeColor = Color.white;
    private void OnDrawGizmos()
    {
        Gizmos.matrix = Matrix4x4.Scale(new Vector3(1, 0, 1));
        Gizmos.color = DEBUG_ExplodeRangeColor;
        Gizmos.DrawWireSphere(transform.position, _explodeRange);
    } 
    #endregion

    public override void Begin()
    {
        //Activo la particula de explosión.
        ExplotionParticle.SetActive(true);
        //Me dejo de mover.

        //Desactivo mis otros componentes.
        DeactivateComponents();

        //Busco todos los objetos que están al rededor y hacer que se mojen con baba.
        var findeds = Physics.OverlapSphere(transform.position, _explodeRange, _StaineableMask, QueryTriggerInteraction.Collide);
        foreach (var col in findeds)
        {
            var staineable = col.GetComponent<Staineable>();
            if (staineable != null)
            {
                staineable.StainWithSlime();
            }
        }

        SwitchToState(CommonState.dead);
    }
}
