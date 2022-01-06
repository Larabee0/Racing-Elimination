using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DriveV3 : MonoBehaviour
{
    public Rigidbody sphereRB;
    public float forwardAccel = 8f, reverseAccel = 4f, maxSpeed = 50f, turnStrength = 180f, extraGravity = 10f, dragOnGround = 3f;

    private float speedInput, turnInput;

    private bool grounded;

    public LayerMask ground;
    public float groundRayLength = 0.5f;
    public Transform groudRayPoint;

    void Start()
    {

        sphereRB.transform.parent = null;
    }

    void Update()
    {
        transform.position = sphereRB.transform.position;

        speedInput = 0;
        if (Input.GetAxis("Vertical") > 0)
        {
            speedInput = Input.GetAxis("Vertical") * forwardAccel * 1000;
        }
        else if (Input.GetAxis("Vertical") < 0)
        {
            speedInput = Input.GetAxis("Vertical") * reverseAccel * 1000;
        }

        turnInput = Input.GetAxis("Horizontal");
        if (grounded)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, turnInput * turnStrength * Time.deltaTime * Input.GetAxis("Vertical"), 0f));
        }
    }

    private void FixedUpdate()
    {
        grounded = false;
        if (Physics.Raycast(groudRayPoint.position, -transform.up, out RaycastHit hit, groundRayLength, ground))
        {
            grounded = true;
            transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
        }

        if (grounded)
        {
            sphereRB.drag = dragOnGround;
            if (Mathf.Abs(speedInput) > 0)
            {
                sphereRB.AddForce(transform.forward * speedInput);
            }
        }
        else
        {
            sphereRB.drag = 0.1f;
            sphereRB.AddForce((Vector3.up * -extraGravity *100));
        }
    }
}
