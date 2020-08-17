using System;

namespace IA.FSM
{
    public interface IState<T>
    {
        T getStateType { get; }
        void Begin();
        void Execute();
        void End();
        IState<T> transitionTo(T input);
        IState<T> AddTransition(IState<T> targetState);
        IState<T> AddTransition(IState<T> targetState, Action<T> onTransition);
    }
}