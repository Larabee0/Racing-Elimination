using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    public GameObject checkPointContainer;
    public List<CheckPoint> checkPoints = new List<CheckPoint>();
    public CheckPoint startFinishLine;
    public int indexOfFstartFinishLine;
    List<ArcadeKart> karts;
    public Dictionary<ArcadeKart,KartTracker> trackers;
    [Range(1,50)]
    public int Laps = 1;
    public bool reverseCheckPoints = false;
    public TimerLapCounter ui;

    public void Awake()
    {
        checkPoints = new List<CheckPoint>(checkPointContainer.GetComponentsInChildren<CheckPoint>());
        if (reverseCheckPoints)
        {
            checkPoints.Reverse();
        }
        
        int i = 0;
        checkPoints.ForEach(p => { startFinishLine = p.StartFinishLine ? p : startFinishLine; p.OnKartPassedCheckPoint += OnKartPassedCheckPoint; p.index = i; i++; });
        if(startFinishLine == null)
        {
            Debug.LogError("no start finish line.");
            enabled = false;
        }
        indexOfFstartFinishLine = checkPoints.IndexOf(startFinishLine);
        karts = new List<ArcadeKart>(FindObjectsOfType<ArcadeKart>());
        trackers = new Dictionary<ArcadeKart, KartTracker>(karts.Count);
        karts.ForEach(k => { 
            trackers.Add(k, new KartTracker(k, Laps, checkPoints.Count,startFinishLine, startFinishLine, startFinishLine)); 
            trackers[k].OnRaceStart += OnKartStartRace;
            trackers[k].OnRaceComplete += OnKartFinishRace;
            trackers[k].OnLapComplete += OnKartCompleteLap;
        });
    }

    public void ResetKart(ArcadeKart kart)
    {
        if (trackers.ContainsKey(kart))
        {
            trackers.Remove(kart);
        }
        trackers.Add(kart, new KartTracker(kart, Laps, checkPoints.Count, startFinishLine, startFinishLine, startFinishLine));
        trackers[kart].OnRaceStart += OnKartStartRace;
        trackers[kart].OnRaceComplete += OnKartFinishRace;
        trackers[kart].OnLapComplete += OnKartCompleteLap;

    }

    public void OnKartPassedCheckPoint(KartPassedCheckPointArgs f)
    {
        CheckPoint sender = f.sender;
        ArcadeKart kart = f.kart;
        KartTracker tracker = trackers[kart];
        tracker.HandleCheckPoint(sender);
    }

    public void OnKartStartRace(ArcadeKart kart)
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

    public void OnKartFinishRace(ArcadeKart kart)
    {
        ui.enabled = false;
        ui.ShowFinished();
        ui.ResetTimer();
        ui.TotalTime();
    }

    public delegate void LapComplete(ArcadeKart kart);
    public delegate void RaceComplete(ArcadeKart kart);
    public delegate void RaceStart(ArcadeKart kart);

    public delegate void CorrectCheckPoint(ArcadeKart kart);
    public delegate void WrongCheckPoint(ArcadeKart kart);

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
        public RaceComplete OnRaceComplete;
        public RaceStart OnRaceStart;

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
                                OnRaceComplete?.Invoke(kart);
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
                        OnRaceStart?.Invoke(kart);
                        Debug.Log("started Race");
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
                    OnWrongCheckPoint?.Invoke(kart);
                    Debug.Log("Incorrect Point");
                }
                
            }
        }

        public void NextLap()
        {
            lastCheckPoint = null;
        }
    }
}
