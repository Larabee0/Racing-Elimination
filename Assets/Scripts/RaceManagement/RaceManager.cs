using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using PathCreation;

public class RaceManager : MonoBehaviour
{
    public VertexPath path;
    public GameObject checkPointContainer;
    public List<CheckPoint> checkPoints = new();
    public CheckPoint startFinishLine;
    public int indexOfFstartFinishLine;
    public int KartCount { get { return karts.Count; } }
    private List<ArcadeKart> karts;
    public Dictionary<ArcadeKart, KartTracker> trackers;
    [Range(1, 50)]
    public int Laps = 1;
    [Range(1f, 10f)]
    public float eliminationInterval = 5f;
    public bool reverseCheckPoints = false;
    public RaceUI ui;

    public ArcadeKart playerKart = null;

    public List<string> kartPlaceInfo = new();
    public List<ArcadeKart> kartPlaces = new();
    public GameModes gameMode;
    private IGameMode Game;
    public KartColourFactory kartColours;

    private float clockInternal;
    private float alerterClock;
    public float Clock { get { return clockInternal; } }
    private bool RunClock = false;
    private bool PluseGame = false;

    public void Awake()
    {
        Game = GameModeFactory.CreateGameMode(this, gameMode);
        playerKart = null;
        path = checkPointContainer.GetComponent<PathCreator>().path;
        List<CheckPoint> cPInternal = new(checkPointContainer.GetComponentsInChildren<CheckPoint>());
        if (reverseCheckPoints)
        {
            cPInternal.Reverse();
        }
        if (ui == null)
        {
            Debug.LogWarning("UI is null! No ui output will be made.");
        }
        else
        {
            ui.gameMode = gameMode;
        }

        cPInternal.ForEach(p => { startFinishLine = p.StartFinishLine ? p : startFinishLine; p.OnKartPassedCheckPoint += OnKartPassedCheckPoint; p.OnKartEnterCheckPoint += OnKartEnterCheckPoint; });
        int start = (cPInternal.IndexOf(startFinishLine) + 1) % cPInternal.Count;


        for (int i = start, count = 0; count < cPInternal.Count; count++)
        {
            checkPoints.Add(cPInternal[i]);
            cPInternal[i].index = count;
            i = (i + 1) % cPInternal.Count;
        }

        if (startFinishLine == null)
        {
            Debug.LogError("No start finish line!");
            enabled = false;
        }
        indexOfFstartFinishLine = cPInternal.IndexOf(startFinishLine);
        karts = new List<ArcadeKart>(FindObjectsOfType<ArcadeKart>());
        trackers = new Dictionary<ArcadeKart, KartTracker>(karts.Count);
        ActiveRacers = karts.Count;
        karts.ForEach(k =>
        {
            k.enabled = false;
            k.Rigidbody.isKinematic = true;
            if(playerKart == null && k.TryGetComponent(out UniversalInput universalInput))
            {
                playerKart = k;
            }
            else
            {
                k.GetComponent<KartAgent>().enabled = false;
            }
            //kartPos.Add(k.transform.position);
            kartPlaceInfo.Add(k.name);
            kartPlaces.Add(k);
            ResetKart(k);
        });
        if (playerKart == null) { Debug.LogWarning("No Player Kart"); }

        if (kartColours != null)
        {
            kartColours.PaintKarts(karts);
        }
    }

    public void StartRace()
    {
        for (int i = 0; i < karts.Count; i++)
        {
            if (karts[i] != playerKart)
            {
                karts[i].GetComponent<KartAgent>().enabled = true;
            }
            karts[i].Rigidbody.isKinematic = false;
            karts[i].enabled = true;
            
        }
        Game.Start();
    }

    private void Update()
    {
        if (RunClock)
        {
            clockInternal += Time.deltaTime;
            alerterClock +=Time.deltaTime;
            if (PluseGame && alerterClock > alertGameModeAfter)
            {
                alerterClock = 0;
                Game.Pluse();
            }
            if(gameMode == GameModes.Elimination)
            {
                ui.placeToElimination = ActiveRacers;
                ui.TimeTillElimination = eliminationInterval - alerterClock;
            }
        }
        DetermineFirstPlace();
    }

    float alertGameModeAfter = float.MaxValue;

    public void SetAlertGameModeInterval(float pingInterval)
    {
        alertGameModeAfter = pingInterval;
        PluseGame = true;
    }

    public void StopAlertGameMode()
    {
        alertGameModeAfter = float.MaxValue;
        PluseGame = false;
    }

    public void SetRunClock(bool stopped = false)
    {
        RunClock = !stopped;
    }

    public void ResetClock()
    {
        clockInternal = 0f;
    }

    public void ResetKart(ArcadeKart kart)
    {
        if (trackers.ContainsKey(kart))
        {
            trackers.Remove(kart);
        }
        trackers.Add(kart, new KartTracker(kart, Laps, checkPoints.Count, ActiveRacers,startFinishLine, startFinishLine, startFinishLine));
        if (ui != null && kart == playerKart)
        {
            trackers[kart].OnCircuitStart += OnKartStartCircuit;
            trackers[kart].OnCircuitComplete += OnKartFinishCircuit;
            trackers[kart].OnLapComplete += OnKartCompleteLap;
            trackers[kart].OnPlaceChanged += OnKartPlaceChanged;
        }
    }

