using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Core.Interaction;

namespace Core.InventorySystem
{
    //Item es una clase que utilizaremos para identificar los objetos dentro de un inventario.
    //Nos permite realizar acciones sobre ella.
    //Es una clase especial de Interactuable que admite multiples comandos, y es dinámico.
    //Por dinamico, me refiero a que dependiendo de los items que estén a manod el jugador, este podría desbloquear mas iteracciones.
    [RequireComponent(typeof(InteractionHandler))]
    public class Item : MonoBehaviour
    {
        [SerializeField] ItemData data;

        //Operaciones estáticas son aquellas que siempre están disponibles.
        List<OperationType> staticOperations = new List<OperationType>();

        private void Awake()
        {
            staticOperations.Add(OperationType.inspect);
            staticOperations.Add(OperationType.Drop);
        }

        public List<OperationType> GetAllOperations(ItemData CurrentInventory)
        {
            //Operaciones dinamicas son aquellas que dependen del inventario actual.
            List<OperationType> dinamicOperations = new List<OperationType>();

            if (CurrentInventory == null)
                dinamicOperations.Add(OperationType.Take);
            else
            {
                // Es intercambiable?.
                dinamicOperations.Add(OperationType.Exchange);
                // Es combinable?
                if (data.isCombinable) dinamicOperations.Add(OperationType.Combine);
                // Es tirable?.
                if (data.isTrowable) dinamicOperations.Add(OperationType.Throw);
                //Es consumible?.
                if (data.isConsumable) dinamicOperations.Add(OperationType.use);
            }

            dinamicOperations.AddRange(staticOperations);
            return dinamicOperations;
        }
    }
}
