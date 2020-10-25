using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : IntEventInvoker
{
    private static Dictionary<GameState, float> gameStateDuration = new Dictionary<GameState, float>();

    private GameState currentGameState;
    private Timer currentStateTimer;

    private GameStateChangedEvent gameStateChangedEvent;

    public GameState GameState
    {
        get { return GameState; }
        set { currentGameState = value; }
    }

    private void Awake()
    {
        InitializeGameStateDuration();
    }

    private void Start()
    {
        currentGameState = GameState.WarmUp;
        gameStateChangedEvent = new GameStateChangedEvent();

        currentStateTimer = gameObject.AddComponent<Timer>();
        currentStateTimer.Duration = GetGameStateDuration(currentGameState);
        //currentStateTimer.AddListener()
    }

    private void InitializeGameStateDuration()
    {
        const float defaultDuration = 5f;

        foreach (GameState eventName in Enum.GetValues(typeof(GameState)))
        {
            float duration = defaultDuration;

            if(eventName == GameState.WarmUp)
            {
                duration = ConfigurationUtils.WarmUpStateDuration;
            }
            else if(eventName == GameState.Calibrate)
            {
                duration = ConfigurationUtils.CalibrateStateDuration;
            }
            else if (eventName == GameState.Reward)
            {
                duration = ConfigurationUtils.RewardStateDuration;
            }
            else if (eventName == GameState.Challenge)
            {
                duration = ConfigurationUtils.ChallengeStateDuration;
            }
            else if (eventName == GameState.Rest)
            {
                duration = ConfigurationUtils.RestStateDuration;
            }

            gameStateDuration.Add(eventName, duration);
        }

    }

    //private void HandleGameStateChanged

    public static float GetGameStateDuration(GameState gameState)
    {
        return gameStateDuration[gameState];
    }

}
