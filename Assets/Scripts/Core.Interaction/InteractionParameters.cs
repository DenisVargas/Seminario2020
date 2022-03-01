using IA.PathFinding;
using UnityEngine;

namespace Core.Interaction
{
    public struct InteractionParameters
    {
        public Node safeInteractionNode;
        public Vector3 orientation;
        public int AnimatorParameter;

        public InteractionParameters(Node safeInteractionNode, Vector3 orientation, int animatorParameter = 0)
        {
            this.safeInteractionNode = safeInteractionNode;
            this.orientation = orientation;
            AnimatorParameter = animatorParameter;
        }
    }
}
