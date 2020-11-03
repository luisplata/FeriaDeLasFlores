using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentMovement : IntEventInvoker
{
    [SerializeField] private Floor[] floor = new Floor[2];
    [SerializeField] private GameObject spawner;

    private float floor0ZInitialPosition;

    private FloorChangeEvent floorChangeEvent = new FloorChangeEvent();
    public Floor NonVisibleFloor
    {
        get { return floor[1]; }
        set { floor[1] = value; }
    }

    public Floor VisibleFloor
    {
        get { return floor[0]; }
        set { floor[0] = value; }
    }

    public GameObject Spawner
    {
        get { return spawner; }
    }


    private void Start()
    {
        floor0ZInitialPosition = VisibleFloor.transform.position.z;

        unityEvents.Add(EventName.FloorChangeEvent, floorChangeEvent);
        EventManager.AddInvoker(EventName.FloorChangeEvent, this);

        EventManager.AddListener(EventName.GameOverEvent, HandleGameOverEvent);
    }
    bool asd = false;

    private void FixedUpdate()
    {
        float positionToCompare = floor[0].InitPosition.z;

        if (!floor[0].gameObject.activeSelf)
        {
            positionToCompare = floor0ZInitialPosition;
        }

        if(floor[1].transform.position.z <= positionToCompare)
        {
            if (!floor[0].gameObject.activeSelf)
            {
                spawner.gameObject.SetActive(true);
                floor[0].gameObject.SetActive(true);
                floor[0].InitPosition = new Vector3(0, 0, floor0ZInitialPosition);
            }

            floor[0].transform.position = new Vector3(floor[1].InitPosition.x, floor[1].InitPosition.y, floor[1].InitPosition.z - (positionToCompare - floor[1].transform.position.z));
            Debug.Log(floor[1].transform.position);
            Debug.Log(floor[0].transform.position);

            Vector3 floor0InitPosition = floor[0].InitPosition;
            floor[0].InitPosition = floor[1].InitPosition;
            floor[1].InitPosition = floor0InitPosition;

            Floor floor0 = floor[0];
            floor[0] = floor[1];
            floor[1] = floor0;

            floorChangeEvent.Invoke(1);
        }
    }

    private void HandleGameOverEvent(int unused)
    {
        enabled = false;
    }
}
