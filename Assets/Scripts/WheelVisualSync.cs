using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelVisualSync : MonoBehaviour
{
    public WheelCollider wheelCollider;

    // Update is called once per frame
    void Update()
    {
        wheelCollider.GetWorldPose(out Vector3 pos, out Quaternion rot);
        transform.position = pos;
        transform.rotation = rot;
    }
}
