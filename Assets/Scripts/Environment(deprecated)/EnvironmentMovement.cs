using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentMovement : MonoBehaviour
{
    [SerializeField] private float movementSpeed;

    private void FixedUpdate()
    {
        Vector3 position = transform.position;
        position.z += movementSpeed;
        transform.position = position;
    }
}
