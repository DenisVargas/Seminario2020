using System;
using System.Collections.Generic;
using System.Linq;
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
        //Si las operaciones se muestran indefinidamente o no.
        [SerializeField] bool _limitedDisplay = false;
        public bool LimitedDisplay { get => _limitedDisplay; }
        //El tiempo en el que se expone las operaciones.
        [SerializeField] float _activeTime = 5.0f;
        public float ActiveTime { get => _activeTime; }
        [SerializeField] Renderer _renderer = null;

        //Bloquea la interactividad.
        [SerializeField] bool _interactionEnabled = true;
        public bool InteractionEnabled
        {
            get => _interactionEnabled;
            set => _interactionEnabled = value;
        }

        public void OnInteractionMouseOver()
        {
            if (_renderer && InteractionEnabled)
            {
                var mat = _renderer.material;
                mat.SetFloat("_mouseOver", 1);
            }
        }
        public void OnInteractionMouseExit()
        {
            if (_renderer && InteractionEnabled)
            {
                var mat = _renderer.material;
                mat.SetFloat("_mouseOver", 0);
            }
        }

        /// <summary>
        /// Retorna una estructura que contiene la cantidad de interacciones posibles y una lista de 
        /// </summary>
        /// <returns>All suported Operation and </returns>
        public InteractionDisplaySettings GetInteractionDisplaySettings(Inventory inventory = null, bool ignoreInventory = true, params object[] aditionalParameters)
        {
            InteractionDisplaySettings ip = new InteractionDisplaySettings();
            var interactionComponents = new List<Tuple<OperationType, IInteractionComponent>>();
            HashSet<OperationType> operationsListed = new HashSet<OperationType>();

            //Buscar todos los componentes y listarlos.
            var icomp = GetComponentsInChildren<IInteractionComponent>();

            foreach (var comp in icomp)
            {
                var pairs = comp.GetAllOperations(inventory, ignoreInventory);
                foreach (var pair in pairs)
                {
                    if (!operationsListed.Contains(pair.Item1))
                        operationsListed.Add(pair.Item1);
                    if (!interactionComponents.Contains(pair))
                        interactionComponents.Add(pair);
                }
            }

            ip.SuportedOperations = interactionComponents;
            ip.AviableInteractionsAmmount = operationsListed.Count;
            return ip;
        }
        /// <summary>
        /// key en 0 es un componente Estático. Mientras que 1 es un componente dinámico.
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="isDynamic">Si el target buscado es de tipo Dinámico.</param>
        /// <returns></returns>
        public IInteractionComponent GetInteractionComponent(OperationType operation, bool isDynamic)
        {
            InteractionDisplaySettings aviableInteractions = GetInteractionDisplaySettings(null, true);
            return aviableInteractions.SuportedOperations.Where(x => x.Item1 == operation && x.Item2.isDynamic == isDynamic)
                                                         .Select(x => x.Item2)
                                                         .FirstOrDefault();
        }
        /// <summary>
        /// Chequea si hay un componente asociado a una determinada operación, ignorando modificadores como el inventario.
        /// </summary>
        /// <param name="operation">El tipo de operación relacionada al componente que estamos buscando.</param>
        /// <returns>False si no hay ningún componente que cumpla con dicho requerimiento.</returns>
        public bool HasCompomponentOfType(OperationType operation)
        {
            InteractionDisplaySettings aviableInteractions = GetInteractionDisplaySettings(null, true);

            foreach (var pair in aviableInteractions.SuportedOperations)
            {
                if (pair.Item1 == operation && pair.Item2.isDynamic == false)
                    return true;
            }

            return false;
        }
    }
}
