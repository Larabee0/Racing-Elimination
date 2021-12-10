using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DriveV2 : MonoBehaviour
{
    public Rigidbody CarRigidbody;
    public WheelCollider FrontRight;
    public WheelCollider FrontLeft;
    public WheelCollider RearRight;
    public WheelCollider RearLeft;
    [Header("Front Wheel Stiffness")]
    [Range(0.1f, 5f)]
    public float frontWheelForwardStiffness = 1f;

    [Range(0.1f, 5f)]
    public float frontWheelSidewaysStiffness = 1f;

    [Header("Rear Wheel Stiffness")]
    [Range(0.1f, 5f)]
    public float rearWheelForwardStiffness = 1f;
    [Range(0.1f, 5f)]
    public float rearWheelSidewaysStiffness = 1f;

    [Header("Power Settings")]
    public AnimationCurve torqueCurve;

    public float thorttlePosition;

    public float frontWheelMotorTorque;
    public float rearWheelMotorTorque;

    public float frontWheelBreakTorque;
    public float rearWheelBreakTorque;

    public float HandBreakForce = 1000;
    public float maxSpeed = 1000;

    [SerializeField] private Direction DirectionLock = Direction.Stop;
    [SerializeField] private bool Lockout = true;
    [SerializeField] private bool IsBreaking = false;

    public float FrontMotorTorqueRaw { set { FrontLeft.motorTorque = FrontRight.motorTorque = value; } }
    public float RearMotorTorqueRaw { set { RearLeft.motorTorque = RearRight.motorTorque = value; } }

    public float FrontBreakTorqueRaw { set { FrontLeft.brakeTorque = FrontRight.brakeTorque = value; } }
    public float RearBreakTorqueRaw { set { RearLeft.brakeTorque = RearRight.brakeTorque = value; } }

    public float FrontMotorTorque { set { FrontMotorTorqueRaw = TorqueCurveRead(Mathf.Clamp(value, -1, 1)) * frontWheelMotorTorque; } }
    public float RearMotorTorque { set { RearMotorTorqueRaw = TorqueCurveRead(Mathf.Clamp(value, -1, 1)) * rearWheelMotorTorque; } }
    public float FrontBreakTorque { set { FrontBreakTorqueRaw = TorqueCurveRead(Mathf.Clamp(value, 0, 1)) * frontWheelBreakTorque; } }
    public float RearBreakTorque { set { RearBreakTorqueRaw = TorqueCurveRead(Mathf.Clamp(value, 0, 1)) * rearWheelBreakTorque; } }

    private void Start()
    {
        FrontRight.ConfigureVehicleSubsteps(10,12,15);
        FrontLeft.ConfigureVehicleSubsteps(10,12,15);
        RearRight.ConfigureVehicleSubsteps(10, 12, 15);
        RearLeft.ConfigureVehicleSubsteps(10, 12, 15);

    }

    // Update is called once per frame
    void Update()
    {
        SetStiffness();
        //Debug.Log("Steer:"+FrontRight.steerAngle+" FR:" + FrontRight.rpm + " FL:" + FrontLeft.rpm + " RR:" + RearRight.rpm + " RL:" + RearLeft.rpm);
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
            float input = Input.GetAxis("Vertical") < 0 ? 0 : Input.GetAxis("Vertical");
            FrontMotorTorque = RearMotorTorque = input;
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

    private float TorqueCurveRead(float throttlePos)
    {

        float rpm = CarRigidbody.velocity.magnitude;
        thorttlePosition = torqueCurve.Evaluate(rpm / maxSpeed);
        return thorttlePosition * throttlePos;
    }

    private void SetStiffness()
    {
        WheelFrictionCurve FrontForwardCurve = FrontLeft.forwardFriction;
        FrontForwardCurve.stiffness = frontWheelForwardStiffness;
        FrontLeft.forwardFriction = FrontRight.forwardFriction = FrontForwardCurve;


        WheelFrictionCurve FrontSideCurve = FrontLeft.sidewaysFriction;
        FrontSideCurve.stiffness = frontWheelSidewaysStiffness;
        FrontLeft.sidewaysFriction = FrontRight.sidewaysFriction = FrontSideCurve;


        WheelFrictionCurve RearForwardCurve = RearLeft.forwardFriction;
        RearForwardCurve.stiffness = rearWheelForwardStiffness;
        RearLeft.forwardFriction = RearRight.forwardFriction = RearForwardCurve;


        WheelFrictionCurve RearSideCurve = RearLeft.sidewaysFriction;
        RearSideCurve.stiffness = rearWheelSidewaysStiffness;
        RearLeft.sidewaysFriction = RearRight.sidewaysFriction = RearSideCurve;
    }
}
