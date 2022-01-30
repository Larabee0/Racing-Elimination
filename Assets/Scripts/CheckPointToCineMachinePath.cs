using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class CheckPointToCineMachinePath : MonoBehaviour
{
    public CinemachineSmoothPath path;
    public GameObject checkPointContainer;
    [Range(-10f, 10f)]
    public float HeightOffset = 0f;
    public bool UpdatePath = false;

    private void OnDrawGizmos()
    {
        if (UpdatePath && path != null && checkPointContainer != null)
        {
            CheckPoint[] points = checkPointContainer.GetComponentsInChildren<CheckPoint>();
            if(points == null || points.Length == 0)
            {
                return;
            }
            path.m_Waypoints = new CinemachineSmoothPath.Waypoint[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                Vector3 pos = points[i].transform.position;
                pos.y = HeightOffset;
                path.m_Waypoints[i] = new CinemachineSmoothPath.Waypoint { position = pos, roll = 0f };
            }
        }
    }
}
