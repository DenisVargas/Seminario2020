using UnityEngine;
using Core.InventorySystem;

public class Cascade : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Cascade");
        if(other.tag == "Player")
        {
            Controller pl = other.GetComponentInParent<Controller>();
            if (pl != null && pl.Inventory.equiped != null)
            {
                 var eq = pl.Inventory.equiped.ID;
                if (eq == Core.InventorySystem.ItemID.Antorcha)//Si tiene equipado una antorcha hay que apagarlo.
                {
                    Torch torch = pl.Inventory.equiped.GetComponent<Torch>();
                    torch.isBurning = false;
                    return;
                }

                if(eq == Core.InventorySystem.ItemID.JarronBaba)//Si tiene un jarron con baba, lo reonvertimos en un jarron comun.
                {
                    var jb = pl.Inventory.UnEquipItem();
                    Destroy(jb.gameObject);

                    var jprefab = Core.InventorySystem.ItemDataBase.getRandomItemPrefab(Core.InventorySystem.ItemID.Jarron);

                    var j = Instantiate(jprefab);
                    var ji = j.GetComponent<Core.InventorySystem.Item>();
                    pl.Inventory.EquipItem(ji);
                    pl.AttachItemToHand(ji);
                    return;
                }
            }
        }

        //Identificar el objeto con el que ha colisionado.
        var item = other.GetComponent<Core.InventorySystem.Item>();
        if(item != null)
        {
            //Si es una antorcha sola hay que apagarlo.
            if(item.ID == ItemID.Antorcha)
            {
                var t = item.GetComponent<Torch>();
                t.isBurning = false;
                return;
            }
            if(item.ID == ItemID.JarronBaba)
            {
                Vector3 currentVel = item.GetComponent<Rigidbody>().velocity;
                Quaternion currentOrientation = item.GetComponent<Transform>().rotation;
                Vector3 currentPos = item.GetComponent<Transform>().position;

                Destroy(item.gameObject);

                var jprefab = ItemDataBase.getRandomItemPrefab(ItemID.Jarron);
                var jgo = Instantiate(jprefab, currentPos, currentOrientation);
                var jgorb = jgo.GetComponent<Rigidbody>();
                jgorb.velocity = currentVel;
                jgorb.isKinematic = false;
                jgorb.useGravity = true;
                return;
            }
        }
    }
}
