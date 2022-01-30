using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate void KartPassedCheckPoint(KartPassedCheckPointArgs f);
public struct KartPassedCheckPointArgs
{
    public CheckPoint sender;
    public ArcadeKart kart;
}
public class CheckPoint : MonoBehaviour
{
    //public Transform Player;

    public Vector3 LeftPos { get { return left.transform.position; } }
    public Vector3 RightPos { get { return left.transform.position; } }
    public Vector3 CentrePos { get { return transform.position; } }

    public bool LiveEditing;
    public Vector3 LeftControlPoint;
    public Vector3 RightControlPoint;
    public Vector3 TopControlPoint;
    public Vector3 BottomControlPoint;

    public Transform left;
    public Transform Top;
    public Transform Bottom;
    public Transform right;
    [Range(0.1f, 2f)]
    public float colliderLength = 0.25f;
    [Range(0.01f, 2f)]
    public float measurePointDst = 0.5f;
    public List<Vector3> points = new List<Vector3>();
    public new BoxCollider collider;
    public bool enableControlPoints;
    public bool StartFinishLine = false;
    public int index;
    public KartPassedCheckPoint OnKartPassedCheckPoint;
    public KartPassedCheckPoint OnKartEnterCheckPoint;


    private void Start()
    {
        collider.isTrigger = true;
        enableControlPoints = false;
        SetEnabled();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Tracker"))
        {
            //Debug.Log("Kart Passed CheckPoint:" + GetInstanceID());
            OnKartPassedCheckPoint?.Invoke(new KartPassedCheckPointArgs
            {
                sender = this,
                kart = other.gameObject.GetComponentInParent<ArcadeKart>()
            });
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Tracker"))
        {
            //Debug.Log("Kart Passed CheckPoint:" + GetInstanceID());
            OnKartEnterCheckPoint?.Invoke(new KartPassedCheckPointArgs
            {
                sender = this,
                kart = other.gameObject.GetComponentInParent<ArcadeKart>()
            });
        }
    }

    private void SetEnabled()
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
            SetEnabled();
            return;
        }
        if (enableControlPoints)
        {
            if (LiveEditing)
            {
                left.transform.localPosition = LeftControlPoint;
                right.transform.localPosition = RightControlPoint;
                Top.transform.localPosition = TopControlPoint;
                Bottom.transform.localPosition = BottomControlPoint;
            }
            else
            {
                LeftControlPoint = left.transform.localPosition;
                RightControlPoint = right.transform.localPosition;
                TopControlPoint = Top.transform.localPosition;
                BottomControlPoint = Bottom.transform.localPosition;
            }


            float centreX = ((left.localPosition + right.localPosition) / 2).x;
            float centreY = (Bottom.localPosition.y + Top.localPosition.y) / 2;
            collider.center = new Vector3(centreX, centreY, 0f);
            collider.size = new Vector3(Vector3.Distance(left.localPosition, right.localPosition), Vector3.Distance(Bottom.localPosition, Top.localPosition), colliderLength);
            points.Clear();
            for (float i = 0; i <= 1f; i+=measurePointDst)
            {
                Vector3 point = Vector3.Lerp(left.localPosition, right.localPosition,i);
                point = transform.TransformPoint(point);
                point.y = 0f;
                points.Add(point);
                Gizmos.DrawSphere(point, measurePointDst * 0.75f);
            }
        }

        SetEnabled();
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
