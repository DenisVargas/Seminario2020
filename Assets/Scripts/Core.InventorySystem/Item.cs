using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Core.Interaction;
using System;

namespace Core.InventorySystem
{
    //Item es una clase que utilizaremos para identificar  realizar operaciones sobre los objetos que pueden entrar dentro de un inventario.
    //Es una clase especial de Interactuable que admite multiples comandos, y es dinámico:
    //  Dependiendo de los items que estén a mano del jugador, este podría desbloquear mas iteracciones.
    [RequireComponent(typeof(InteractionHandler))]
    public class Item : MonoBehaviour, IInteractionComponent
    {
        public int ID = 0;
        public string ItemName = "";
        public string Description = "";
        public Texture2D Icon = null;

        public bool isCombinable, isThroweable, isConsumable = false;

        //Operaciones estáticas son aquellas que siempre están disponibles.
        List<OperationType> Operations = new List<OperationType>();

        public bool isDynamic => true;
        public Vector3 LookToDirection => transform.position;

        [SerializeField] Collider _physicCollider;
        Rigidbody _rb = null;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();

            SetData(ItemDataBase.Manager.getItemData(ID));

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

        public virtual List<Tuple<OperationType, IInteractionComponent>> GetAllOperations(Inventory CurrentInventory = null)
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
                    if (isCombinable && ItemDataBase.Manager.CanCombineItems(ID, CurrentInventory.equiped.ID))
                        _myOperations.Add(new Tuple<OperationType, IInteractionComponent>(OperationType.Combine, this));
                }
            }
            else //Si no se específica un inventario, se añaden todas las operaciones por defecto.
            {
                _myOperations.Add(new Tuple<OperationType, IInteractionComponent>(OperationType.Take, this));
            }

            // Es tirable?. Este objeto no se puede tirar, si no es equipado primero.La secuencia es Take>Throw.
            //if (isThroweable)
            //    _myOperations.Add(new Tuple<OperationType, IInteractionComponent>(OperationType.Throw, this));

            //Es consumible?.
            if (isConsumable)
                _myOperations.Add(new Tuple<OperationType, IInteractionComponent>(OperationType.use, this));

            return _myOperations;
        }

        public Vector3 requestSafeInteractionPosition(Vector3 requesterPosition)
        {
            return transform.position;
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
                    StartCoroutine(ParabolicMove((Transform)optionalParams[0]));
                    break;
                case OperationType.inspect:
                    //La UI no requiere del objeto.
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
        //==================================== Operaciones =============================================================

        /*
         * Notas: la idea de estas operaciones es que sean llamadas desde un operador si es necesario.
         * Hay opraciones que no requieren una implementación, pues los efectos se hacen externos al item.
        */

        private void OnTake()
        {
            _rb.useGravity = false;
            _rb.velocity = Vector3.zero;
            _physicCollider.enabled = false;
        }
        public virtual void Drop(params object[] optionalParams)
        {
            //Rehabilitar interacción.
            if (optionalParams != null && optionalParams.Length > 0)
                transform.position = (Vector3)optionalParams[0];

            _rb.useGravity = true;
            _physicCollider.enabled = true;
        }
        public virtual void Use(params object[] optionalParams) { }

        //==================================== Corrutinas ==============================================================

        IEnumerator ParabolicMove(Transform target)
        {
            Vector3 firstPosition = transform.position;
            for (float i = 0; i < 1; i += 0.1f)
            {
                yield return new WaitForSeconds(0.01f);
                //transform.position = Vector3.Slerp(firstPosition, target.transform.position, i);
                transform.position = new Vector3(Mathf.Lerp(firstPosition.x, target.transform.position.x, i), Mathf.Lerp(firstPosition.y, target.position.y, i) + Mathf.Sin(i * Mathf.PI) * 5, Mathf.Lerp(firstPosition.z, target.transform.position.z, i));
            }
        }

    }
}
