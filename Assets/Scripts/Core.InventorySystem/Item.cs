using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Core.Interaction;
using System;
using System.Linq;

namespace Core.InventorySystem
{
    //Item es una clase que utilizaremos para identificar  realizar operaciones sobre los objetos que pueden entrar dentro de un inventario.
    //Es una clase especial de Interactuable que admite multiples comandos, y es dinámico:
    //  Dependiendo de los items que estén a mano del jugador, este podría desbloquear mas iteracciones.
    [System.Serializable, RequireComponent(typeof(InteractionHandler))]
    public class Item : MonoBehaviour, IInteractionComponent
    {
        public Action<GameObject> OnPickDepedency = delegate { };
        public Action<Collider> OnSetOwner = delegate { };
        public Action OnThrowItem = delegate { };

        public int ID = 0;
        [SerializeField] string ItemName = "";
        [SerializeField] string Description = "";
        [SerializeField] Texture2D Icon = null;
        public bool isCombinable, isThroweable, isConsumable = false;

        //Operaciones estáticas son aquellas que siempre están disponibles.
        List<OperationType> Operations = new List<OperationType>();
        public bool isDynamic => true;

        [SerializeField] Collider _physicCollider = null;
        [SerializeField] Collider _owner = null;
        [SerializeField] Rigidbody _rb = null;

#if UNITY_EDITOR
        [SerializeField] bool debugThis = false; 
#endif

        protected virtual void Awake()
        {
            if (_rb == null)
                _rb = GetComponent<Rigidbody>();

            SetData(ItemDataBase.getItemData(ID));

            Operations.Add(OperationType.inspect);
            Operations.Add(OperationType.Drop);
        }

        /// <summary>
        /// Permite setear manualmente la data de este item.
        /// </summary>
        /// <param name="data"></param>
        public void SetData(ItemData data)
        {
            ID = (int)data.ID;
            ItemName = data.name;
            Description = data.Description;
            Icon = data.Icon;

            isCombinable = data.isCombinable;
            isThroweable = data.isTrowable;
            isConsumable = data.isConsumable;
        }

        //============================== Interaction System ============================================================

        public virtual List<Tuple<OperationType, IInteractionComponent>> GetAllOperations(Inventory CurrentInventory = null, bool ignoreInventory = false)
        {
            //Operaciones dinamicas son aquellas que dependen del inventario actual.
            List<Tuple<OperationType, IInteractionComponent>> _myOperations = new List<Tuple<OperationType, IInteractionComponent>>();

            if (CurrentInventory != null)//Si el inventario está específicado.
            {
                if (CurrentInventory.equiped == null)
                {
                    //Take es condicional de acuerdo al inventario del jugador.
                    _myOperations.Add(new Tuple<OperationType, IInteractionComponent>(OperationType.Take, this));
                }
                else
                {
                    _myOperations.Add(new Tuple<OperationType, IInteractionComponent>(OperationType.Exchange, this));

                    // Es combinable? Añado la operación si el equipado tiene una combinación con este item.
                    if (isCombinable && ItemDataBase.CanCombineItems(ID, CurrentInventory.equiped.ID))
                        _myOperations.Add(new Tuple<OperationType, IInteractionComponent>(OperationType.Combine, this));
                }
            }
            else //Si no se específica un inventario, se añaden todas las operaciones por defecto.
            {
                _myOperations.Add(new Tuple<OperationType, IInteractionComponent>(OperationType.Take, this));
            }

            //Es consumible?.
            if (isConsumable)
                _myOperations.Add(new Tuple<OperationType, IInteractionComponent>(OperationType.use, this));

            return _myOperations;
        }

        public virtual InteractionParameters getInteractionParameters(Vector3 requesterPosition)
        {
            var graph = FindObjectOfType<NodeGraphBuilder>();
            var pickNode = PathFindSolver.getCloserNodeInGraph(transform.position, graph);
            var neighbours = pickNode.Connections;
            var closer = neighbours.OrderBy(x => Vector3.Distance(requesterPosition, x.transform.position))
                                   .First();

            Vector3 LookToDirection = (transform.position - closer.transform.position).normalized.YComponent(0);

            return new InteractionParameters(closer, LookToDirection);
        }
        public virtual void InputConfirmed(OperationType operation, params object[] optionalParams) { }
        public virtual void ExecuteOperation(OperationType operation, params object[] optionalParams)
        {
            //Esto va a requerir una revisitada mas adelante.

            switch (operation)
            {
                case OperationType.Take:
                    //LLamar OnTake.
                    OnTake();
                    break;
                case OperationType.Ignite:
                    //Chequear si hay un componente IIgniteable y prenderlo fuego.
                    break;
                case OperationType.Activate:
                    //Llama Use.
                    break;
                case OperationType.Equip:
                    //Innecesario porque equip es más una funcionalidad del player.
                    break;
                case OperationType.Throw:
                    OnThrow();
                    break;
                case OperationType.inspect:
                    FindObjectOfType<InspectionMenu>().DisplayText(new string[] { Description }, () => { Debug.Log("Display Completado. "); });
                    break;
                case OperationType.Combine:
                    //Esta funcionalidad va por fuera del objeto. El objeto desconoce las combinaciones.
                    break;
                case OperationType.Exchange:
                    //Esta Operación se hace en el player.
                    break;
                case OperationType.Drop:
                    Drop(optionalParams); //Estándar fijo.
                    break;
                case OperationType.use:
                    Use(optionalParams); //Estándar fijo.
                    break;
                default:
                    break;
            }
        }
        public virtual void CancelOperation(OperationType operation, params object[] optionalParams) { }

        //==============================================================================================================

        public void SetPhysicsProperties(bool state, Vector3 velocity)
        {
            //print($"Set Physics Proterties:: el estado es: {state}, rigidBody es: {_rb}");
            if (_rb != null)
            {
                _rb.useGravity = state;
                _rb.velocity = velocity;
                _rb.isKinematic = !state;
            }
            _physicCollider.enabled = state;
        }
        public void SetOwner(Collider Owner)
        {
            if (Owner != null)
            {
                _owner = Owner;
                OnSetOwner(_owner);
            }
        }

        //==================================== Operaciones =============================================================

        /*
         * Notas: la idea de estas operaciones es que sean llamadas desde un operador si es necesario.
         * Hay opraciones que no requieren una implementación, pues los efectos se hacen externos al item.
        */

        protected virtual void OnTake()
        {

#if UNITY_EDITOR
            if (debugThis)
            {
                print($"Take Executed in item {gameObject.name}");
            }
#endif

            OnPickDepedency(gameObject);
        }
        protected virtual void Drop(params object[] optionalParams)
        {
            //print($"Drop Executed in item {gameObject.name}");

            //TODO::Rehabilitar interacción.
            //TODO::Caller:: Posicionar el objeto si es necesario.
            if (optionalParams != null && optionalParams.Length > 0) 
                transform.position = (Vector3)optionalParams[0];
        }
        protected virtual void OnThrow()
        {
            //print($"Throw Executed in item {gameObject.name}");
            OnThrowItem();
        }
        protected virtual void Use(params object[] optionalParams)
        {
            print($"Use Executed in item {gameObject.name}");
        }
    }
}
