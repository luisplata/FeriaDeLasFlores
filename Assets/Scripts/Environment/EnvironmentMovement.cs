using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentMovement : IntEventInvoker
{
    [SerializeField] private Floor[] floor = new Floor[2];
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

    private void Start()
    {
        unityEvents.Add(EventName.FloorChangeEvent, floorChangeEvent);
        EventManager.AddInvoker(EventName.FloorChangeEvent, this);
    }

    private void Update()
    {
        float positionToCompare = floor[0].InitPosition.z;

        if (!floor[0].gameObject.activeSelf)
        {
            positionToCompare = 90f;
        }

        if(floor[1].transform.position.z <= positionToCompare)
        {
            if (!floor[0].gameObject.activeSelf)
            {
                floor[0].gameObject.SetActive(true);
                floor[0].InitPosition = new Vector3(0, 0, 90f);
            }

            floor[0].transform.position = floor[1].InitPosition;

            Vector3 floor0InitPosition = floor[0].InitPosition;
            floor[0].InitPosition = floor[1].InitPosition;
            floor[1].InitPosition = floor0InitPosition;

            Floor floor0 = floor[0];
            floor[0] = floor[1];
            floor[1] = floor0;

            floorChangeEvent.Invoke(1);
        }
    }
}
