using System;
using System.Collections.Generic;
using Core.InventorySystem;

namespace Core.Interaction
{
    public struct InteractionDisplaySettings
    {
        //Este es el count de interacciones. No admite repeticiones.
        public int AviableInteractionsAmmount;

        //Los tipos de acciones que son soportadas. Se rellena automáticamente.
        public List<Tuple<OperationType, IInteractionComponent>> SuportedOperations;
    }

    public interface IInteractable
    {
        bool InteractionEnabled { get; set; }
        bool LimitedDisplay { get; } //Si las operaciones se muestran indefinidamente o no.
        float ActiveTime { get; }    //El tiempo en el que se expone las operaciones.

        void OnInteractionMouseOver();
        void OnInteractionMouseExit();

        //Contemplar: Añadir componentes --> Activar o desactivar componentes.
        InteractionDisplaySettings GetInteractionDisplaySettings(Inventory inventory, bool ignoreInventory, params object[] aditionalParameters);
        IInteractionComponent GetInteractionComponent(OperationType operation, bool isDynamic);
        bool HasCompomponentOfType(OperationType operation);
    }
}
