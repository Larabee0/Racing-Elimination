using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class RaceUI : MonoBehaviour
{
    private Label lapCounter;
    private Label timer;
    private Label eliminationTime;
    private Label place;
    private Label FinishedText;
    private VisualElement ExitPlayAgainButtons;
    private Button GreenButton;

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

    private void Awake()
    {
        VisualElement rootVisualElement = GetComponent<UIDocument>().rootVisualElement;

        lapCounter = rootVisualElement.Q<Label>("LapLabel");
        timer = rootVisualElement.Q<Label>("TimerLabel");
        eliminationTime = rootVisualElement.Q<Label>("EliminationLabel");
        place = rootVisualElement.Q<Label>("PlaceLabel");
        FinishedText = rootVisualElement.Q<Label>("CentreLabel");
        ExitPlayAgainButtons = rootVisualElement.Q<VisualElement>("ButtonContainer");
        GreenButton = rootVisualElement.Q<Button>("StartButton");

        GreenButton.RegisterCallback<NavigationSubmitEvent>(ev => PlayAgain());
        GreenButton.RegisterCallback<ClickEvent>(ev => PlayAgain());

        rootVisualElement.Q<Button>("QuitButton").RegisterCallback<ClickEvent>(ev => Quit());
        rootVisualElement.Q<Button>("QuitButton").RegisterCallback<NavigationSubmitEvent>(ev => Quit());
    }

    private void SetLapCounter()
    {
        lapCounter.text = gameMode switch
        {
            GameModes.Laps => string.Format("Lap {0}/{1}", currentLap.ToString("D2"), maxLaps.ToString("D2")),
            _ => string.Format("Lap {0}", currentLap.ToString("D2"))
        };

        if (gameMode == GameModes.Elimination)
        {
            int timeTillDeath = Mathf.Max(Mathf.RoundToInt(timeTillElimination), 0);
            eliminationTime.text = String.Format("Eliminating place: {1} in: {0}", timeTillDeath.ToString("D2"), placeToElimination.ToString("D2"));
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
        timer.style.width = 375;
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
        FinishedText.style.display = DisplayStyle.None;
    }

    public void ShowFinished()
    {
        FinishedText.text = "Finished";
        FinishedText.style.display = DisplayStyle.Flex;
    }

    public void ShowText(string text)
    {
        FinishedText.text = text;
        FinishedText.style.display = DisplayStyle.Flex;
    }

    public void ShowPlace()
    {
        place.style.display = DisplayStyle.Flex;
    }

    public void ShowLaps()
    {
        lapCounter.style.display = DisplayStyle.Flex;
    }

    public void ShowLapTime()
    {
        timer.style.display = DisplayStyle.Flex;
    }

    public void ShowEliminationTime()
    {
        eliminationTime.style.display = DisplayStyle.Flex;
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
        ExitPlayAgainButtons.style.display = shown switch
        {
            true => DisplayStyle.Flex,
            false => DisplayStyle.None
        };

        if (shown)
        {
            SetMenuSelection();
        }
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
            GreenButton.text = "Resume";
            SetButtons(true);
            Time.timeScale = 0;
        }
    }
    public void UnPause()
    {
        Time.timeScale = 1;
        SetButtons(false);
        GreenButton.text = "One More Go?";
    }
    public void SetMenuSelection()
    {
        FindObjectOfType<EventSystem>().SetSelectedGameObject(FindObjectOfType<PanelEventHandler>().gameObject);
    }
}
