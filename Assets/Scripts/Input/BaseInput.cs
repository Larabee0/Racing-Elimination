using UnityEngine;

public struct InputData
{
    public bool Accelerate;
    public bool Brake;
    public float TurnInput;

    public static InputData NoInput = new() { Accelerate = false, Brake = false, TurnInput = 0 };
}

public interface IInput
{
    InputData GenerateInput();
}

public abstract class BaseInput : MonoBehaviour, IInput
{
    /// <summary>
    /// Override this function to generate an XY input that can be used to steer and control the car.
    /// </summary>
    public abstract InputData GenerateInput();
}

