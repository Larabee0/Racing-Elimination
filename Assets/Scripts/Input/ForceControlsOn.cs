using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceControlsOn : MonoBehaviour
{
    void Start()
    {
        GetComponent<UniversalInput>().actions.UI.Disable();
        GetComponent<UniversalInput>().actions.Player.Enable();
    }
}