    public void DetermineFirstPlace()
    {
        List<KartPositionInfo> kartPositions = new(karts.Count);
        for (int i = 0; i < karts.Count; i++)
        {
            KartTracker tracker = trackers[karts[i]];
            int placeCheckPointIndex = tracker.PlaceCheckPointIndex;
            int lapInternal = tracker.Placelap;
            float closestDst = tracker.GetClosestDistance(checkPoints[placeCheckPointIndex]);
            kartPositions.Add(new KartPositionInfo(i, lapInternal, placeCheckPointIndex, closestDst));

            kartPlaceInfo[i] = karts[i].gameObject.name + " Lap: " + lapInternal + " CP: " + placeCheckPointIndex + " Dst: " + closestDst.ToString();
        }
        kartPositions.Sort();
        kartPositions.Reverse();
        for (int i = 0; i < kartPositions.Count; i++)
        {
            trackers[karts[kartPositions[i].kartIndex]].Place = i + 1;
            kartPlaces[i] = karts[kartPositions[i].kartIndex];
        }
    }

    [HideInInspector]
    public int ActiveRacers = 0;

    public ArcadeKart GetLastPlace()
    {
        return kartPlaces[ActiveRacers - 1 >= 0 ? ActiveRacers - 1 : 0];
    }

    #region EventImplentations

    public void OnKartPassedCheckPoint(KartPassedCheckPointArgs f)
    {
        CheckPoint sender = f.sender;
        ArcadeKart kart = f.kart;
        KartTracker tracker = trackers[kart];
        tracker.HandleCheckPoint(sender);
    }

    public void OnKartEnterCheckPoint(KartPassedCheckPointArgs f)
    {
        CheckPoint sender = f.sender;
        ArcadeKart kart = f.kart;
        KartTracker tracker = trackers[kart];
        tracker.HandleCheckPointEnter(sender);
    }

    public void OnKartPlaceChanged(ArcadeKart kart, int oldPlace, int newPlace)
    {
        ui.Place = newPlace;
    }

    public void OnKartStartCircuit(ArcadeKart kart)
    {
        ui.HideFinished();
        ui.CurrentLap = trackers[kart].currentLap;
        ui.MaxLaps = Laps;
        ui.ResetTimer();
        ui.enabled = true;
    }

    public void OnKartCompleteLap(ArcadeKart kart)
    {
        ui.CurrentLap = trackers[kart].currentLap;
        ui.ResetTimer();
    }

    public void OnKartFinishCircuit(ArcadeKart kart)
    {
        ui.enabled = false;
        //ui.ShowFinished();
        ui.ResetTimer();
        ui.TotalTime();
    }

    public void EndAllEpisodes(bool endOfRace = false)
    {
        for (int i = 0; i < karts.Count; i++)
        {
            if (karts[i].TryGetComponent(out KartAgent agent))
            {
                if (endOfRace)
                {
                    agent.AddReward(karts.Count - karts[i].place);
                }
                agent.EndEpisode();
            }
        }
    }
    #endregion

    #region Events
    public delegate void LapComplete(ArcadeKart kart);
    public delegate void CircuitComplete(ArcadeKart kart);
    public delegate void CircuitStart(ArcadeKart kart);

    public delegate void CorrectCheckPoint(ArcadeKart kart);
    public delegate void WrongCheckPoint(ArcadeKart kart);
    public delegate void PlaceChanged(ArcadeKart kart, int oldPlace, int newPlace);
    #endregion

    public class KartTracker
    {
        public ArcadeKart kart;
        public CheckPoint startLine = null;
        public CheckPoint lastCheckPoint = null;
        public CheckPoint finishLineCheckPoint = null;
        public CheckPoint CurrentLapEndPoint;
        public int laps;
        public int currentLap = 0;
        public int nextCheckPointIndex;
        public int totalCheckpoints = 0;
        public CheckPoint RaceEndCheckPoint;
        public bool HasStartedRace { get { return lastCheckPoint != null; } }
        public bool HasFinishedRace { get { return HasStartedRace && finishLineCheckPoint != null; } }

        public LapComplete OnLapComplete;
        public CircuitComplete OnCircuitComplete;
        public CircuitStart OnCircuitStart;

        public CorrectCheckPoint OnCorrectCheckPoint;
        public WrongCheckPoint OnWrongCheckPoint;

        public PlaceChanged OnPlaceChanged;

        private int highestPlace = 10;
        private int place = -1;
        public int Place
        {
            set
            {
                if (place != value)
                {
                    int oldPlace = place;
                    place = value;
                    if (place > 1)
                    {
                        float inverseLerp = Mathf.InverseLerp(1f, highestPlace, place);
                        kart.kartSpeedMul = 1f + (inverseLerp * (kart.TryGetComponent<UniversalInput>(out _) ? 3f : 2f) / 10f);
                    }
                    OnPlaceChanged?.Invoke(kart, oldPlace, place);
                    kart.place = value;
                }
            }
            get { return place; }
        }

