using System;
using Core.TimeManagement;

public class TimerHandler
{
    int ID = -1;
    bool reverseCounting = false;

    /// <summary>
    /// Crea un Time Handler.
    /// </summary>
    /// <param name="Duration">Tiempo de duración en segundos.</param>
    /// <param name="OnTimesUp">Callback que se llamará al finalizar el conteo.</param>
    public TimerHandler(float Duration, Action OnTimesUp)
    {
        //Single timer 
        var _manager = TimeManager.instance;
        if (_manager)
            ID = _manager.CreateNewTimer(0, 1, Duration, OnTimesUp);
    }
    /// <summary>
    /// Crea un TimeHandler.
    /// </summary>
    /// <param name="Duration">Tiempo de duración en segundos.</param>
    /// <param name="Repetitions">Cantidad de repeticiones.</param>
    /// <param name="OnTimesUp">Callback que se llamará al final el conteo.</param>
    public TimerHandler(float Duration, int Repetitions, Action OnTimesUp)
    {
        //Un timer que inicie inmediatamente, y haga x cantidad de repeticiones.
        var _manager = TimeManager.instance;
        if (_manager != null)
            ID = _manager.CreateNewTimer(0, Repetitions, Duration, OnTimesUp);
    }
    /// <summary>
    /// Crea un TimeHandler.
    /// </summary>
    /// <param name="Delay">Tiempo que pasa antes de que inicie el conteo.</param>
    /// <param name="Duration">Tiempo de duración en segundos.</param>
    /// <param name="OnTimesUp">Callback que se llamará al finalizar el conteo.</param>
    public TimerHandler(float Delay, float Duration, Action OnTimesUp)
    {
        //Un timer que inicie con un Delay, y luego se ejecute 1 sola vez.
        var _manager = TimeManager.instance;
        if (_manager != null)
            ID = _manager.CreateNewTimer(0, 1, Duration, OnTimesUp);
    }
    /// <summary>
    /// Crea un TimeHandler.
    /// </summary>
    /// <param name="Delay">Tiempo que pasa antes de que inicie el conteo.</param>
    /// <param name="repetitions">Cantidad de repeticiones.</param>
    /// <param name="Duration">Tiempo de Duración en segundos.</param>
    /// <param name="OnTimesUp">Callback que se llama al finalizar el conteo.</param>
    public TimerHandler(float Delay, int repetitions, float Duration, Action OnTimesUp)
    {
        //Un timer que inicie con un Delay, y luego se ejecute x cantidad de veces.
        var _manager = TimeManager.instance;
        if (_manager != null)
            ID = _manager.CreateNewTimer( Delay, repetitions, Duration, OnTimesUp);
    }

    public bool isPaused
    {
        get
        {
            TimeManager.Timer t = TimeManager.instance.getTimer(ID);
            if (t != null)
                return t.isPaused;

            return false;
        }
    }
    public bool isReady
    {
        get
        {
            TimeManager.Timer t = TimeManager.instance.getTimer(ID);
            if (t != null)
                return t.isReady;

            return false;
        }
    }
    public float RemainingTime
    {
        get
        {
            TimeManager.Timer t = TimeManager.instance.getTimer(ID);
            if (t != null)
            {
                float remainingTime = t.remainingTime;

                if (reverseCounting)
                    remainingTime = t.Time - t.remainingTime;

                return remainingTime;
            }

            return -1;
        }
    }

    public void setDelay(float startDelay)
    {
        TimeManager.Timer t = TimeManager.instance.getTimer(ID);
        if (t != null)
        {
            t.Delay = startDelay;
            t.remainingDelay = startDelay;
        }
    }
    public void setAsCountDownTimer()
    {
        reverseCounting = true;
    }
    public void setAsCountUpTimer()
    {
        reverseCounting = false;
    }

    public void Start()
    {
        TimeManager.instance.StartCount(ID);
    }
    public void Pause()
    {
        TimeManager.instance.getTimer(ID).isPaused = true;
    }
    public void Continue()
    {
        TimeManager.instance.getTimer(ID).isPaused = false;
    }
    public void Reset()
    {
        TimeManager.instance.getTimer(ID).reset();
    }
}
