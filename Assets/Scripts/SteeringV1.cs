using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteeringV1 : MonoBehaviour
{
    public WheelCollider SteeredWheelRight;
    public WheelCollider SteeredWheelLeft;
    public float SteeringAngleMax = 30;
    public float SteeringAngle { set { SteeredWheelLeft.steerAngle = SteeredWheelRight.steerAngle = Mathf.Clamp(value, -1, 1) * SteeringAngleMax; } }
    
    // Update is called once per frame
    void Update()
    {
        SteeringAngle = Input.GetAxis("Horizontal");
    }
}
