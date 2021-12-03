using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DriveV1 : MonoBehaviour
{
    public WheelCollider wheelCollider;
    public float torque = 200f;
    public float SteeringAngleMax = 30;
    public bool EnableSteering = false;
    [SerializeField] private Direction DirectionLock = Direction.Stop;
    [SerializeField] private bool Lockout = true;

    public float MotorTorque { set { wheelCollider.motorTorque = Mathf.Clamp(value, -1, 1) * torque; } }
    public float BreakingTorque { set { wheelCollider.brakeTorque = Mathf.Clamp(value, 0, 1) * torque; } }
    public float SteeringAngle { set { wheelCollider.steerAngle = EnableSteering ? Mathf.Clamp(value, -1, 1) * SteeringAngleMax : 0f; } }
    private void Start()
    {
        Debug.Log(-float.Epsilon);
    }
    // Update is called once per frame
    void Update()
    {
        Debug.Log(wheelCollider.rpm);
        if (((wheelCollider.rpm > 0 && wheelCollider.rpm < float.Epsilon) || (wheelCollider.rpm < 0 && wheelCollider.rpm > -float.Epsilon) || wheelCollider.rpm == 0) && DirectionLock != Direction.Stop)
        {
            DirectionLock = Direction.Stop;
            Lockout = true;
            return;
        }
        else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S))
        {
            Lockout = false;
            return;
        }

        if(DirectionLock == Direction.Stop && Input.GetAxis("Vertical") != 0 && !Lockout)
        {
            if (wheelCollider.rpm > 0)
            {
                DirectionLock = Direction.Foward;
            }
            else if (wheelCollider.rpm < 0)
            {
                DirectionLock = Direction.Reverse;
            }
            MotorTorque = Input.GetAxis("Vertical");
            BreakingTorque = 0;
        }
        else if (DirectionLock == Direction.Stop)
        {
            wheelCollider.brakeTorque = 1000;
        }
        if(DirectionLock == Direction.Foward)
        {
            MotorTorque = Input.GetAxis("Vertical") < 0 ? 0 : Input.GetAxis("Vertical");
            BreakingTorque = Input.GetAxis("Vertical") > 0 ? 0 : -Input.GetAxis("Vertical");
        }
        if (DirectionLock == Direction.Reverse)
        {
            MotorTorque = Input.GetAxis("Vertical") > 0 ? 0 : Input.GetAxis("Vertical");
            BreakingTorque = Input.GetAxis("Vertical") < 0 ? 0 : Input.GetAxis("Vertical");
        }

        if (Input.GetKey(KeyCode.Space))
        {
            wheelCollider.brakeTorque = 1000;
        }
        SteeringAngle = Input.GetAxis("Horizontal");

    }
}

public enum Direction
{
    Stop,
    Foward,
    Reverse
}
