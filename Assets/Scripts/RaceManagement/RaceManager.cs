using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using PathCreation;

public class RaceManager : MonoBehaviour
{
    public VertexPath path;
    public GameObject checkPointContainer;
    public List<CheckPoint> checkPoints = new List<CheckPoint>();
    public CheckPoint startFinishLine;
    public int indexOfFstartFinishLine;
    private List<ArcadeKart> karts;
    public Dictionary<ArcadeKart, KartTracker> trackers;
    [Range(1,50)]
    public int Laps = 1;
    public bool reverseCheckPoints = false;
    public TimerLapCounter ui;

    public void Awake()
    {
        path = checkPointContainer.GetComponent<PathCreator>().path;
        List< CheckPoint> cPInternal = new List<CheckPoint>(checkPointContainer.GetComponentsInChildren<CheckPoint>());
        if (reverseCheckPoints)
        {
            cPInternal.Reverse();
        }
        if (ui == null)
        {
            Debug.LogWarning("UI is null! No ui output will be made.");
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
            Debug.LogError("no start finish line.");
            enabled = false;
        }
        indexOfFstartFinishLine = cPInternal.IndexOf(startFinishLine);
        karts = new List<ArcadeKart>(FindObjectsOfType<ArcadeKart>());
        trackers = new Dictionary<ArcadeKart, KartTracker>(karts.Count);
        karts.ForEach(k =>
        {
            kartPos.Add(k.transform.position);
            kartTimes.Add(k.name);
            kartPlaces.Add(k.name);
            ResetKart(k);
        });
        startFinishLineTime = path.GetClosestDistanceAlongPath(path.GetClosestPointOnPath(startFinishLine.transform.position)) +2f;
        Debug.Log("Start finish distance: " + startFinishLineTime);
    }

    private void Update()
    {
        DetermineFirstPlaceV2();
    }

    public void ResetKart(ArcadeKart kart)
    {
        if (trackers.ContainsKey(kart))
        {
            trackers.Remove(kart);
        }
        trackers.Add(kart, new KartTracker(kart, Laps, checkPoints.Count, startFinishLine, startFinishLine, startFinishLine));
        if (ui != null)
        {
            trackers[kart].OnCircuitStart += OnKartStartCircuit;
            trackers[kart].OnCircuitComplete += OnKartFinishCircuit;
            trackers[kart].OnLapComplete += OnKartCompleteLap;
            trackers[kart].OnPlaceChanged+=OnKartPlaceChanged;
        }
    }

    public int firstPlacelapNumber = 0;
    public ArcadeKart firstPlaceKart;
    public CheckPoint firstPlaceLastCheckPoint;
    public List<string> kartTimes = new List<string>();
    public List<string> kartPlaces = new List<string>();
    List<Vector3> kartPos = new List<Vector3>();
    float startFinishLineTime;
    public void DetermineFirstPlace()
    {
        List<KartPositionInfo> kartPositions = new List<KartPositionInfo>(karts.Count);
        for (int i = 0; i < karts.Count; i++)
        {
            //kartPos[i] = path.GetClosestPointOnPath(karts[i].transform.position);
            float timeAbs = path.GetClosestDistanceAlongPath(karts[i].transform.position);
            float time = timeAbs;
            if (timeAbs < startFinishLineTime)
            {
                time += startFinishLineTime;
            }
            else // greater or equal
            {
                time -= startFinishLineTime;
            }
            kartPositions.Add(new KartPositionInfo(i, trackers[karts[i]].currentLap,  time));
            //kartTimes[i] = karts[i].gameObject.name +" lap: " + trackers[karts[i]].currentLap + " distance: " + time.ToString();
            
        }
        kartPositions.Sort();
        kartPositions.Reverse();
        for (int i = 0; i < kartPositions.Count; i++)
        {
            trackers[karts[kartPositions[i].kartIndex]].Place = i + 1;
            //kartPlaces[i] = karts[kartPositions[i].kartIndex].name;

        }
    }

    private class KartPositionInfo : IEquatable<KartPositionInfo>, IComparable<KartPositionInfo>
    {
        public int kartIndex;
        public int Lap;
        public float timeOnPath;

