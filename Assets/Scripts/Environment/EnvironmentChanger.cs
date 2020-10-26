using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentChanger : MonoBehaviour
{
    [SerializeField] private List<EnvironmentMovement> environments = new List<EnvironmentMovement>();
    private EnvironmentMovement currentEnvironment;
    private EnvironmentMovement nextEnvironement;

    private bool mustEndChange;

    private void Start()
    {
        currentEnvironment = environments[0];
        EventManager.AddListener(EventName.GameStateChangedEvent, HandleGameStateChange);
        EventManager.AddListener(EventName.FloorChangeEvent, ChangeEnvironment);
    }

    private void HandleGameStateChange(int gameStateInt)
    {
        nextEnvironement = environments[gameStateInt];
    }

    private void ChangeEnvironment(int unused)
    {
        if (mustEndChange)
        {

            currentEnvironment.gameObject.SetActive(false);

            currentEnvironment = nextEnvironement;
            nextEnvironement = null;
            mustEndChange = false;
        }

        if (nextEnvironement == null)
        {
            return;
        }

        nextEnvironement.gameObject.SetActive(true);
        nextEnvironement.NonVisibleFloor.gameObject.SetActive(true);
        nextEnvironement.VisibleFloor.gameObject.SetActive(false);

        currentEnvironment.NonVisibleFloor.gameObject.SetActive(false);

        mustEndChange = true;
    }
}
