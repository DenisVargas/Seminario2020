using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.InventorySystem;
using UnityEngine;

public class cmd_Combine : IQueryComand
{
    private Item target;
    private Inventory _inventory;
    public Action AnimTrigget = delegate { };
    private Action<Item> AttachToArm = delegate { };

    public cmd_Combine(Item target, Inventory inventory, Action<Item> AttachToArm,Action AnimTrigger)
    {
        this.target = target;
        _inventory = inventory;
        this.AnimTrigget = AnimTrigger;
        this.AttachToArm = AttachToArm;
    }

    public bool completed { get; private set; } = false;
    public bool isReady { get; private set; } = false;
    public bool cashed => true;

    public void SetUp()
    {
        AnimTrigget();
        isReady = true;
    }
    public void Execute()
    {
        Item equiped = _inventory.UnEquipItem();//Desequipo el objeto equipado.
        //Tomamos item a y b, guardamos sus indexes.
        int AID = target.ID;
        int BID = equiped.ID;

        ItemData resultData = null;
        var prefab = ItemDataBase.Manager.Combine(AID, BID, out resultData);//Obtengo el prefab resultante.
        if (prefab)
        {
            //Destruyo a y b.
            GameObject.Destroy(equiped.gameObject);
            GameObject.Destroy(target.gameObject);
            //Instancio el prefab resultante.
            var newItem = GameObject.Instantiate(prefab);
            var itemSettings = newItem.GetComponent<Item>();
            itemSettings.SetData(resultData);
            //Lo equipo en el inventario.
            AttachToArm(itemSettings);
        }
        completed = true;
    }
    public void Cancel()
    {
    }
}
