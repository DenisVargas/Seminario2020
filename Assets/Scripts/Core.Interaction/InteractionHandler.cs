using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Core.InventorySystem;

namespace Core.Interaction
{
    /// <summary>
    /// Se ocupa de comunicar el menu de comandos con el objeto. 
    /// Lista las operaciones disponibles.
    /// Permite activar o desactivar componentes.
    /// Permite Activar o desactivar todas las interacciones.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class InteractionHandler : MonoBehaviour, IInteractable
    {
        [SerializeField] Collider _interactionCollider = null;
        [SerializeField] InteractionDisplaySettings displayOptions = new InteractionDisplaySettings();
        //Item attacheado a este handler.
        Item item;
        //Lista de interacciones posibles.
        Dictionary<OperationType, IStaticInteractionComponent> interactionComponents;

        //Bloquea la interactividad.
        [SerializeField] bool _interactionEnabled = true;
        public bool InteractionEnabled
        {
            get => _interactionEnabled;
            set
            {
                _interactionEnabled = value;
                _interactionCollider.enabled = _interactionEnabled;
            }
        }
        public int InteractionsAmmount => interactionComponents.Count;
        /// <summary>
        /// Retorna las opciones de display para este objeto, contemplando los tiempos del display y las operaciones disponibles.
        /// </summary>
        /// <returns>All suported Operation and </returns>
        public InteractionDisplaySettings GetInteractionDisplaySettings(params object[] aditionalParameters)
        {
            ItemData ActiveItem = null;
            if (aditionalParameters.Length > 0)
                ActiveItem = (ItemData)aditionalParameters[0];

            InteractionDisplaySettings ip = new InteractionDisplaySettings(displayOptions);
            ip.SuportedOperations = interactionComponents.Keys.ToList();
            if (item != null)
                ip.SuportedOperations.AddRange(item.GetAllOperations(ActiveItem));

            return ip;
        }

        public IStaticInteractionComponent GetInteractionComponent(OperationType operation)
        {
            if (interactionComponents.ContainsKey(operation))
                return interactionComponents[operation];

            return null;
        }

        private void Awake()
        {
            interactionComponents = new Dictionary<OperationType, IStaticInteractionComponent>();

            var icomp = GetComponentsInChildren<IStaticInteractionComponent>();
            foreach (var comp in icomp)
            {
                if (!interactionComponents.ContainsKey(comp.OperationType))
                    interactionComponents.Add(comp.OperationType, comp);
            }

            var attachedItem = GetComponent<Item>();
            if (attachedItem != null)
            {
                item = attachedItem;
            }
        }
    }
}
