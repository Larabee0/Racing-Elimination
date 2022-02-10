using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
public class FrameRate : MonoBehaviour
{
    public Text FrameTime;
    // Update is called once per frame
    void Update()
    {
        FrameTime.text = (Time.deltaTime *1000f).ToString("0.00") +"ms";
    }
}
