using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class KartAgent : Agent, IInput
{
    VectorSensor sensors;
    ArcadeKart kart;
    public LayerMask raycastLayers;
    public float debugRaycastTime = 2f;
    public float raycastDistance = 10;
    public Transform[] raycasts;
    float _acceleration;
    public float Acceleration => _acceleration;

    float _steering;
    public float Steering => _steering;

    public float rewardOnCheckpoint = 1;

    Vector3 startingPos;
    Quaternion startingRot;

    private void Awake()
    {

        kart = GetComponent<ArcadeKart>();
        startingPos = transform.position;
        startingRot = transform.rotation;
        
    }

    public void InternalCollectObserverses()
    {
        sensors = new VectorSensor(raycasts.Length + 1);
        sensors.AddObservation(kart.LocalSpeed());
        for (int i = 0; i < raycasts.Length; i++)
        {
            AddRaycastVectorObs(raycasts[i]);
        }
        CollectObservations(sensors);
    }


    public void AgentReset()
    {
        kart.transform.position = startingPos;
        kart.transform.rotation = startingRot;
        kart.ForceMove(Vector3.zero, Quaternion.identity);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        base.OnActionReceived(actions);
        _acceleration = actions.ContinuousActions[0];
        if (_acceleration > 0) { _acceleration = 1; }
        _steering = actions.ContinuousActions[1];

        AddReward(kart.LocalSpeed()*0.001f);
    }

    void AddRaycastVectorObs(Transform ray)
    {
        RaycastHit hitInfo = new RaycastHit();
        var hit = Physics.Raycast(ray.position, ray.forward, out hitInfo, raycastDistance, raycastLayers.value, QueryTriggerInteraction.Ignore);
        var distance = hitInfo.distance;
        if (!hit) distance = raycastDistance;
        var obs = distance / raycastDistance;
        sensors.AddObservation(obs);

        if (distance < 1f)
        {
            this.EndEpisode();
            this.AgentReset();
        }
        Debug.DrawRay(ray.position, ray.forward * distance, Color.Lerp(Color.red, Color.green, obs), Time.deltaTime * debugRaycastTime);
    }

    public InputData GenerateInput()
    {
        return new InputData
        {
            Accelerate = Acceleration > 0,
            Brake = Acceleration < 0,
            TurnInput = Steering
        };
    }
}