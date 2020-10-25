using UnityEngine;
using Core.Timers;

public class TimerCube : MonoBehaviour
{
    public TimerHandler hability1 = new TimerHandler(); //Autoexpuesto.
    public TimerHandler hability2 = new TimerHandler(); //Autoexpuesto.
    public TimerHandler hability3 = new TimerHandler(); //Autoexpuesto.

    // Start is called before the first frame update
    void Start()
    {
        //Si el handler no esta inicializado, Al momento de llamar isReady o cualquier función este se inicializa automáticamente.

        //if (hability1.isReady)
        //    hability1.Start();
        //if (hability2.isReady)
        //    hability2.Start();
        //if (hability3.isReady)
        //    hability3.Start();

        //Si quieres crear timers a mano. Usa directamente la clase TimeManager.
        //TimeManager.CreateTimer retorna un índice que puede ser manageado por un Handler.

        // hability3.RemainingTime retorna el tiempo restante para que se termine el timer.

        // hability1.isPaused retorna true si el timer esta pausado en este momento.

        // Hay soporte para cambiar el tiempo de delay.
        hability1.setDelay(5f);

        // Los timers soportan 3 Eventos principales: OnTimeStart, OnTimeUpdate, OnTimesUp.

        // OnTimeStart inicia cada vez que el Timer Arranca el conteo.
        hability1.setOnTimeStart((turns)=>
        {
            if (turns == 1) print("Starting!");
            print("=========== Start of Counting =================");
        });
        // El parametro "Turns" es un índice que arranca en 1. Indica el número de vueltas.

        // OnTimeUpdate se llama cada frame mientras el timer esta activo. 
        hability1.setOnTimeUpdate((progress)=>{
            print("El progreso es: " + progress);
        });
        // El parámetro "progress" es un flotante con valores de 0 a 1, representa el porcentaje de completado.

        //OnTimesUp se llama cada vez que el timer finaliza un ciclo.
        hability1.setOnTimesUp((turns, ciclesEnd) =>
        {
            if (ciclesEnd) print("All cicles completed!");
            print("=========== End of Counting =================");
        });
        // El parámetro "turns" indica el número de vueltas. 
        // El parámetro "ciclesEnd" es un booleano que determina si todos los ciclos fueron completados.
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            hability1.Pause();
            hability2.Pause();
            hability3.Pause();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            hability1.Continue();
            hability2.Continue();
            hability3.Continue();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            hability1.Reset();
            hability2.Reset();
            hability3.Reset();
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            print("Super F");
            if (hability1.isReady)
                hability1.Start();
            //if (hability2.isReady)
            //    hability2.Start();
            //if (hability3.isReady)
            //    hability3.Start();
        }
    }
}
