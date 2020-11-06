using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Train : MonoBehaviour
{
    private float movementSpeed = -40f;

    private Rigidbody rigidBody;

    private void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }
    private void Update()
    {
        Vector3 position = transform.position;
        position.z += movementSpeed * Time.deltaTime;
        transform.position = position;
    }

}
