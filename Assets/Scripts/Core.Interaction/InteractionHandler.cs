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

        //Lista de interacciones posibles.
        Dictionary<OperationType, List<IInteractionComponent>> interactionComponents;

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
            Inventory ActiveItem = null;
            if (aditionalParameters != null && aditionalParameters.Length > 0)
                ActiveItem = (Inventory)aditionalParameters[0];

            InteractionDisplaySettings ip = new InteractionDisplaySettings(displayOptions);
            ip.SuportedOperations = interactionComponents.Keys.ToList();
            //if (item != null)
            //    ip.SuportedOperations.AddRange(item.GetAllOperations(ActiveItem));

            return ip;
        }

        /// <summary>
        /// key en 0 es un componente Estático. Mientras que 1 es un componente dinámico.
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public IInteractionComponent GetInteractionComponent(OperationType operation, bool isDynamic)
        {
            if (interactionComponents.ContainsKey(operation))
            {
                var comp = interactionComponents[operation]
                           .Where(x => x.isDynamic == isDynamic)
                           .FirstOrDefault(null);

                return comp;
            }

            return null;
        }

        private void Awake()
        {
            interactionComponents = new Dictionary<OperationType, List<IInteractionComponent>>();

            //Buscar todos los componentes y listarlos.
            var icomp = GetComponentsInChildren<IInteractionComponent>();
            foreach (var comp in icomp)
            {
                var pairs = comp.GetAllOperations(null);
                foreach (var pair in pairs)
                {
                    if (!interactionComponents.ContainsKey(pair.Item1))
                    {
                        if (interactionComponents[pair.Item1] == null ||
                            interactionComponents[pair.Item1].Count == 0)
                            interactionComponents[pair.Item1] = new List<IInteractionComponent>();

                        interactionComponents[pair.Item1].Add(pair.Item2);
                    }
                }
            }
        }
    }
}
