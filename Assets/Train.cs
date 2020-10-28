using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Train : MonoBehaviour
{
    [SerializeField]
    private bool invertMovement;

    [SerializeField]
    private float movementSpeed = -10f;

    private Rigidbody rigidBody;

    private void Start()
    {
        movementSpeed *= invertMovement ? -1 : 1;
        rigidBody = GetComponent<Rigidbody>();
    }
    private void Update()
    {
        rigidBody.velocity = Vector3.zero;
        rigidBody.AddForce(new Vector3(0, 0, movementSpeed), ForceMode.Impulse);
    }

}
