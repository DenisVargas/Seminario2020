using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakedFloor : MonoBehaviour
{
    public List<Rigidbody> pieces = new List<Rigidbody>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            foreach (var item in pieces)
            {
                item.useGravity = true;
                item.AddForce(0, Random.Range(0, 100),0,ForceMode.Force);
            }
            var player = other.GetComponent<Controller>();
            if (player)
            player.enabled = false;
            other.GetComponent<Rigidbody>().isKinematic = false;
            other.GetComponent<Rigidbody>().useGravity = true;
        }
        Debug.Log("entre a la collision");
        
    }
}
