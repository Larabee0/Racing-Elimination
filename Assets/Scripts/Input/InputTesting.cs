using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class InputTesting : MonoBehaviour
{
    private Rigidbody sphereRigibody;
    TestActions testActions;
    private void Awake()
    {
        sphereRigibody = GetComponent<Rigidbody>();

        testActions = new TestActions();
        testActions.Player.Enable();
        testActions.Player.Jump.performed += Jump;
    }

    private void FixedUpdate()
    {
        Vector2 inVector = testActions.Player.Movement.ReadValue<Vector2>();
        float speed = 5f;
        sphereRigibody.AddForce(new Vector3(inVector.x, 0, inVector.y) * speed, ForceMode.Force);
    }

    public void Jump(InputAction.CallbackContext context)
    {
        Debug.Log(context);
        if (context.performed)
        {
            Debug.Log("Jump" + context.phase);
            sphereRigibody.AddForce(Vector3.up * 100f, ForceMode.Impulse);
        }
    }
}
