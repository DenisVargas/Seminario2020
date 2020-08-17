using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.TimeManagement
{
    public class TimeManager : MonoBehaviour
    {
        public static TimeManager instance;

        public  class Timer
        {
            public Action OnTimesUp = delegate { };

            public bool isReady = true;
            public bool isPaused = false;

            public float Time = 1f;
            public float Delay  = 0f;

            int   _repetitions = 1;

            public int remainingRepetititons = 1;
            public float remainingDelay = 0;
            public float remainingTime = 1f;

            public Timer(float startDelay, int repetititions, float time, Action callback)
            {
                Delay = startDelay;
                remainingDelay = Delay;

                Time = time;
                remainingTime = Time;

                _repetitions = repetititions;
                remainingRepetititons = _repetitions;

                OnTimesUp = callback;
            }

            public void startRepetition()
            {
                remainingTime = Time;
                remainingRepetititons--;
            }

            public void reset()
            {
                isReady = true;
                isPaused = false;

                remainingDelay = Delay;
                remainingTime = Time;
                remainingRepetititons = _repetitions;
            }
        }

        int lastIndex = 0;
        internal Dictionary<int, Timer> Timers = new Dictionary<int, Timer>();

        //Cola de ejecución
        List<Timer> _executionList = new List<Timer>();

        private void Awake()
        {
            if (instance == null) instance = this;
            else Destroy(gameObject);
        }

        private void Update()
        {
            List<Timer> clearList = new List<Timer>();
            float deltaTime = Time.deltaTime;

            foreach (var timer in _executionList)
            {
                if (timer.isPaused)
                {
                    clearList.Add(timer);
                    continue;
                }

                if (timer.remainingDelay > 0)
                    timer.remainingDelay -= deltaTime;
                else
                {
                    if (timer.remainingDelay < 0)
                        timer.remainingDelay = 0;

                    timer.remainingTime -= deltaTime;

                    if (timer.remainingTime <= 0)
                    {
                        if (timer.remainingRepetititons > 0)
                        {
                            timer.startRepetition();
                            timer.OnTimesUp();
                        }
                        else
                        {
                            timer.reset();
                            continue;
                        }
                    }

                }

                clearList.Add(timer);
            }

            _executionList = clearList;
        }

        public void StartCount(int ID)
        {
            if (Timers.ContainsKey(ID))
            {
                Timers[ID].isReady = false;
                _executionList.Add(Timers[ID]);
            }
        }

        public Timer getTimer(int ID)
        {
            if (Timers.ContainsKey(ID))
                return Timers[ID];

            return null;
        }

        public int CreateNewTimer(float delay, int repetitions, float time, Action onTimesUp)
        {
            int asignedID = lastIndex;
            Timer newTimer = new Timer(delay, repetitions, time, onTimesUp);

            Timers.Add(lastIndex, newTimer);
            lastIndex++;

            return asignedID;
        }
    }
}

