using UnityEngine;

namespace Core.Interaction
{
    public interface IIgnitableObject : IInteractionComponent
    {
        bool lockInteraction { get; }
        bool isFreezed { get; set; }
        bool Burning { get; }

        bool IsActive { get; }
        GameObject gameObject { get; }

        //El objeto igniteable debería iniciar un efecto en cadena al activarse.
        //void StartChainReaction();
        //Debería encargarse del renderizado también. 
        //Para ello se debería encargar de stainear el objeto.
        void StainObjectWithSlime();
    } 
}