using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class KartAgent : Agent, IInput
{
    public RaceManager raceManager;
    public Transform spawnPosition;
    public Vector2 randomRange;
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
    }

    private void Start()
    {
        kart.Rigidbody.isKinematic = true;
        kart.tracker.OnCorrectCheckPoint += OnCarCorrectCheck;
        kart.tracker.OnWrongCheckPoint += OnCarWrongCheck;
        kart.tracker.OnRaceComplete += OnRaceEnd;
        kart.tracker.OnRaceStart += OnRaceBegin;
        kart.Rigidbody.isKinematic = false;
    }

    private void OnRaceBegin(ArcadeKart agent)
    {
        runTimer = true;
        lastTimeSinceLastCheckpoint = timeSinceLastCheckpoint = 0;
    }
    private void Update()
    {
        if (runTimer)
        {
            timeSinceLastCheckpoint += Time.deltaTime;
        }
    }

    private void OnCarWrongCheck(ArcadeKart agent)
    {
        if(agent == kart)
        {
            AddReward(-4f);
        }
    }

    public void OnCarCorrectCheck(ArcadeKart agent)
    {
        if (agent == kart)
        {
            Debug.Log("Time since last check point: " + timeSinceLastCheckpoint);
            float scoreMul = Mathf.Clamp(1 - Mathf.Clamp01(timeSinceLastCheckpoint), 0.001f, 1);
            AddReward(2f* scoreMul);
            AddReward(1f);
            lastTimeSinceLastCheckpoint = timeSinceLastCheckpoint;
            timeSinceLastCheckpoint = 0f;
        }
    }
    public void OnRaceEnd(ArcadeKart agent)
    {
        runTimer = false;
        Debug.Log("Time since last check point: " + timeSinceLastCheckpoint);
        lastTimeSinceLastCheckpoint = timeSinceLastCheckpoint;
        timeSinceLastCheckpoint = 0f;
        EndEpisode();
    }
    public override void OnEpisodeBegin()
    {
        transform.position = spawnPosition.position + new Vector3(Random.Range(randomRange.x, randomRange.y), 0, Random.Range(-2f, 2f));
        transform.forward = spawnPosition.forward;
        raceManager.ResetKart(kart);
        Start();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 checkPointForwrd = raceManager.checkPoints[kart.tracker.nextCheckPointIndex].transform.forward;
        float directionDot = Vector3.Dot(transform.forward, checkPointForwrd);
        sensor.AddObservation(directionDot);
        sensor.AddObservation(lastTimeSinceLastCheckpoint);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        _steering = actions.ContinuousActions[1];
        _acceleration = actions.ContinuousActions[0];
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continousActions = actionsOut.ContinuousActions;
        continousActions[0] = Input.GetAxis("Vertical");
        continousActions[1] = Input.GetAxis("Horizontal");
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            AddReward(-2f);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            AddReward(-0.25f);
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