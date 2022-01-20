using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class KartAgent : Agent, IInput
{
    public RaceManager raceManager;
    public bool invertCheckpointForward = false;

    private float baseSpeed;
    public Vector2 speedRange = Vector2.zero;

    private ArcadeKart kart;
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

    private Vector3 spawnPosition;
    private Quaternion spawnRot;

    private float _acceleration;
    private float _steering;

    public float Acceleration => _acceleration;
    public float Steering => _steering;

    #region Awake(), Start(), Update()
    private void Awake()
    {
        kart = GetComponent<ArcadeKart>();
        baseSpeed = kart.baseStats.TopSpeed;
        speedRange.x = Mathf.Clamp(speedRange.x, 0.5f, 0.99f);
        speedRange.y = Mathf.Clamp(speedRange.y, 0f, 0.5f);
        spawnPosition = transform.position;
        spawnRot = transform.rotation;
        InvokeRepeating(nameof(KartStatMod), Random.Range(2.5f, 5f), Random.Range(5f, 7.5f));
    }

    private void Start()
    {
        kart.Rigidbody.isKinematic = true;
        kart.tracker.OnCorrectCheckPoint += OnCarCorrectCheck;
        kart.tracker.OnWrongCheckPoint += OnCarWrongCheck;
        kart.tracker.OnCircuitComplete += OnRaceEnd;
        kart.tracker.OnCircuitStart += OnRaceBegin;
        kart.tracker.OnLapComplete += OnLapComplete;
        kart.tracker.OnPlaceChanged += OnPlaceChanged;
        kart.Rigidbody.isKinematic = false;
    }

    private void Update()
    {
        Debug.DrawRay(raceManager.checkPoints[kart.tracker.nextCheckPointIndex].transform.position, NextChockPointFoward);
    }
    #endregion

    #region AI Events and Data Inputs
    #region AI rewards/penalities

    #region CollisionEnter/Stay
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            Debug.Log("Bumped wall");
            AddReward(-0.5f);
        }
        if (collision.transform.CompareTag("Kart"))
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
        if (collision.transform.CompareTag("Kart"))
        {
            AddReward(-0.1f);
        }
    }
    #endregion

    #region OnRaceBegin/End & Laps
    private void OnRaceBegin(ArcadeKart agent)
    {

    }

    private void OnLapComplete(ArcadeKart agent)
    {
        if (agent == kart)
        {
            AddReward(3f);
        }
    }

    public void OnRaceEnd(ArcadeKart agent)
    {
        AddReward(5f);
        EndEpisode();
    }
    #endregion

    #region CheckPoints
    public void OnCarCorrectCheck(ArcadeKart agent)
    {
        if (agent == kart)
        {
            AddReward(0.75f);
        }
    }

    private void OnCarWrongCheck(ArcadeKart agent)
    {
        if (agent == kart)
        {
            Debug.Log("Incorrect Point");
            AddReward(-1f);
        }
    }
    #endregion

    public void OnPlaceChanged(ArcadeKart kart, int oldPlace, int newPlace)
    {
        placeCurrent = newPlace;
    }

    #endregion

    public override void OnEpisodeBegin()
    {
        raceManager.ResetKart(kart);
        transform.SetPositionAndRotation(spawnPosition, spawnRot);
        Start();
    }

    int placeLastObs;
    int placeCurrent;

    public override void CollectObservations(VectorSensor sensor)
    {
        float directionDot = Vector3.Dot(transform.forward, NextChockPointFoward);
        sensor.AddObservation(directionDot);
        sensor.AddObservation(kart.LocalSpeed());
        AddReward(kart.LocalSpeed() * .001f);
        sensor.AddObservation(placeCurrent);
        if(placeLastObs == placeCurrent)
        {
            AddReward(0.1f);
        }
        else if(placeLastObs < placeCurrent)
        {
            AddReward(-0.5f);
        }
        else
        {
            AddReward(0.5f);
        }
        placeLastObs = placeCurrent;
    }
    #endregion

    #region Control outputs to kart & IInput
    public InputData GenerateInput()
    {
        return new InputData
        {
            Accelerate = Acceleration > 0,
            Brake = Acceleration < 0,
            TurnInput = Steering
        };
    }

    #region AI Input
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
    #endregion

    #region Human Input
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        switch (Input.GetAxis("Horizontal"))
        {
            case 0:
                discreteActions[0] = 1;
                break;
            case > 0:
                discreteActions[0] = 2;
                break;
            case < 0:
                discreteActions[0] = 0;
                break;
        }
        switch (Input.GetAxis("Vertical"))
        {
            case 0:
                discreteActions[1] = 1;
                break;
            case > 0:
                discreteActions[1] = 2;
                break;
            case < 0:
                discreteActions[1] = 0;
                break;
        }
    }
    #endregion

    #endregion

    private void KartStatMod()
    {
        ArcadeKart.Stats stats = kart.baseStats;
        bool ModSpeedToNoNet = Random.Range(1, 4) % 2 == 0;
        stats.TopSpeed = ModSpeedToNoNet switch
        {
            true => baseSpeed * (Random.Range(0,2) == 1? 1f + speedRange.y: speedRange.x),
            false => baseSpeed
        };
        //Debug.Log(kart.name+" "+baseSpeed + "|" + stats.TopSpeed);
        kart.baseStats = stats;
    }
}