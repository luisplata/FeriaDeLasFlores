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
        movementSpeed = ConfigurationUtils.FloorMovementSpeed;
        EventManager.AddListener(EventName.GameOverEvent, HandleGameOver);
    }

    private void FixedUpdate()
    {
        Vector3 position = transform.position;
        position.z += movementSpeed;
        transform.position = position;
    }

    private void HandleGameOver(int unused)
    {
        enabled = false;
    }
}
