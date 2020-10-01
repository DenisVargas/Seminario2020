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
        //[SerializeField] Collider _interactionCollider = null;
        [SerializeField] InteractionDisplaySettings displayOptions = new InteractionDisplaySettings();

        //Lista de interacciones posibles.
        List<Tuple<OperationType, IInteractionComponent>> interactionComponents = new List<Tuple<OperationType, IInteractionComponent>>();

        [SerializeField] Renderer _renderer = null;

        //Bloquea la interactividad.
        [SerializeField] bool _interactionEnabled = true;
        public bool InteractionEnabled
        {
            get => _interactionEnabled;
            set => _interactionEnabled = value;
        }
        int _interactionsListed = -1;
        public int InteractionsAmmount
        {
            get
            {
                if (_interactionsListed == -1)
                    GetInteractionsAbviable();

                return _interactionsListed;
            }
        }

        /// <summary>
        /// Retorna las opciones de display para este objeto, contemplando los tiempos del display y las operaciones disponibles.
        /// </summary>
        /// <returns>All suported Operation and </returns>
        public InteractionDisplaySettings GetInteractionDisplaySettings(params object[] aditionalParameters)
        {
            Inventory ActiveInventory = null;
            if (aditionalParameters != null && aditionalParameters.Length > 0)
                ActiveInventory = (Inventory)aditionalParameters[0];

            InteractionDisplaySettings ip = new InteractionDisplaySettings(displayOptions);
            ip.SuportedOperations = GetInteractionsAbviable(ActiveInventory);

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
            return GetInteractionsAbviable().Where(x => x.Item1 == operation && x.Item2.isDynamic == isDynamic)
                                            .Select(x => x.Item2)
                                            .FirstOrDefault();
        }

        private List<Tuple<OperationType, IInteractionComponent>> GetInteractionsAbviable(Inventory inventory = null)
        {
            interactionComponents = new List<Tuple<OperationType, IInteractionComponent>>();

            //Buscar todos los componentes y listarlos.
            var icomp = GetComponentsInChildren<IInteractionComponent>();

            foreach (var comp in icomp)
            {
                var pairs = comp.GetAllOperations(inventory);
                foreach (var pair in pairs)
                {
                    if (!interactionComponents.Contains(pair))
                        interactionComponents.Add(pair);
                }
            }

            _interactionsListed = interactionComponents.Count;
            return interactionComponents;
        }
        /// <summary>
        /// Chequea si hay un componente estático para una determinada operación.
        /// </summary>
        /// <param name="operation">El tipo de operación relacionada al componente que estamos buscando.</param>
        /// <returns>False si no hay ningún componente que cumpla con dicho requerimiento.</returns>
        public bool HasStaticInteractionOfType(OperationType operation)
        {
            var aviableInteractions = GetInteractionsAbviable();

            foreach (var pair in aviableInteractions)
            {
                if (pair.Item1 == operation && pair.Item2.isDynamic == false)
                    return true;
            }

            return false;
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
    }
}
