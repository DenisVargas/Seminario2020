namespace Core.Timers
{
    [System.Serializable]
    public class TimerHandler
    {
        int ID = -1;
        public float Duration;
        public int Repetitions;
        public float StartDelay;

        /// <summary>
        /// Crea un Default Time Handler.
        /// </summary>
        public TimerHandler()
        {
            ID = -1;
            Duration = 1f;
            Repetitions = 1;
            StartDelay = 0;
        }
        /// <summary>
        /// Crea un Time Handler.
        /// </summary>
        /// <param name="startingData">Los Settings iniciales del Timer.</param>
        public TimerHandler(float Duration, int repetitions, float startDelay)
        {
            //Single Common timer.
            ID = TimeManager.CreateNewTimer(Duration, repetitions, StartDelay);
        }

        public bool isPaused
        {
            get
            {
                if(ID == -1)
                    ID = TimeManager.CreateNewTimer(Duration, Repetitions, StartDelay);

                return TimeManager.isPaused(ID);
            }
        }
        public bool isReady
        {
            get
            {
                if(ID == -1)
                    ID = TimeManager.CreateNewTimer(Duration, Repetitions, StartDelay);

                return TimeManager.isReady(ID);
            }
        }
        public float RemainingTime
        {
            get
            {
                if (ID == -1)
                    ID = TimeManager.CreateNewTimer(Duration, Repetitions, StartDelay);

                return TimeManager.getTimerRemainingTime(ID);
            }
        }

        public TimerHandler setDelay(float startDelay)
        {
            if (ID == -1)
                ID = TimeManager.CreateNewTimer(Duration, Repetitions, StartDelay);
            TimeManager.SetStartDelay(ID, startDelay);
            return this;
        }
        public TimerHandler setOnTimeStart(System.Action<int> OnTimerStart)
        {
            if (ID == -1)
                ID = TimeManager.CreateNewTimer(Duration, Repetitions, StartDelay);

            TimeManager.SetStartEvent(ID, OnTimerStart);
            return this;
        }
        public TimerHandler setOnTimeUpdate(System.Action<float> OnTimeUpdate)
        {
            if (ID == -1)
                ID = TimeManager.CreateNewTimer(Duration, Repetitions, StartDelay);

            TimeManager.SetUpdateEvent(ID, OnTimeUpdate);
            return this;
        }
        public TimerHandler setOnTimesUp(System.Action<int, bool> OnTimesUp)
        {
            if (ID == -1)
                ID = TimeManager.CreateNewTimer(Duration, Repetitions, StartDelay);

            TimeManager.SetEndEvent(ID, OnTimesUp);
            return this;
        }

        public void Start()
        {
            if(ID == -1)
                ID = TimeManager.CreateNewTimer(Duration, Repetitions, StartDelay);

            TimeManager.StartCount(ID);
        }
        public void Pause()
        {
            if(ID == -1)
                ID = TimeManager.CreateNewTimer(Duration, Repetitions, StartDelay);

            TimeManager.PauseTimer(ID, true);
        }
        public void Continue()
        {
            if(ID == -1)
                ID = TimeManager.CreateNewTimer(Duration, Repetitions, StartDelay);

            TimeManager.PauseTimer(ID, false);
        }
        public void Reset()
        {
            if(ID == -1)
                ID = TimeManager.CreateNewTimer(Duration, Repetitions, StartDelay);

            TimeManager.ResetTimer(ID);
        }
        public void Delete()
        {
            if(ID != -1)
                TimeManager.DeleteTimer(ID);
        }
    }
}
