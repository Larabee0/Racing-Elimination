using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Title : MonoBehaviour
{
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

        List<ArcadeKart> karts = new List<ArcadeKart>(FindObjectsOfType<ArcadeKart>());

        if (kartColours != null)
        {
            kartColours.PaintKarts(karts);
        }
    }
}
