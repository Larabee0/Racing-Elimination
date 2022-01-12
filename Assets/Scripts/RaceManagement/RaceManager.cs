using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceManager : MonoBehaviour
{
    public List<CheckPoint> checkPoints = new List<CheckPoint>();
    public CheckPoint startFinishLine;


    public class KartAndLastPoin
    {
        public CheckPoint lastCheckPoint = null;
        public CheckPoint finishLineCheckPoint = null;
        public bool HasStarted { get { return lastCheckPoint != null; } }
        public bool HasFinished { get { return HasStarted && finishLineCheckPoint != null; } }

        public void HandleStartFinish(CheckPoint checkPoint)
        {
            if (HasStarted)
            {
                if (HasFinished)
                {
                    return;
                }
                else
                {
                    finishLineCheckPoint = checkPoint;
                    // invoke Finish Handle;
                }
            }
            else
            {
                lastCheckPoint = checkPoint;
                // invoke start handle
            }
        }

        public void NextLap()
        {
            lastCheckPoint = null;
        }
    }
}