        public int PlaceCheckPointIndex;
        public int Placelap;

        public float GetClosestDistance(CheckPoint checkPoint)
        {
            Vector3 kartPos = kart.transform.position;
            kartPos.y = 0;
            float[] distances = new float[checkPoint.points.Count];
            for (int i = 0; i < distances.Length; i++)
            {
                distances[i] =Mathf.Abs( Vector3.Distance(kartPos, checkPoint.points[i]));
            }

            return Mathf.Min(distances);
        }

        public KartTracker(ArcadeKart kart, int laps, int totalCheckPoints,int highestPlace, CheckPoint startLine, CheckPoint finishLine, CheckPoint LapEndPoint)
        {
            this.kart = kart;
            totalCheckpoints = totalCheckPoints;
            this.highestPlace = highestPlace;
            this.laps = laps;
            this.startLine = startLine;
            RaceEndCheckPoint = finishLine;
            CurrentLapEndPoint = LapEndPoint;
            PlaceCheckPointIndex = nextCheckPointIndex = startLine.index;
            kart.tracker = this;
        }

        public void HandleCheckPointEnter(CheckPoint checkPoint)
        {
            if (checkPoint.index == PlaceCheckPointIndex)
            {
                PlaceCheckPointIndex = (PlaceCheckPointIndex + 1) % totalCheckpoints;
                if (HasStartedRace)
                {
                    if (checkPoint == CurrentLapEndPoint)
                    {
                        Placelap++;
                    }
                }
                else
                {
                    if (checkPoint == startLine)
                    {
                        Placelap = 1;
                    }
                }
            }
        }

        public void HandleCheckPoint(CheckPoint checkPoint)
        {
            if (checkPoint.index == nextCheckPointIndex)
            {
                //Debug.Log("Correct Point");
                OnCorrectCheckPoint?.Invoke(kart);
                nextCheckPointIndex = (nextCheckPointIndex + 1) % totalCheckpoints;

                if (HasStartedRace)
                {
                    if (HasFinishedRace)
                    {
                        return;
                    }
                    else
                    {
                        if (checkPoint == CurrentLapEndPoint)
                        {
                            currentLap++;
                            if (currentLap > laps)
                            {
                                finishLineCheckPoint = checkPoint;
                                //Debug.Log("Finished Race");
                                OnCircuitComplete?.Invoke(kart);
                                return;
                            }
                            else
                            {
                                CurrentLapEndPoint = checkPoint;
                                OnLapComplete?.Invoke(kart);
                                //Debug.Log("Next Lap: " + (currentLap));
                            }
                        }
                    }
                    lastCheckPoint = checkPoint;
                }
                else
                {
                    if (checkPoint == startLine)
                    {
                        lastCheckPoint = checkPoint;
                        currentLap = 1;
                        OnCircuitStart?.Invoke(kart);
                        //Debug.Log("started Race");
                    }
                }
            }
            else
            {
                // wrong checkpoint passed.
                if (nextCheckPointIndex == 0 && checkPoint.index + 1 == totalCheckpoints)
                {

                }
                else if (checkPoint.index < nextCheckPointIndex)
                {

                }
                else
                {
                }
                if (HasStartedRace)
                {
                    lastWrongCheckPoint = checkPoint;
                    OnWrongCheckPoint?.Invoke(kart);
                }
            }
        }
        private CheckPoint lastWrongCheckPoint;
        public void FixCheckPoint()
        {
            nextCheckPointIndex = lastWrongCheckPoint.index;
        }
    }

    private class KartPositionInfo : IEquatable<KartPositionInfo>, IComparable<KartPositionInfo>
    {
        public int kartIndex;
        public int Lap;
        public int lastCheckPoint;
        public float DistanceToNextCheckPoint;

        public KartPositionInfo(int kartIndex, int lap, int lastCheckPoint, float distanceToNextCheckPoint)
        {
            this.kartIndex = kartIndex;
            Lap = lap;
            this.lastCheckPoint = lastCheckPoint;
            DistanceToNextCheckPoint = distanceToNextCheckPoint;
        }

        public bool Equals(KartPositionInfo other)
        {
            return other.kartIndex == this.kartIndex;
        }

        public int CompareTo(KartPositionInfo other)
        {
            if (other == null)
            {
                return 1;
            }
            else
            {
                if (Lap > other.Lap)
                {
                    return 1;
                }
                else if (Lap < other.Lap)
                {
                    return -1;
                }
                else
                {
                    if (lastCheckPoint > other.lastCheckPoint)
                    {
                        return 1;
                    }
                    else if (lastCheckPoint < other.lastCheckPoint)
                    {
                        return -1;
                    }
                    else
                    {
                        if (DistanceToNextCheckPoint > other.DistanceToNextCheckPoint)
                        {
                            return -1;
                        }
                        else if (DistanceToNextCheckPoint < other.DistanceToNextCheckPoint)
                        {
                            return 1;
                        }
                        else
                        {
                            return 0;
                        }
                    }
                }
            }
        }
    }

}
