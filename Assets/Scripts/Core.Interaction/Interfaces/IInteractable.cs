using System.Collections.Generic;
using UnityEngine;
using Core.InventorySystem;

namespace Core.Interaction
{
    public struct InteractionDisplaySettings
    {
        //Si las operaciones se muestran indefinidamente o no.
        public bool LimitedDisplay;
        //El tiempo en el que se expone las operaciones.
        public float ActiveTime;
        //Los tipos de acciones que son soportadas. Se rellena automáticamente.
        [HideInInspector]
        public List<OperationType> SuportedOperations;
        public InteractionDisplaySettings(InteractionDisplaySettings toCopy)
        {
            this.LimitedDisplay = toCopy.LimitedDisplay;
            this.ActiveTime = toCopy.ActiveTime;
            this.SuportedOperations = new List<OperationType>();
        }
    }

    public interface IInteractable
    {
        bool InteractionEnabled { get; set; }
        int InteractionsAmmount { get; }

        InteractionDisplaySettings GetInteractionDisplaySettings(params object[] aditionalParameters);
        IStaticInteractionComponent GetInteractionComponent(OperationType operation);
        //Contemplar: Añadir componentes --> Activar o desactivar componentes.
    }
}
