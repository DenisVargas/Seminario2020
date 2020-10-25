using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Timers
{
    public class TimeManager : MonoBehaviour
    {
        public static TimeManager _instance;
        public static TimeManager manager
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<TimeManager>();

                    if (_instance == null)
                        _instance = new GameObject("TimerManager").AddComponent<TimeManager>();

                    DontDestroyOnLoad(_instance);
                }

                return _instance;
            }
        }

        public enum TimerState
        {
            idle,
            running,
            paused
        }
        public class Timer
        {
            public TimerState state = TimerState.idle;

            public Action<int> OnTimerStart = delegate { };
            public Action<float> OnUpdate = delegate { };
            public Action<int, bool> OnTimesUp = delegate { };

            public float duration = 1f;
            public float completition = 0f; //Porcentaje de completado.
            public float remainingTime = 0f; //Tiempo que falta para el completado.

            public float Delay  = 0f;
            public float remainingDelay = 0;

            public int repetitions = 1;
            public int CurrentRepetititon = 1;

            public Timer(float Duration, int repetitions, float startDelay)
            {
                state = TimerState.idle;
                Delay = startDelay;
                remainingDelay = Delay;

                this.duration = Duration;
                remainingTime = Duration;

                this.repetitions = repetitions;
                CurrentRepetititon = 1;
            }
        }

        public static bool isReady(int iD)
        {
            if(manager.Timers.ContainsKey(iD))
                return manager.Timers[iD].state == TimerState.idle;

            return false;
        }
        public static bool isPaused(int iD)
        {
            var m = manager;
            if (m.Timers.ContainsKey(iD))
                return m.Timers[iD].state == TimerState.paused;

            return false;
        }
        public static float getTimerRemainingTime(int iD)
        {
            var m = manager;
            if (m.Timers.ContainsKey(iD))
                return m.Timers[iD].remainingTime;

            return -1;
        }

        public static void SetStartEvent(int iD, Action<int> onTimerStart)
        {
            var instance = manager;
            if (instance.Timers.ContainsKey(iD))
                instance.Timers[iD].OnTimerStart = onTimerStart;
        }
        public static void SetEndEvent(int iD, Action<int, bool> onTimesUp)
        {
            var instance = manager;
            if (instance.Timers.ContainsKey(iD))
                instance.Timers[iD].OnTimesUp = onTimesUp;
        }
        public static void SetUpdateEvent(int iD, Action<float> onTimeUpdate)
        {
            var instance = manager;
            if (instance.Timers.ContainsKey(iD))
                instance.Timers[iD].OnUpdate = onTimeUpdate;
        }

        int lastIndex = 0;
        internal Dictionary<int, Timer> Timers = new Dictionary<int, Timer>();

        //Cola de ejecución
        List<int> _activeTimers = new List<int>();
        List<int> toDispose = new List<int>();

        private void Update()
        {
            var m = manager;

            foreach (var id in _activeTimers)
            {
                Timer timer = null;
                if (m.Timers.ContainsKey(id))
                    timer = m.Timers[id];

                if (timer == null || timer.state == TimerState.paused || toDispose.Contains(id))
                    continue;

                float deltaTime = Time.deltaTime;

                if (timer.Delay > 0 && timer.remainingDelay > 0)
                {
                    timer.remainingDelay -= deltaTime;

                    if (timer.remainingDelay > 0)
                        continue;
                }

                if (timer.state == TimerState.idle)
                {
                    timer.OnTimerStart(timer.CurrentRepetititon);
                    timer.state = TimerState.running;
                }

                if (timer.state == TimerState.running)
                {
                    if (timer.remainingTime > 0)
                    {
                        timer.remainingTime -= deltaTime;

                        //Cálculo de completado.
                        float completitionAmmount = Mathf.Clamp(1 - (timer.remainingTime / timer.duration), 0, 1f);
                        timer.OnUpdate(completitionAmmount);
                    }
                }

                //Termino del ciclo.
                if (timer.remainingTime <= 0)
                {
                    if (timer.CurrentRepetititon < timer.repetitions)
                    {
                        timer.OnTimesUp(timer.CurrentRepetititon, false);
                        timer.CurrentRepetititon++;
                        timer.remainingTime = timer.duration;
                        timer.state = TimerState.idle;
                        timer.OnTimerStart(timer.CurrentRepetititon);
                    }
                    else
                    {
                        toDispose.Add(id);
                        ResetTimer(id);
                        timer.OnTimesUp(timer.CurrentRepetititon, true);
                        timer.state = TimerState.idle;
                    }
                }
            }

            foreach (var id in toDispose)
                _activeTimers.Remove(id);
            toDispose = new List<int>();
        }

        public static void SetStartDelay(int iD, float startDelay)
        {
            var m = manager;
            if (m.Timers.ContainsKey(iD))
                m.Timers[iD].Delay = startDelay;
        }
        public static void StartCount(int ID)
        {
            var m = manager;
            if (m.Timers.ContainsKey(ID))
            {
                Timer t = m.Timers[ID];
                m._activeTimers.Add(ID);
            }
        }
        public static void PauseTimer(int iD, bool value)
        {
            var m = manager;
            if (m.Timers.ContainsKey(iD))
            {
                Timer t = manager.Timers[iD];
                if (value)
                {
                    t.state = TimerState.paused;
                    if (m._activeTimers.Contains(iD))
                        m._activeTimers.Remove(iD);
                }
                else
                {
                    t.state = TimerState.running;
                    if (!m._activeTimers.Contains(iD))
                        m._activeTimers.Add(iD);
                }
            }
        }

        public static void DeleteTimer(int iD)
        {
            var m = manager;
            if (m.Timers.ContainsKey(iD))
            {
                m.Timers.Remove(iD);
                if (m._activeTimers.Contains(iD))
                    m._activeTimers.Remove(iD);
            }
        }
        public static void ResetTimer(int iD)
        {
            var m = manager;
            if(m.Timers.ContainsKey(iD))
            {
                var timer = m.Timers[iD];
                timer.state = TimerState.idle;
                timer.remainingDelay = timer.Delay;
                timer.remainingTime = timer.duration;
                timer.CurrentRepetititon = 1;

                if (m._activeTimers.Contains(iD))
                    m.toDispose.Add(iD);
            }
        }

        public static int CreateNewTimer(float Duration, int repetitions, float startDelay)
        {
            var m = manager;
            if (m != null)
            {
                int asignedID = m.lastIndex;
                Timer newTimer = new Timer(Duration, repetitions, startDelay);

                m.Timers.Add(_instance.lastIndex, newTimer);
                m.lastIndex++;

                return asignedID;
            }

            return -1;
        }
    }
}
