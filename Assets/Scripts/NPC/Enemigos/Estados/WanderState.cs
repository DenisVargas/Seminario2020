using IA.FSM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PathFindSolver))]
public class WanderState : State
{
    public Action StopMovement = delegate { };

    [Header("Wander State")]
    [SerializeField] float _wanderMinDistance = 2f;
    [SerializeField] float _wanderMaxDistance = 10f;
    [SerializeField] float _minWanderTime = 2f;
    [SerializeField] float _maxWanderTime = 10f;

    float _wanderingTime = 0f;
    Vector3 _targetPosition;

    PathFindSolver _solver = null;

    #region DEBUG
#if UNITY_EDITOR
    [SerializeField] bool DEBUG_WanderStateRanges = true;
    [SerializeField] Color DEBUG_WanderRange_Min_GIZMOCOLOR = Color.blue;
    [SerializeField] Color DEBUG_WanderRange_Max_GIZMOCOLOR = Color.blue;
    private void OnDrawGizmos()
    {
        if (DEBUG_WanderStateRanges)
        {
            Gizmos.matrix = Matrix4x4.Scale(new Vector3(1, 0, 1));

            Gizmos.color = DEBUG_WanderRange_Min_GIZMOCOLOR;
            Gizmos.DrawWireSphere(transform.position, _wanderMinDistance);

            Gizmos.color = DEBUG_WanderRange_Max_GIZMOCOLOR;
            Gizmos.DrawWireSphere(transform.position, _wanderMaxDistance);
        }
    }
#endif 
    #endregion

    public override void Begin()
    {
        _solver = GetComponent<PathFindSolver>();
        _wanderingTime = 0;
    }

    public override void Execute()
    {
        _wanderingTime -= Time.deltaTime; // Reduzco un timer, cuando el tiempo llegue a 0. 
        //print($"Wandering Time is: {_wanderingTime}");
        if (_wanderingTime < 0)
        {
            //Debug.LogWarning("Terminó el tiempo del wandering");
            _targetPosition = getRandomPosition();

            _wanderingTime = UnityEngine.Random.Range(_minWanderTime, _maxWanderTime);

            _solver.SetOrigin(transform.position)
                   .SetTarget(_targetPosition)
                   .CalculatePathUsingSettings();
        }
    }

    Vector3 getRandomPosition()
    {
        //Calculo un target Random.
        var RandomTargetPosition = new Vector3(UnityEngine.Random.Range(0f, 1f),
                                                 0f,
                                                 UnityEngine.Random.Range(0f, 1f));
        float randomDistance = UnityEngine.Random.Range(_wanderMinDistance, _wanderMaxDistance);

        return (transform.position + (RandomTargetPosition * randomDistance));
    }
}
