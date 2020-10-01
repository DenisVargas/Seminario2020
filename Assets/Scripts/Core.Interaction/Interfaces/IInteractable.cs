using System.Collections.Generic;
using UnityEngine;
using Core.InventorySystem;
using System;

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
        public List<Tuple<OperationType, IInteractionComponent>> SuportedOperations;
        public InteractionDisplaySettings(InteractionDisplaySettings toCopy)
        {
            LimitedDisplay = toCopy.LimitedDisplay;
            ActiveTime = toCopy.ActiveTime;
            SuportedOperations = new List<Tuple<OperationType, IInteractionComponent>>();
        }
    }

    public interface IInteractable
    {
        bool InteractionEnabled { get; set; }
        int InteractionsAmmount { get; }

        void OnInteractionMouseOver();
        void OnInteractionMouseExit();

        //Contemplar: Añadir componentes --> Activar o desactivar componentes.
        InteractionDisplaySettings GetInteractionDisplaySettings(params object[] aditionalParameters);
        IInteractionComponent GetInteractionComponent(OperationType operation, bool isDynamic);
        bool HasStaticInteractionOfType(OperationType operation);
    }
}
