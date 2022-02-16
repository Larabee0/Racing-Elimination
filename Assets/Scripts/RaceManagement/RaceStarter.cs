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
            raceManager.playerKart.GetComponent<UniversalInput>().actions.UI.Disable();
            raceManager.ui.SetButtons(false);
            raceManager.ui.ShowText("Get Ready!");
            float divider = 1f/4f;
            if (raceManager.gameMode == GameModes.Elimination) { divider = 1f / 5f; Invoke(nameof(Elim), StartDelay * (divider * 4)); }
            Invoke(nameof(Laps), StartDelay * divider);
            Invoke(nameof(Timer), StartDelay * (divider * 2));
            Invoke(nameof(Place), StartDelay * (divider * 3));
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
