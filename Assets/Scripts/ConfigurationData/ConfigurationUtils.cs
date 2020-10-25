using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Provides access to configuration data
/// </summary>
public static class ConfigurationUtils
{
    private static ConfigurationData configurationData;

    #region Properties

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

    #endregion

    /// <summary>
    /// Initializes the configuration utils
    /// </summary>
    public static void Initialize()
    {
        configurationData = new ConfigurationData();
    }
}
