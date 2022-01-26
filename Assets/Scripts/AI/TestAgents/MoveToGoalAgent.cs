using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;


// D:\GitHub\Racing-Elimination
// venv\Scripts\activate
// mlagents-learn --torch-device "cpu" --force (or --resume | run-id="IDNAME")
public class MoveToGoalAgent : Agent
{
    [SerializeField] private Transform targetTransform;
    readonly float moveSpeed = 1f;
    Vector3 startState;
    private void Awake()
    {
        startState = transform.localPosition;
    }
    public override void OnEpisodeBegin()
    {
        transform.localPosition = startState;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(targetTransform.localPosition);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        transform.localPosition += moveSpeed * Time.deltaTime * new Vector3(moveX, 0, moveZ);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Goal"))
        {
            SetReward(1f);
        }
        else
        {
            SetReward(-1f);
        }
        EndEpisode();
    }
}
