using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IA.FSM
{
    public interface ITransition<T>
    {
        void registerOnTransitionEvent(Action<T> OnTransition);
        void unRegisterOnTransitionEvent(Action<T> OnTransition);
    }
}
