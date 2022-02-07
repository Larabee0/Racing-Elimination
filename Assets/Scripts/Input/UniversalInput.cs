using UnityEngine.InputSystem;
using UnityEngine;

public class UniversalInput : BaseInput
{
    public UniversalInputActions actions;

    public static UniversalInput instance;

    public RaceManager raceManager;


    public bool UIMapEnabled;
    public bool ControlMapEnabled;
    public static bool ControllerMode = false;

    private void Awake() 
    {
        instance = this;
        actions = new UniversalInputActions();
        raceManager = FindObjectOfType<RaceManager>();
        if(raceManager != null)
        {
            actions.Player.Pause.performed += raceManager.ui.Pause;
        }
        if (Gamepad.current != null)
        {
            ControllerMode = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
    public override InputData GenerateInput() { return new InputData { TurnInput = actions.Player.Steering.ReadValue<float>(), Accelerate = actions.Player.Accelerate.IsPressed(), Brake = actions.Player.Brake.IsPressed() }; }
    private void Update()
    {
        if (raceManager != null)
        {
            if (raceManager.Game is EliminationMode elim)
            {
                if (elim.isRunning)
                {
                    if (actions.UI.enabled)
                    {
                        actions.UI.Disable();
                    }
                    if (!actions.Player.enabled)
                    {
                        actions.UI.Enable();
                    }
                }
                else
                {
                    if (!actions.UI.enabled)
                    {
                        actions.UI.Enable();
                    }
                    if (actions.Player.enabled)
                    {
                        actions.UI.Disable();
                    }
                }
            }
        }
        else
        {
            if (!actions.UI.enabled)
            {
                actions.UI.Enable();
            }
        }
        UIMapEnabled = actions.UI.enabled;
        ControlMapEnabled = actions.Player.enabled;
    }
}
