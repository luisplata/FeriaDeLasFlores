using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : IntEventInvoker
{
    private static Dictionary<GameState, float> gameStateDuration;

    private GameState currentGameState;
    private CountdownTimer currentStateTimer;

    private GameStateChangedEvent gameStateChangedEvent = new GameStateChangedEvent();

    private bool gameOver = false;

    public GameState GameState
    {
        get { return GameState; }
        set { currentGameState = value; }
    }

    private void Awake()
    {
        gameStateDuration = new Dictionary<GameState, float>();
        InitializeGameStateDuration();
    }

    private void Start()
    {
        currentGameState = GameState.WarmUp;

        unityEvents.Add(EventName.GameStateChangedEvent, gameStateChangedEvent);
        EventManager.AddInvoker(EventName.GameStateChangedEvent, this);

        currentStateTimer = gameObject.AddComponent<CountdownTimer>();
        currentStateTimer.AddTimerFinishedEventListener(HandleGameStateFinished);
        RunStateTimer();

        EventManager.AddListener(EventName.GameOverEvent, HandleGameOverEvent);
    }

    private void InitializeGameStateDuration()
    {
        const float defaultDuration = 5f;

        foreach (GameState eventName in Enum.GetValues(typeof(GameState)))
        {
            float duration = defaultDuration;

            switch (eventName)
            {
                case GameState.WarmUp:
                    duration = ConfigurationUtils.WarmUpStateDuration;
                    break;
                case GameState.Calibrate:
                    duration = ConfigurationUtils.CalibrateStateDuration;
                    break;
                case GameState.Reward:
                    duration = ConfigurationUtils.RewardStateDuration;
                    break;
                case GameState.Challenge:
                    duration = ConfigurationUtils.ChallengeStateDuration;
                    break;
                case GameState.Rest:
                    duration = ConfigurationUtils.RestStateDuration;
                    break;

            }

            gameStateDuration.Add(eventName, duration);
            
        }

    }

    private void RunStateTimer()
    {
        currentStateTimer.Duration = GetGameStateDuration(currentGameState);
        currentStateTimer.Run();
    }

    private void HandleGameStateFinished()
    {
        if (gameOver)
        {
            return;
        }

        switch (currentGameState)
        {
            case GameState.WarmUp:
                currentGameState = GameState.Calibrate;
                break;
            case GameState.Calibrate:
                currentGameState = GameState.Reward;
                break;
            case GameState.Reward:
                currentGameState = GameState.Challenge;
                break;
            case GameState.Challenge:
                currentGameState = GameState.Rest;
                break;
            case GameState.Rest:
                currentGameState = GameState.Challenge;
                break;

        }
        gameStateChangedEvent.Invoke((int)currentGameState);
        RunStateTimer();
        
    }

    public static float GetGameStateDuration(GameState gameState)
    {
        return gameStateDuration[gameState];
    }

    private void HandleGameOverEvent(int unused)
    {
        gameOver = true;
    }
}
