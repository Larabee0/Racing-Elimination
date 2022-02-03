using UnityEngine.InputSystem;

public class UniversalInput : BaseInput
{
    public UniversalInputActions actions;
    private RaceUI raceUI;
    private void Awake() 
    { 
        actions = new UniversalInputActions(); actions.Player.Enable(); actions.UI.Disable();
        raceUI = FindObjectOfType<RaceUI>();
        actions.UI.Green.performed += Play;
        actions.UI.Red.performed += Exit;
    }
    public override InputData GenerateInput() { return new InputData { TurnInput = actions.Player.Steering.ReadValue<float>(), Accelerate = actions.Player.Accelerate.IsPressed(), Brake = actions.Player.Brake.IsPressed() }; }

    private void Play(InputAction.CallbackContext context)
    {

        if (context.performed)
        {
            raceUI.PlayAgain();
        }
    }
    private void Exit(InputAction.CallbackContext context)
    {

        if (context.performed)
        {
            raceUI.Quit();
        }
    }
}
