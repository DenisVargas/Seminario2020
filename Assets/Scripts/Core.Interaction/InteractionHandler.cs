using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
        //Lista de interacciones posibles.
        Dictionary<OperationType, IInteractionComponent> interactionComponents;

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
        public InteractionDisplaySettings GetInteractionDisplaySettings()
        {
            InteractionDisplaySettings ip = new InteractionDisplaySettings(displayOptions);
            ip.SuportedOperations = interactionComponents.Keys.ToList();
            return ip;
        }

        public IInteractionComponent GetInteractionComponent(OperationType operation)
        {
            if (interactionComponents.ContainsKey(operation))
                return interactionComponents[operation];

            return null;
        }

        private void Awake()
        {
            interactionComponents = new Dictionary<OperationType, IInteractionComponent>();

            var icomp = GetComponentsInChildren<IInteractionComponent>();
            foreach (var comp in icomp)
            {
                if (!interactionComponents.ContainsKey(comp.OperationType))
                    interactionComponents.Add(comp.OperationType, comp);
            }
        }
    }
}
