using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class RaceUI : MonoBehaviour
{
    public Text lapCounter;
    public Text timer;
    public Text eliminationTime;
    public Text place;
    public GameObject FinishedText;
    public GameObject ExitPlayAgainButtons;

    private int maxLaps;
    private int currentLap;

    [HideInInspector]
    public GameModes gameMode;

    public int MaxLaps { set { maxLaps = value; SetLapCounter(); } }
    public int CurrentLap { set { currentLap = value; SetLapCounter(); } }

    public int Place { set { place.text = "Place: " + value.ToString("D2"); } }

    float timerLap = 0;
    float timerTotal = 0;

    float timeTillElimination;
    public int placeToElimination;
    public float TimeTillElimination { set { timeTillElimination = value; SetLapCounter(); } }

    private void SetLapCounter()
    {
        lapCounter.text = gameMode switch
        {
            GameModes.Laps => string.Format("| Lap {0}/{1}", currentLap.ToString("D2"), maxLaps.ToString("D2")),
            _ => string.Format("| Lap {0}", currentLap.ToString("D2"))
        };

        if (gameMode == GameModes.Elimination)
        {
            int timeTillDeath = Mathf.Max(Mathf.RoundToInt(timeTillElimination), 0);
            eliminationTime.text = String.Format("Eliminating place: {1} in: {0} |", timeTillDeath.ToString("D2"), placeToElimination.ToString("D2"));
        }
    }

    public void SetTime()
    {
        TimeSpan time = TimeSpan.FromSeconds(timerLap);
        timer.text = string.Format("Lap: {0}:{1}.{2}", time.Minutes.ToString("D2"), time.Seconds.ToString("D2"), time.Milliseconds.ToString("D3"));
    }
    public void TotalTime()
    {
        TimeSpan time = TimeSpan.FromSeconds(timerTotal);
        timer.text = string.Format("Total: {0}:{1}.{2}", time.Minutes.ToString("D2"), time.Seconds.ToString("D2"), time.Milliseconds.ToString("D3"));
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
        FinishedText.GetComponent<Text>().text = "Finished";
        FinishedText.SetActive(true);
    }

    public void ShowText(string text)
    {
        FinishedText.GetComponent<Text>().text = text;
        FinishedText.SetActive(true);
    }

    public void ShowPlace()
    {
        place.enabled = true;
    }

    public void ShowLaps()
    {
        lapCounter.enabled = true;
    }

    public void ShowLapTime()
    {
        timer.enabled = true;
    }

    public void ShowEliminationTime()
    {
        eliminationTime.enabled = true;
    }

    public void Quit()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(0);
    }

    public void PlayAgain()
    {
        if (Time.timeScale == 0)
        {
            UnPause();
            return;
        }
        SceneManager.LoadScene(1);
    }

    public void SetButtons(bool shown)
    {
        ExitPlayAgainButtons.SetActive(shown);
    }

    public void Pause(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (Time.timeScale == 0)
            {
                UnPause();
                return;
            }
            FindObjectOfType<EventSystem>().SetSelectedGameObject(ExitPlayAgainButtons.transform.GetChild(1).gameObject);
            Time.timeScale = 0;
            ExitPlayAgainButtons.transform.GetChild(1).GetComponentInChildren<Text>().text = "Resume";
            SetButtons(true);
        }
    }
    public void UnPause()
    {
        Time.timeScale = 1;
        SetButtons(false);
        ExitPlayAgainButtons.transform.GetChild(1).GetComponentInChildren<Text>().text = "One More Go?";
    }

    public void SetMenuSelection()
    {
        FindObjectOfType<EventSystem>().SetSelectedGameObject(ExitPlayAgainButtons.transform.GetChild(1).gameObject);
    }
}
