using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarReadout : MonoBehaviour
{
    public Rigidbody rb;
    public DriveV2 drive;
    public Text speedO;
    public Text wheelRPMAverage;
    public Text wheelRPM;
    public Text thottlePosition;
    public Text forwardFriction;
    public Text sideWaysFriction;
    // Start is called before the first frame update
    void Start()
        
    {
    }

    // Update is called once per frame
    void Update()
    {
        speedO.text = rb.velocity.magnitude.ToString();
        float FLRPM = drive.FrontLeft.rpm;
        float FRRPM = drive.FrontRight.rpm;
        float RLRPM = drive.RearLeft.rpm;
        float RRRPM = drive.RearRight.rpm;

        wheelRPM.text = FLRPM + " " + FRRPM + " " + RLRPM + " " + RRRPM;
        wheelRPMAverage.text = ((FLRPM + FRRPM + RLRPM + RRRPM) / 4).ToString();
        thottlePosition.text = drive.thorttlePosition.ToString();
    }
}
