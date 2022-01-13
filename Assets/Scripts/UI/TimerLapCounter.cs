using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimerLapCounter : MonoBehaviour
{
    public Text lapCounter;
    public Text timer;
    public GameObject FinishedText;

    private int maxLaps;
    private int currentLap;

    public int MaxLaps { set { maxLaps = value; SetLapCounter(); } }
    public int CurrentLap { set { currentLap = value; SetLapCounter(); } }

    float timerLap = 0;
    float timerTotal = 0;

    private void SetLapCounter()
    {
        lapCounter.text = string.Format("Lap {0}/{1}", currentLap, maxLaps);
    }

    public void SetTime()
    {
        TimeSpan time = TimeSpan.FromSeconds(timerLap);
        timer.text = string.Format("Lap: {0}:{1}.{2}", time.Minutes, time.Seconds, time.Milliseconds);
    }
    public void TotalTime()
    {
        TimeSpan time = TimeSpan.FromSeconds(timerTotal);
        timer.text = string.Format("Total: {0}:{1}.{2}", time.Minutes, time.Seconds, time.Milliseconds);
    }

    private void Update()
    {
        timerLap += Time.deltaTime;
        SetTime();
    }

    public void ResetTimer()
    {
        timerTotal += timerLap;
        timerLap = 0f;
    }

    public void HideFinished()
    {
        FinishedText.SetActive(false);
    }

    public void ShowFinished()
    {
        FinishedText.SetActive(true);
    }
}
