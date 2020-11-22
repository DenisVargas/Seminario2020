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
            }
          
        }
        Debug.Log("entre a la collision");
        
    }
}
