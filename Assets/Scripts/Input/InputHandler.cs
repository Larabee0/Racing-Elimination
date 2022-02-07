using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public static InputHandler instance;
    
    public RaceManager raceManager;

    public UniversalInputActions actions;

    public bool UIMapEnabled;
    public bool ControlMapEnabled;
    private void Awake()
    {
        instance = this;
        raceManager = FindObjectOfType<RaceManager>();
        
    }

    
}
