using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Floor : MonoBehaviour
{
    private float movementSpeed;

    private Vector3 initPosition;

    public Vector3 InitPosition
    {
        get { return initPosition; }
        set { initPosition = value; }
    }

    private void Start()
    {
        initPosition = transform.position;
        movementSpeed = ConfigurationUtils.FloorInitialMovementSpeed;
    }

    private void FixedUpdate()
    {
        Vector3 position = transform.position;
        position.z += movementSpeed;
        transform.position = position;
    }
}
