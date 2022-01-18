using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class KartAgent : Agent, IInput
{
    public RaceManager raceManager;
    private Vector3 spawnPosition;
    private Quaternion spawnRot;
    public bool invertCheckpointForward = false;
    private Vector3 NextChockPointFoward
    {
        get
        {
            return invertCheckpointForward switch
            {
                true => -raceManager.checkPoints[kart.tracker.nextCheckPointIndex].transform.forward,
                false => raceManager.checkPoints[kart.tracker.nextCheckPointIndex].transform.forward
            };
        }
    }
    ArcadeKart kart;
    float _acceleration;
    public float Acceleration => _acceleration;

    float _steering;
    public float Steering => _steering;

    public float rewardOnCheckpoint = 1;
    [SerializeField] private float timeSinceLastCheckpoint = 0f;
    [SerializeField] private float lastTimeSinceLastCheckpoint = 0f;
    private bool runTimer = false;

    private void Awake()
    {
        kart = GetComponent<ArcadeKart>();
        spawnPosition = transform.position;
        spawnRot = transform.rotation;
    }

    private void Start()
    {
        kart.Rigidbody.isKinematic = true;
        kart.tracker.OnCorrectCheckPoint += OnCarCorrectCheck;
        kart.tracker.OnWrongCheckPoint += OnCarWrongCheck;
        kart.tracker.OnRaceComplete += OnRaceEnd;
        kart.tracker.OnRaceStart += OnRaceBegin;
        kart.tracker.OnLapComplete += OnLapComplete;
        kart.Rigidbody.isKinematic = false;
    }

    private void OnLapComplete(ArcadeKart agent)
    {
        if (agent == kart)
        {
            AddReward(3f);
        }
    }

    private void OnRaceBegin(ArcadeKart agent)
    {
        runTimer = true;
        lastTimeSinceLastCheckpoint = timeSinceLastCheckpoint = 0;
    }
    private void Update()
    {
        Debug.DrawRay(raceManager.checkPoints[kart.tracker.nextCheckPointIndex].transform.position, NextChockPointFoward);
        if (runTimer)
        {
            timeSinceLastCheckpoint += Time.deltaTime;
        }
    }

    private void OnCarWrongCheck(ArcadeKart agent)
    {
        if(agent == kart)
        {
            Debug.Log("Incorrect Point");
            AddReward(-1f);
        }
    }

    public void OnCarCorrectCheck(ArcadeKart agent)
    {
        if (agent == kart)
        {
            float scoreMul = Mathf.Clamp(1 - Mathf.Clamp01(timeSinceLastCheckpoint), 0.001f, 1);
            //AddReward(2f* scoreMul);
            float delta = Mathf.Abs(lastTimeSinceLastCheckpoint - timeSinceLastCheckpoint);
            //delta *= 10f;
            delta = 1f - Mathf.Clamp(delta, 0, 0.8f);
            //Debug.Log("Reward: " + (0.2f * delta));
            AddReward(0.75f);
            lastTimeSinceLastCheckpoint = timeSinceLastCheckpoint;
            timeSinceLastCheckpoint = 0f;
        }
    }
    public void OnRaceEnd(ArcadeKart agent)
    {
        runTimer = false;
        lastTimeSinceLastCheckpoint = timeSinceLastCheckpoint;
        timeSinceLastCheckpoint = 0f;
        AddReward(5f);
        EndEpisode();
    }
    public override void OnEpisodeBegin()
    {
        raceManager.ResetKart(kart);
        transform.position = spawnPosition;
        transform.rotation = spawnRot;
        Start();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        float directionDot = Vector3.Dot(transform.forward, NextChockPointFoward);
        sensor.AddObservation(directionDot);
        sensor.AddObservation(kart.LocalSpeed());
        AddReward(kart.LocalSpeed() * .001f);
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        ActionSegment<int> discreteActions = actions.DiscreteActions;
        switch (discreteActions[0])
        {
            case 0:
                _steering = -1f;
                break;
            case 1:
                _steering = 0f;
                break;
            case 2:
                _steering = 1f;
                break;
        }
        switch (discreteActions[1])
        {
            case 0:
                _acceleration = -1f;
                break;
            case 1:
                _acceleration = 0f;
                break;
            case 2:
                _acceleration = 1f;
                break;
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        switch (Input.GetAxis("Horizontal"))
        {
            case 0:
                discreteActions[0] = 1;
                break;
                case >0:
                discreteActions[0] = 2;
                break;
                case <0:
                discreteActions[0] = 0;
                break;
        }
        switch (Input.GetAxis("Vertical"))
        {
            case 0:
                discreteActions[1] = 1;
                break;
            case >0:
                discreteActions[1] = 2;
                break;
            case <0:
                discreteActions[1] = 0;
                break;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("Bumped wall");
            AddReward(-0.5f);
        }
        if (collision.gameObject.CompareTag("Kart"))
        {
            Debug.Log("Bumped Kart");
            AddReward(-0.4f);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            AddReward(-0.1f);
        }
        if (collision.gameObject.CompareTag("Kart"))
        {
            AddReward(-0.1f);
        }
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