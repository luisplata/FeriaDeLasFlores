﻿using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A container for the configuration data
/// </summary>
public class ConfigurationData
{
    #region Fields


    private const string ConfigurationDataFileName = "Configurations.csv";
    private Dictionary<ConfigurationDataValueName, float> values = new Dictionary<ConfigurationDataValueName, float>();

    #endregion

    #region Properties

    /// <summary>
    /// Warm up state duration in seconds
    /// </summary>
    public float WarmUpStateDuration
    {
        get { return values[ConfigurationDataValueName.WarmUpStateDuration]; }
    }

    /// <summary>
    /// Calibrate state duration in seconds
    /// </summary>
    public float CalibrateStateDuration
    {
        get { return values[ConfigurationDataValueName.CalibrateStateDuration]; }
    }

    /// <summary>
    /// Reward state duration in seconds
    /// </summary>
    public float RewardStateDuration
    {
        get { return values[ConfigurationDataValueName.RewardStateDuration]; }
    }

    /// <summary>
    /// Challenge state duration in seconds
    /// </summary>
    public float ChallengeStateDuration
    {
        get { return values[ConfigurationDataValueName.ChallengeStateDuration]; }
    }

    /// <summary>
    /// Rest state duration in seconds
    /// </summary>
    public float RestStateDuration
    {
        get { return values[ConfigurationDataValueName.RestStateDuration]; }
    }

    #endregion

    #region Constructor

    /// <summary>
    /// Constructor
    /// Reads configuration data from a file. If the file
    /// read fails, the object contains default values for
    /// the configuration data
    /// </summary>
    public ConfigurationData()
    {
        StreamReader file = null;
        try
        {
            file = File.OpenText(Path.Combine(Application.streamingAssetsPath, ConfigurationDataFileName));

            string currentLine = file.ReadLine();
            while (currentLine != null)
            {
                string[] tokens = currentLine.Split(',');
                ConfigurationDataValueName valueName = 
                    (ConfigurationDataValueName)Enum.Parse(typeof(ConfigurationDataValueName), tokens[0]);
                values.Add(valueName, float.Parse(tokens[1]));
                currentLine = file.ReadLine();
            }

        }
        catch(Exception e)
        {
            SetDeafultValues();
        }
        finally
        {
            if(file != null)
            {
                file.Close();
            }
        }
    }

    #endregion

    #region Methods

    private void SetDeafultValues()
    {
        values.Clear();
        values.Add(ConfigurationDataValueName.WarmUpStateDuration, 12);
        values.Add(ConfigurationDataValueName.CalibrateStateDuration, 5);
        values.Add(ConfigurationDataValueName.RewardStateDuration, 5);
        values.Add(ConfigurationDataValueName.ChallengeStateDuration, 10);
        values.Add(ConfigurationDataValueName.RestStateDuration, 5);
    }

    #endregion
}