using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnvironmentChanger : MonoBehaviour
{
    [SerializeField] private List<EnvironmentMovement> environments = new List<EnvironmentMovement>();
    private EnvironmentMovement currentEnvironment;
    private EnvironmentMovement nextEnvironment;
    private Queue<EnvironmentMovement> nextEnvironements = new Queue<EnvironmentMovement>();

    private bool mustEndChange;

    private void Start()
    {
        currentEnvironment = environments[0];
        EventManager.AddListener(EventName.GameStateChangedEvent, HandleGameStateChange);
        EventManager.AddListener(EventName.FloorChangeEvent, ChangeEnvironment);

        EventManager.AddListener(EventName.GameOverEvent, HandleGameOverEvent);
    }

    private void HandleGameStateChange(int gameStateInt)
    {
        nextEnvironements.Enqueue(environments[gameStateInt]);
        nextEnvironment = nextEnvironements.Peek();
    }

    private void ChangeEnvironment(int unused)
    {
        if (mustEndChange)
        {

            currentEnvironment.gameObject.SetActive(false);

            currentEnvironment = nextEnvironment;
            nextEnvironements.Dequeue();
            nextEnvironment = null;
            mustEndChange = false;
            return;
        }

        if (!nextEnvironment)
        {
            return;
        }

        nextEnvironment.gameObject.SetActive(true);
        nextEnvironment.VisibleFloor.gameObject.SetActive(false);
        nextEnvironment.NonVisibleFloor.gameObject.SetActive(true);

        currentEnvironment.NonVisibleFloor.gameObject.SetActive(false);

        mustEndChange = true;
    }

    private void HandleGameOverEvent(int unused)
    {
        enabled = false;
    }
}
