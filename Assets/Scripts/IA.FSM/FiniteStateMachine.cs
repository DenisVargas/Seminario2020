using System;
using System.Collections.Generic;
using UnityEngine;

namespace IA.FSM
{
    [Serializable]
    public class FiniteStateMachine<T>
    {
        public IState<T> currentState;
        public T CurrentStateType => currentState.StateType;

        [SerializeField] Dictionary<T, IState<T>> States = new Dictionary<T, IState<T>>();

        public FiniteStateMachine()
        {
            currentState = null;
            States = new Dictionary<T, IState<T>>();
        }
        public void Update()
        {
            currentState.Execute();
        }
        public void Feed(T input)
        {
            currentState = currentState.transitionTo(input);
        }
        public void Feed(T input, T fallback)
        {
            currentState = currentState.transitionTo(input);
            if (currentState == null)
                SetState(fallback);
        }
        public void SetState(T input)
        {
            if(States.ContainsKey(input))
            {
                currentState = States[input];
                currentState.Begin();
            }
        }
        public void AddState(IState<T> state)
        {
            if (States.ContainsKey(state.StateType))
                States[state.StateType] = state;
            else
                States.Add(state.StateType, state);
        }
    }
}
