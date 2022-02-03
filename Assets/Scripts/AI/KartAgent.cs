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
            if (raceManager.checkPoints!= null || raceManager.checkPoints[kart.tracker.nextCheckPointIndex] != null)
            {
                return transform.forward;
            }
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
    public bool SkipStartReset = false;

    #region Awake(), Start(), Update()
    private void Awake()
    {
        kart = GetComponent<ArcadeKart>();
        baseSpeed = kart.baseStats.TopSpeed;
        speedRange.x = Mathf.Clamp(speedRange.x, 0.5f, 1.1f);
        speedRange.y = Mathf.Clamp(speedRange.y, 0f, 0.5f);
        spawnPosition = transform.position;
        spawnRot = transform.rotation;
        if(!kart.TryGetComponent<UniversalInput>(out _))
            InvokeRepeating(nameof(KartStatMod), Random.Range(2.5f, 5f), Random.Range(2.5f, 5f));
    }

    private void Start()
    {
        kart.Rigidbody.isKinematic = true;
        if (kart.tracker != null)
        {
            kart.tracker.OnCorrectCheckPoint += OnCarCorrectCheck;
            kart.tracker.OnWrongCheckPoint += OnCarWrongCheck;
            kart.tracker.OnCircuitComplete += OnRaceEnd;
            kart.tracker.OnCircuitStart += OnRaceBegin;
            kart.tracker.OnLapComplete += OnLapComplete;
            kart.tracker.OnPlaceChanged += OnPlaceChanged;
        }
        kart.Rigidbody.isKinematic = false;
    }

    //float StationaryResetTime = 10f;
    //float StationaryTime = 0f;
    //float StationarylockOutTime = 5f;
    //private void Update()
    //{
    //    if(kart.LocalSpeed() < kart.baseStats.TopSpeed * 0.1f && StationarylockOutTime <= 0f)
    //    {
    //        StationaryTime += Time.deltaTime;
    //    }
    //    else
    //    {
    //        StationaryTime = 0f;
    //        StationarylockOutTime -= Time.deltaTime;
    //    }
    //    if(StationaryTime > StationaryResetTime)
    //    {
    //        AddReward(-4f);
    //        OnEpisodeBegin();
    //    }
    //    
    //}

    #endregion

    #region AI Events and Data Inputs
    #region AI rewards/penalities

    #region CollisionEnter/Stay
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            //Debug.Log("Bumped wall");
            AddReward(-0.6f);
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
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
        //AddReward(5f);
        //raceManager.EndAllEpisodes(true);
    }
    #endregion

    #region CheckPoints
    private int wrongCheckPoints = 0;
    public void OnCarCorrectCheck(ArcadeKart agent)
    {
        if (agent == kart)
        {
            wrongCheckPoints = 0;
            AddReward(0.5f);
        }
    }

    private void OnCarWrongCheck(ArcadeKart agent)
    {
        if (agent == kart)
        {
            wrongCheckPoints += 1;
            //Debug.Log("Incorrect Point");
            AddReward(-2f);
            if(wrongCheckPoints > 1)
            {
                raceManager.trackers[kart].FixCheckPoint();
                TurnKartAround();
            }
        }
    }

    private void TurnKartAround()
    {

        Debug.LogWarning("Turning AI Around");
        AddReward(-4f);
        kart.Rigidbody.isKinematic = true;
        transform.Rotate(new Vector3(0, 1f, 0), 180f, Space.Self);
        kart.Rigidbody.isKinematic = false;
    }
    #endregion

    public void OnPlaceChanged(ArcadeKart kart, int oldPlace, int newPlace)
    {
        placeCurrent = newPlace;
    }

    #endregion

    public override void OnEpisodeBegin()
    {
        
        if (SkipStartReset)
        {
            raceManager.ResetKart(kart);
            transform.SetPositionAndRotation(spawnPosition, spawnRot);
            //SkipStartReset = false;
        }
        
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
        placeLastObs = placeCurrent;
        AddReward(Mathf.InverseLerp(1, raceManager.KartCount, placeCurrent) * 0.002f);
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
        int playerPlace = (raceManager.playerKart == null) ? int.MaxValue : raceManager.playerKart.place;
        switch (ModSpeedToNoNet)
        {
            case false:
                float speed = baseSpeed;
                if(placeCurrent > playerPlace)
                {
                    bool slowDown = Random.Range(0,2) == 1 && !(playerPlace < placeCurrent);
                    speed *= slowDown ? speedRange.x : 1f + speedRange.y; //Random.Range(0.0001f, speedRange.y);
                }
                else
                {
                    speed *= speedRange.x;
                }

                stats.TopSpeed = speed;
                break;
            case true:
                stats.TopSpeed = baseSpeed;
                break;
        }

        kart.baseStats = stats;
    }
}