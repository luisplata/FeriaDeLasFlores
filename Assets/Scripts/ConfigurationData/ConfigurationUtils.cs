using UnityEditorInternal;
using UnityEngine;

/// <summary>
/// Provides access to configuration data
/// </summary>
public static class ConfigurationUtils
{
    private static ConfigurationData configurationData;

    private static Vector3[] linesPositions;

    #region Properties

    /// <summary>
    /// Get the positions of each line in the game
    /// </summary>
    public static Vector3[] LinesPositions
    {
        get { return linesPositions; }
    }

    /// <summary>
    /// Warm up state duration in seconds
    /// </summary>
    public static float WarmUpStateDuration
    {
        get { return configurationData.WarmUpStateDuration; }
    }

    /// <summary>
    /// Calibrate state duration in seconds
    /// </summary>
    public static float CalibrateStateDuration
    {
        get { return configurationData.CalibrateStateDuration; }
    }

    /// <summary>
    /// Reward state duration in seconds
    /// </summary>
    public static float RewardStateDuration
    {
        get { return configurationData.RewardStateDuration; }
    }

    /// <summary>
    /// Challenge state duration in seconds
    /// </summary>
    public static float ChallengeStateDuration
    {
        get { return configurationData.ChallengeStateDuration; }
    }

    /// <summary>
    /// Rest state duration in seconds
    /// </summary>
    public static float RestStateDuration
    {
        get { return configurationData.RestStateDuration; }
    }

    /// <summary>
    /// Player movement speed
    /// </summary>
    public static float PlayerMovementSpeed
    {
        get { return configurationData.PlayerMovementSpeed; }
    }

    /// <summary>
    /// Player position tolerance to reach next position
    /// </summary>
    public static float PlayerMovementTolerance
    {
        get { return configurationData.PlayerMovementTolerance; }
    }

    #endregion

    /// <summary>
    /// Initializes the configuration utils
    /// </summary>
    public static void Initialize()
    {
        configurationData = new ConfigurationData();
        InitializeLinesPositions();
    }

    private static void InitializeLinesPositions()
    {
        GameObject[] lines = GameObject.FindGameObjectsWithTag(GameInitializer.LineTag);
        linesPositions = new Vector3[lines.Length];
        
        for(int i = 0; i < lines.Length; i++)
        {
            linesPositions[i] = lines[i].transform.position;
        }
    }
}
