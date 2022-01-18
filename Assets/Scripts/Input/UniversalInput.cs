public class UniversalInput : BaseInput
{
    UniversalInputActions actions;
    private void Awake() { actions = new UniversalInputActions(); actions.Player.Enable(); }
    public override InputData GenerateInput() { return new InputData { TurnInput = actions.Player.Steering.ReadValue<float>(), Accelerate = actions.Player.Accelerate.IsPressed(), Brake = actions.Player.Brake.IsPressed() }; }
}
