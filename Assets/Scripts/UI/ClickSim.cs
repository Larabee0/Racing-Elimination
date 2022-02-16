using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickSim : StandaloneInputModule
{
    public void ClickAt(float x, float y)
    {
        Input.simulateMouseWithTouches = true;
        var pointerData = GetTouchPointerEventData(new Touch()
        {
            position = new Vector3(x, y),
        }, out bool b, out bool bb);
        ProcessTouchPress(pointerData, true, false);
        ProcessTouchPress(pointerData, false, true);
        enabled = false;
    }
}
