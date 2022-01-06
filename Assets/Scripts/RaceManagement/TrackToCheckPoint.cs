using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
public class TrackToCheckPoint : MonoBehaviour
{
    public PathCreator  path;
    [Range(1, 50)]
    public float resolution = 1;

    public CheckPoint trackerPrefab;
    public Transform player;
    //private void OnDrawGizmos()
    //{
    //    float length = path.path.length;
    //    for (float i = 0; i < length; i+= resolution)
    //    {
    //        Vector3 point = path.path.GetPointAtDistance(i);
    //        Gizmos.DrawSphere(point, 0.5f);
    //    }
    //}

    public void PlaceCheckPoints()
    {
        float length = path.path.length;
        for (float i = 0; i < length; i += resolution)
        {
            Vector3 point = path.path.GetPointAtDistance(i);
            Vector3 dir = point - path.path.GetPointAtDistance(i + resolution);
            Spawn(point, dir);
        }
    }

    private void Spawn(Vector3 Pos, Vector3 forward)
    {
        CheckPoint tracker = Instantiate( trackerPrefab,Pos,Quaternion.identity,transform);
        tracker.transform.forward = forward;
        tracker.Player = player;
    }

}