        public KartPositionInfo(int kartIndex, int lap, float time)
        {
            this.kartIndex = kartIndex;
            Lap = lap;
            this.timeOnPath = time;
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
                if(Lap > other.Lap)
                {
                    return 1;
                }
                else if(Lap < other.Lap)
                {
                    return -1;
                }
                else
                {
                    if (timeOnPath > other.timeOnPath)
                    {
                        return 1;
                    }
                    else if (timeOnPath < other.timeOnPath)
                    {
                        return -1;
                    }
                    else
                    {
                        return 0;
                    }
                }
            }
        }
    }

    public void DetermineFirstPlaceV2()
    {
        List<KartPositionInfoV2> kartPositions = new List<KartPositionInfoV2>(karts.Count);
        for (int i = 0; i < karts.Count; i++)
        {
            KartTracker tracker = trackers[karts[i]];
            int placeCheckPointIndex = tracker.PlaceCheckPointIndex;
            int lapInternal = tracker.Placelap;
            float closestDst = tracker.GetClosestDistance(checkPoints[placeCheckPointIndex]);
            kartPositions.Add(new KartPositionInfoV2(i, lapInternal, placeCheckPointIndex, closestDst));

            kartTimes[i] = karts[i].gameObject.name +" Lap: " + lapInternal + " CP: "+ placeCheckPointIndex + " Dst: " + closestDst.ToString();
        }
        kartPositions.Sort();
        kartPositions.Reverse();
        for (int i = 0; i < kartPositions.Count; i++)
        {
            trackers[karts[kartPositions[i].kartIndex]].Place = i + 1;
            kartPlaces[i] = karts[kartPositions[i].kartIndex].name;

        }
    }

    private class KartPositionInfoV2 : IEquatable<KartPositionInfoV2>, IComparable<KartPositionInfoV2>
    {
        public int kartIndex;
        public int Lap;
        public int lastCheckPoint;
        public float DistanceToNextCheckPoint;

        public KartPositionInfoV2(int kartIndex, int lap, int lastCheckPoint, float distanceToNextCheckPoint)
        {
            this.kartIndex = kartIndex;
            Lap = lap;
            this.lastCheckPoint = lastCheckPoint;
            DistanceToNextCheckPoint = distanceToNextCheckPoint;
        }

        public bool Equals(KartPositionInfoV2 other)
        {
            return other.kartIndex == this.kartIndex;
        }

        public int CompareTo(KartPositionInfoV2 other)
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

    #region EventImplentations

    public void OnKartPlaceChanged(ArcadeKart kart, int oldPlace, int newPlace)
    {
        if(oldPlace > newPlace)
        {
            Debug.Log(kart.name + " moved up to " + newPlace + " place from " + oldPlace + " place.");
        }
        else
        {
            Debug.Log(kart.name + " moved down to " + newPlace + " place from " + oldPlace + " place.");
        }
        if(kart.name == "Red")
        {
            ui.Place = newPlace;
        }
    }

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
        ui.ShowFinished();
        ui.ResetTimer();
        ui.TotalTime();
    }
    #endregion

    #region Events
    public delegate void LapComplete(ArcadeKart kart);
    public delegate void CircuitComplete(ArcadeKart kart);
    public delegate void CircuitStart(ArcadeKart kart);

    public delegate void CorrectCheckPoint(ArcadeKart kart);
    public delegate void WrongCheckPoint(ArcadeKart kart);
    public delegate void PlaceChanged(ArcadeKart kart,int oldPlace, int newPlace);
    #endregion

    public class KartTracker
    {
        public ArcadeKart kart;
        public CheckPoint startLine = null;
        public CheckPoint lastCheckPoint = null;
        public CheckPoint finishLineCheckPoint = null;
        public CheckPoint CurrentLapEndPoint;
        public int laps;
        private int place = -1;
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

        public float GetClosestDistance(CheckPoint checkPoint)
        {
            Vector3 kartPos = kart.transform.position;
            Vector3 checkCentre = checkPoint.CentrePos;
            Vector3 checkLeft = checkPoint.RightPos;
            Vector3 checkRight = checkPoint.LeftPos;
            checkCentre.y = checkRight.y = checkLeft.y = kartPos.y = 0;
            Vector3 CentreLeft = (checkCentre + checkLeft) / 2f;
            Vector3 CentreRight = (checkCentre + checkLeft) / 2f;


            return Mathf.Min(
                Vector3.Distance(kartPos, checkCentre), 
                Vector3.Distance(kartPos, checkLeft), 
                Vector3.Distance(kartPos, checkRight),
                Vector3.Distance(kartPos, CentreLeft),
                Vector3.Distance(kartPos, CentreRight));
        }

        public KartTracker(ArcadeKart kart, int laps, int totalCheckPoints, CheckPoint startLine, CheckPoint finishLine, CheckPoint LapEndPoint)
        {
            this.kart = kart;
            this.totalCheckpoints = totalCheckPoints;
            this.laps = laps;
            this.startLine = startLine;
            RaceEndCheckPoint = finishLine;
            CurrentLapEndPoint = LapEndPoint;
            PlaceCheckPointIndex = nextCheckPointIndex = startLine.index;
            kart.tracker = this;
        }

        public int Place
        {
            set
            {
                if (place != value)
                {
                    int oldPlace = place;
                    place = value;
                    if (place > 0)
                    {
                        kart.kartSpeedMul = 1f + (value / 100f);
                        OnPlaceChanged?.Invoke(kart,oldPlace, place);
                    }
                    kart.place = value;
                }
            }
            get { return place; }
        }

        public int PlaceCheckPointIndex;
        public int Placelap;

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
                Debug.Log("Correct Point");
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
                                Debug.Log("Finished Race");
                                OnCircuitComplete?.Invoke(kart);
                                return;
                            }
                            else
                            {
                                CurrentLapEndPoint = checkPoint;
                                OnLapComplete?.Invoke(kart);
                                Debug.Log("Next Lap: " + (currentLap));
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
                if(nextCheckPointIndex == 0 && checkPoint.index+1 == totalCheckpoints)
                {

                }
                else if(checkPoint.index < nextCheckPointIndex)
                {

                }
                else
                {
                }
                if (HasStartedRace)
                {
                    OnWrongCheckPoint?.Invoke(kart);
                }
            }
        }

        public void NextLap()
        {
            lastCheckPoint = null;
        }
    }
}
