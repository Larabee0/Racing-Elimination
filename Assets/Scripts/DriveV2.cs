using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DriveV2 : MonoBehaviour
{
    public WheelCollider FrontRight;
    public WheelCollider FrontLeft;
    public WheelCollider RearRight;
    public WheelCollider RearLeft;

    public float frontWheelMotorTorque;
    public float rearWheelMotorTorque;

    public float frontWheelBreakTorque;
    public float rearWheelBreakTorque;

    public float HandBreakForce = 1000;

    [SerializeField] private Direction DirectionLock = Direction.Stop;
    [SerializeField] private bool Lockout = true;
    [SerializeField] private bool IsBreaking = false;

    public float FrontMotorTorqueRaw { set { FrontLeft.motorTorque = FrontRight.motorTorque = value; } }
    public float RearMotorTorqueRaw { set { RearLeft.motorTorque = RearRight.motorTorque = value; } }

    public float FrontBreakTorqueRaw { set { FrontLeft.brakeTorque = FrontRight.brakeTorque = value; } }
    public float RearBreakTorqueRaw { set { RearLeft.brakeTorque = RearRight.brakeTorque = value; } }

    public float FrontMotorTorque { set { FrontMotorTorqueRaw = Mathf.Clamp(value, -1, 1) * frontWheelMotorTorque; } }
    public float RearMotorTorque { set { RearMotorTorqueRaw = Mathf.Clamp(value, -1, 1) * rearWheelMotorTorque; } }
    public float FrontBreakTorque { set { FrontBreakTorqueRaw = Mathf.Clamp(value, 0, 1) * frontWheelBreakTorque; } }
    public float RearBreakTorque { set { RearBreakTorqueRaw = Mathf.Clamp(value, 0, 1) * rearWheelBreakTorque; } }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(RearLeft.rpm);
        if (((RearLeft.rpm > 0 && RearLeft.rpm < float.Epsilon) || (RearLeft.rpm < 0 && RearLeft.rpm > -float.Epsilon) || RearLeft.rpm == 0) && DirectionLock != Direction.Stop && IsBreaking)
        {
            DirectionLock = Direction.Stop;
            Lockout = true;
            return;
        }
        else if ((Input.GetKeyDown(KeyCode.W) && !Input.GetKey(KeyCode.S)) || (Input.GetKeyDown(KeyCode.S) && !Input.GetKey(KeyCode.W)))
        {
            Lockout = false;
        }

        if (DirectionLock == Direction.Stop && Input.GetAxis("Vertical") != 0 && !Lockout)
        {
            if (Input.GetAxis("Vertical") > 0)
            {
                DirectionLock = Direction.Foward;
            }
            else if (Input.GetAxis("Vertical") < 0)
            {
                DirectionLock = Direction.Reverse;
            }
            RearBreakTorque = FrontBreakTorque = 0;
            IsBreaking = false;
        }
        else if (DirectionLock == Direction.Stop)
        {
            FrontBreakTorqueRaw = RearBreakTorqueRaw = 1000;
        }

        if (DirectionLock == Direction.Foward)
        {
            RearMotorTorque = FrontMotorTorque = Input.GetAxis("Vertical") < 0 ? 0 : Input.GetAxis("Vertical");
            float isBreaking = RearBreakTorque = FrontBreakTorque = Input.GetAxis("Vertical") > 0 ? 0 : -Input.GetAxis("Vertical");
            IsBreaking = isBreaking > 0;
        }
        if (DirectionLock == Direction.Reverse)
        {
            RearMotorTorque = FrontMotorTorque = Input.GetAxis("Vertical") > 0 ? 0 : Input.GetAxis("Vertical");
            float isBreaking = RearBreakTorque = FrontBreakTorque = Input.GetAxis("Vertical") < 0 ? 0 : Input.GetAxis("Vertical");
            IsBreaking = isBreaking > 0;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            FrontBreakTorqueRaw = RearBreakTorqueRaw = 1000;
            IsBreaking = true;
        }
    }
}
