using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NMA_Controller : MonoBehaviour
{
    [SerializeField] Transform MouseDebug = null;
    //[SerializeField] float moveSpeed = 6;

    NavMeshAgent agent = null;
    Camera viewCamera = null;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        viewCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        //Esto es constante por ahora, por motivos de Debugueo. 
        //TODO: Añadir el Feedback dentro del Input.
        Vector3 wMousePos = viewCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,
                                                                      Input.mousePosition.y,
                                                                      viewCamera.transform.position.y));
        MouseDebug.position = wMousePos;

        //Asigno la posición como target a nuestro navMeshAgent.
        if (Input.GetMouseButton(1))
        {
            //Calculo la posición del mouse en el espacio.
            transform.LookAt(wMousePos + Vector3.up * transform.position.y);
            agent.destination = wMousePos;
        }
    }
}
