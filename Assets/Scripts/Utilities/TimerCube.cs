using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerCube : MonoBehaviour
{
    public TimerHandler hability1;
    public TimerHandler hability2;
    public TimerHandler hability3;

    private void Awake()
    {
        hability1 = new TimerHandler(10, () => { print("Terminó la habilidad 1"); });
        hability2 = new TimerHandler(4, 5, () => { print("Terminó la habilidad 2"); });
        hability3 = new TimerHandler(5, 3, 3, () => { print("Terminó la habilidad 3"); });
    }

    // Start is called before the first frame update
    void Start()
    {
        hability1.Start();
        hability2.Start();
        hability3.Start();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.A))
        {
            hability1.Pause();
            hability2.Pause();
            hability3.Pause();
        }
        if (Input.GetKey(KeyCode.S))
        {
            hability1.Continue();
            hability2.Continue();
            hability3.Continue();
        }

        //print("Falta:" + hability3.RemainingTime);
    }
}
