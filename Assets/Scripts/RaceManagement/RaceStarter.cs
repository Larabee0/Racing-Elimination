using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceStarter : MonoBehaviour
{
    RaceManager raceManager;
    [Range(0f, 10f)]
    [SerializeField] private float StartDelay = 5f;
    [Range(3, 10)]
    [SerializeField] private int CountDownLength = 3;

    private void Awake()
    {
        raceManager = GetComponent<RaceManager>();
    }

    private void Start()
    {
        if (raceManager.ui != null)
        {
            raceManager.ui.SetButtons(false);
            raceManager.ui.ShowText("Get Ready!");
            Invoke(nameof(Laps), StartDelay * 0.25f);
            Invoke(nameof(Timer), StartDelay * 0.5f);
            Invoke(nameof(Place), StartDelay * 0.75f);
            if (raceManager.gameMode == GameModes.Elimination) { Invoke(nameof(Elim), StartDelay * 0.8f); }
            Invoke(nameof(StartRaceCountDown), StartDelay);
        }
    }

    private void StartRaceCountDown()
    {
        StopAllCoroutines();
        StartCoroutine(CountDown());
    }

    private void Laps()
    {
        raceManager.ui.ShowLaps();
    }

    private void Timer()
    {
        raceManager.ui.ShowLapTime();
    }

    private void Place()
    {
        raceManager.ui.ShowPlace();
    }

    private void Elim()
    {
        raceManager.ui.ShowEliminationTime();
    }

    private IEnumerator CountDown()
    {
        for (int t = CountDownLength; t > 0; t --)
        {
            raceManager.ui.ShowText(t.ToString());
            yield return new WaitForSeconds(1f);
        }


        raceManager.ui.ShowText("Go!");
        raceManager.StartRace();
        yield return new WaitForSeconds(1f);
        raceManager.ui.HideFinished();
    }
}
