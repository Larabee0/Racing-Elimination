using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class Title : MonoBehaviour
{
    public UniversalInputActions actions;
    public KartColourFactory kartColours;
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

        actions = new UniversalInputActions(); actions.UI.Enable();
        actions.UI.Green.performed += Play;
        actions.UI.Green.performed += Exit;
        List<ArcadeKart> karts = new List<ArcadeKart>(FindObjectsOfType<ArcadeKart>());

        if (kartColours != null)
        {
            kartColours.PaintKarts(karts);
        }
    }

    private void Play(InputAction.CallbackContext context)
    {

        if (context.performed)
        {
            EliminationMode();
        }
    }
    private void Exit(InputAction.CallbackContext context)
    {

        if (context.performed)
        {
            Quit();
        }
    }
}
