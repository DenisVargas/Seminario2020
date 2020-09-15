using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Core.Interaction
{
    interface IDynamicInteractionComponent
    {
        Transform transform { get; }
        T GetComponent<T>();

        void InputConfirmed();
        void ExcecuteOperation();
        void CancelOperation();
    }
}
