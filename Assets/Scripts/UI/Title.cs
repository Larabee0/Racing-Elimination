using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class Title : MonoBehaviour
{
    public UniversalInputActions actions;
    public KartColourFactory kartColours;
    private UIDocument document;
    public void EliminationMode()
    {
        SceneManager.LoadScene(1);
    }
    public void Quit()
    {
        Application.Quit();
    }

    private void Awake()
    {
        document = GetComponent<UIDocument>();
        //document.rootVisualElement.Q<Button>("StartButton").();
        document.rootVisualElement.Q<Button>("StartButton").RegisterCallback<NavigationSubmitEvent>(ev=>EliminationMode());
        document.rootVisualElement.Q<Button>("QuitButton").RegisterCallback<NavigationSubmitEvent>(ev => Quit());
        document.rootVisualElement.Q<Button>("StartButton").RegisterCallback<ClickEvent>(ev => EliminationMode());
        document.rootVisualElement.Q<Button>("QuitButton").RegisterCallback<ClickEvent>(ev => Quit());
        actions = new UniversalInputActions(); actions.UI.Enable();
        List<ArcadeKart> karts = new(FindObjectsOfType<ArcadeKart>());

        if (kartColours != null)
        {
            kartColours.PaintKarts(karts);
        }
    }

    private void Start()
    {
        GetComponent<EventSystem>().SetSelectedGameObject(FindObjectOfType<PanelEventHandler>().gameObject);
    }
}
