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
    public Dictionary<ArcadeKart,KartTracker> trackers;
    [Range(1,50)]
    public int Laps = 1;
    public bool reverseCheckPoints = false;
    public TimerLapCounter ui;

    public void Awake()
    {
        path = checkPointContainer.GetComponent<PathCreator>().path;
        checkPoints = new List<CheckPoint>(checkPointContainer.GetComponentsInChildren<CheckPoint>());
        if (reverseCheckPoints)
        {
            checkPoints.Reverse();
        }
        if (ui == null)
        {
            Debug.LogWarning("UI is null! No ui output will be made.");
        }

        int i = 0;
        checkPoints.ForEach(p => { startFinishLine = p.StartFinishLine ? p : startFinishLine; p.OnKartPassedCheckPoint += OnKartPassedCheckPoint; p.index = i; i++; });
        if (startFinishLine == null)
        {
            Debug.LogError("no start finish line.");
            enabled = false;
        }
        indexOfFstartFinishLine = checkPoints.IndexOf(startFinishLine);
        karts = new List<ArcadeKart>(FindObjectsOfType<ArcadeKart>());
        trackers = new Dictionary<ArcadeKart, KartTracker>(karts.Count);
        karts.ForEach(k =>
        {
            kartPos.Add(k.transform.position);
            ResetKart(k);
        });
        startFinishLineTime = path.GetClosestDistanceAlongPath(path.GetClosestPointOnPath(startFinishLine.transform.position));
        Debug.Log("Start finish distance: " + startFinishLineTime);
    }

    private void Update()
    {
        DetermineFirstPlace();
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
        }
    }

    public int firstPlacelapNumber = 0;
    public ArcadeKart firstPlaceKart;
    public CheckPoint firstPlaceLastCheckPoint;
    List<Vector3> kartPos = new List<Vector3>();
    bool showOnce = true;
    float startFinishLineTime;
    public void DetermineFirstPlace()
    {
        List<KartPositionInfo> kartPositions = new List<KartPositionInfo>(karts.Count);
        for (int i = 0; i < karts.Count; i++)
        {
            kartPos[i] = path.GetClosestPointOnPath(karts[i].transform.position);
            float time = reverseCheckPoints ? startFinishLineTime - path.GetClosestDistanceAlongPath(path.GetClosestPointOnPath(karts[i].transform.position)) : path.GetClosestDistanceAlongPath(path.GetClosestPointOnPath(karts[i].transform.position)) - startFinishLineTime;
            kartPositions.Add(new KartPositionInfo(i, trackers[karts[i]].currentLap,  time));
            if (showOnce)
            {
                Debug.Log(karts[i].gameObject.name + " distance: " + ( time));
            }
            
        }
        showOnce = false;
        kartPositions.Sort();
        for (int i = 0; i < kartPositions.Count; i++)
        {
            karts[kartPositions[i].kartIndex].position = i + 1;
        }
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < kartPos.Count; i++)
        {
            Gizmos.DrawSphere(kartPos[i], 0.2f);
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
                int lapCompare = this.Lap.CompareTo(other.Lap);
                if(lapCompare > 0)
                {
                    return 1;
                }
                else if(lapCompare < 0)
                {
                    return -1;
                }
                else
                {
                    int checkPointCompare = this.timeOnPath.CompareTo(other.timeOnPath);
                    if (checkPointCompare > 0)
                    {
                        return 1;
                    }
                    else if (lapCompare < 0)
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

    #region EventImplentations
    public void OnKartPassedCheckPoint(KartPassedCheckPointArgs f)
    {
        CheckPoint sender = f.sender;
        ArcadeKart kart = f.kart;
        KartTracker tracker = trackers[kart];
        tracker.HandleCheckPoint(sender);
    }

    public void OnKartStartCircuit(ArcadeKart kart)
    {
        ui.HideFinished();
        ui.CurrentLap = trackers[kart].currentLap + 1;
        ui.MaxLaps = Laps;
        ui.ResetTimer();
        ui.enabled = true;
    }

    public void OnKartCompleteLap(ArcadeKart kart)
    {
        ui.CurrentLap = trackers[kart].currentLap + 1;
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
    #endregion

    public class KartTracker
    {
        public ArcadeKart kart;
        public CheckPoint startLine = null;
        public CheckPoint lastCheckPoint = null;
        public CheckPoint finishLineCheckPoint = null;
        public CheckPoint CurrentLapEndPoint;
        public int laps;
        public int currentLap;
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

        public KartTracker(ArcadeKart kart, int laps, int totalCheckPoints, CheckPoint startLine, CheckPoint finishLine, CheckPoint LapEndPoint)
        {
            this.kart = kart;
            this.totalCheckpoints = totalCheckPoints;
            this.laps = laps;
            this.startLine = startLine;
            RaceEndCheckPoint = finishLine;
            CurrentLapEndPoint = LapEndPoint;
            nextCheckPointIndex = startLine.index;
            kart.tracker = this;
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
                            if (currentLap >= laps)
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
                                Debug.Log("Next Lap: " + (currentLap + 1));
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
