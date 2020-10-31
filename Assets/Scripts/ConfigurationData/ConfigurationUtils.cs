using UnityEngine;

/// <summary>
/// Provides access to configuration data
/// </summary>
public static class ConfigurationUtils
{
    private static ConfigurationData configurationData;

    #region Properties

    public static int TotalFlowers { get; set; }
    public static int CollectedFlowers { get; set; }
    public static int PlaySeconds { get; set; }
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
    /// Player jump height
    /// </summary>
    public static float PlayerJumpHeight
    {
        get { return configurationData.PlayerJumpHeight; }
    }

    /// <summary>
    /// Player position tolerance to reach next position
    /// </summary>
    public static float PlayerMovementTolerance
    {
        get { return configurationData.PlayerMovementTolerance; }
    }

    /// <summary>
    /// Initial floor movement speed
    /// </summary>
    public static float FloorMovementSpeed
    {
        get { return configurationData.FloorMovementSpeed; }
        set { configurationData.FloorMovementSpeed = value; }
    }

    #endregion

    /// <summary>
    /// Initializes the configuration utils
    /// </summary>
    public static void Initialize()
    {
        configurationData = new ConfigurationData();
    }
}
