using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    public Transform Player;

    public Transform left;
    public Transform Top;
    public Transform Bottom;
    public Transform right;

    public BoxCollider collider;
    public bool enableControlPoints;
    public bool StartFinishLine = false;

    private void Start()
    {
        Player = GameObject.Find("GoKart3(WheelColliderV2)").transform;
        collider.isTrigger = true;
        SetEnabled(false);
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Tracker"))
        {
            Debug.Log("Player Passed CheckPoint:" + GetInstanceID());
        }
    }

    private void SetEnabled(bool enabled)
    {
        left.gameObject.SetActive(enableControlPoints);
        Top.gameObject.SetActive(enableControlPoints);
        Bottom.gameObject.SetActive(enableControlPoints);
        right.gameObject.SetActive(enableControlPoints);
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            SetEnabled(false);
            return;
        }
        if (enableControlPoints)
        {

            float centreX = ((left.localPosition + right.localPosition) / 2).x;
            float centreY = (Bottom.localPosition.y + Top.localPosition.y) / 2;
            collider.center = new Vector3(centreX, centreY, 0f);
            collider.size = new Vector3(Vector3.Distance(left.localPosition, right.localPosition), Vector3.Distance(Bottom.localPosition, Top.localPosition), 1f);

        }

        SetEnabled(enableControlPoints);
        //Debug.DrawLine((Bottom.position + Top.position) / 2, Player.position, Color.white);
        //Debug.DrawLine(left.position, Player.position, Color.red);
        //Debug.DrawLine(right.position, Player.position, Color.green);
        //
        //Gizmos.DrawSphere(left.position,0.1f);
        //Gizmos.DrawSphere(Top.position, 0.1f);
        //Gizmos.DrawSphere(Bottom.position, 0.1f);
        //Gizmos.DrawSphere(right.position, 0.1f);

    }
}
