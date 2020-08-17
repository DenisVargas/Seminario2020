using System;
using System.Collections.Generic;
using UnityEngine;

namespace IA.FSM
{
    [Serializable]
    public abstract class State : MonoBehaviour, IState<CommonState>
    {
        [SerializeField] protected Animator _anims;
        [SerializeField]
        protected CommonState stateType;
        [SerializeField]
        protected Dictionary<CommonState, Transition> transitions = new Dictionary<CommonState, Transition>();

        public CommonState getStateType
        {
            get => stateType;
            private set => stateType = value;
        }

        public virtual void Begin() { }
        public virtual void Execute() { }
        public virtual void End() { }

        public IState<CommonState> AddTransition(IState<CommonState> targetState)
        {
            Transition transition = new Transition(targetState, (stateType) => { });
            transitions.Add(targetState.getStateType, transition);
            return this;
        }
        public IState<CommonState> AddTransition(IState<CommonState> targetState, Action<CommonState> OnTransition)
        {
            Transition transition = new Transition(targetState, OnTransition);
            transitions.Add(targetState.getStateType, transition);
            return this;
        }
        public IState<CommonState> transitionTo(CommonState input)
        {
            if (transitions.ContainsKey(input))
            {
                var transition = transitions[input];
                End();
                transition.OnTransition(input);
                transition.target.Begin();
                return transition.target;
            }
            else return this;
        }

        public State SetAnimator(Animator anim)
        {
            _anims = anim;
            return this;
        }
    }

    public static class StateExtensions
    {
        public static State AttachTo(this State state, FiniteStateMachine<CommonState> FSM, bool setAsDefault = false)
        {
            FSM.AddState(state);

            if (setAsDefault)
                FSM.SetState(state.getStateType);

            return state;
        }
    }

    [Serializable]
    public class Transition : ITransition<CommonState>
    {
        public IState<CommonState> target;
        public Action<CommonState> OnTransition = delegate { };

        public Transition(IState<CommonState> targetState, Action<CommonState> OnTransition)
        {
            target = targetState;
            this.OnTransition += OnTransition;
        }
        public void registerOnTransitionEvent(Action<CommonState> OnTransition)
        {
            this.OnTransition += OnTransition;
        }
        public void unRegisterOnTransitionEvent(Action<CommonState> OnTransition)
        {
            this.OnTransition -= OnTransition;
        }
    }
}
