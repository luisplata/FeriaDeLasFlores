using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnvironmentChanger : MonoBehaviour
{
    [SerializeField] private List<EnvironmentMovement> forestEnvironments = new List<EnvironmentMovement>();
    [SerializeField] private List<EnvironmentMovement> streetEnvironments = new List<EnvironmentMovement>();
    [SerializeField] private List<EnvironmentMovement> tramEnvironments = new List<EnvironmentMovement>();
    private List<EnvironmentMovement> currentEnvironmentList;
    private List<EnvironmentMovement> nextEnvironmentList;
    private Queue<List<EnvironmentMovement>> nextEnvironments = new Queue<List<EnvironmentMovement>>();
    private GameState currentGameState;
    private GameState nextGameState;
    private Queue<GameState> nextGameStates = new Queue<GameState>();

    private bool mustEndChange;

    private void Start()
    {
        currentEnvironmentList = forestEnvironments;
        currentGameState = GameState.WarmUp;
        EventManager.AddListener(EventName.GameStateChangedEvent, HandleGameStateChange);
        EventManager.AddListener(EventName.FloorChangeEvent, ChangeEnvironment);
        EventManager.AddListener(EventName.GameOverEvent, HandleGameOverEvent);
        EventManager.AddListener(EventName.EnvironmentChangedEvent, HandleEnvironmentChangeEvent);
    }

    private void HandleGameStateChange(int gameStateInt)
    {
        nextGameStates.Enqueue((GameState)gameStateInt);
        nextGameState = nextGameStates.Peek();
    }

    private void ChangeEnvironment(int unused)
    {
        EnvironmentMovement currentEnvironment = currentEnvironmentList[(int)currentGameState];

        if (mustEndChange)
        {
            currentEnvironment.gameObject.SetActive(false);

            if (nextEnvironmentList != null && nextGameState == GameState.Rest)
            {
                currentEnvironmentList = nextEnvironmentList;
                nextEnvironments.Dequeue();
                nextEnvironmentList = null;
            }

            currentGameState = nextGameState;
            nextGameStates.Dequeue();
            nextGameState = GameState.None;
            mustEndChange = false;
            return;
        }

        if (nextGameState == GameState.None)
        {
            return;
        }

        if(nextEnvironments.Count > 0)
        {
            nextEnvironmentList = nextEnvironments.Peek();
        }

        EnvironmentMovement nextEnvironment = currentEnvironmentList[(int)nextGameState];
        if (nextEnvironmentList != null && nextGameState == GameState.Rest)
        {
            nextEnvironment = nextEnvironmentList[(int)nextGameState];
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

    private void HandleEnvironmentChangeEvent(int environmentNumber)
    {
        EnvironmentName environmentName = (EnvironmentName)environmentNumber;
        if (environmentName == EnvironmentName.Forest)
        {
            nextEnvironments.Enqueue(forestEnvironments);
        }
        else if(environmentName == EnvironmentName.Street)
        {
            nextEnvironments.Enqueue(streetEnvironments);
        }
        else if (environmentName == EnvironmentName.Tram)
        {
            nextEnvironments.Enqueue(tramEnvironments);
        }
    }
}
