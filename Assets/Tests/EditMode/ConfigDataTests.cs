using NUnit.Framework;

/// <summary>
/// EditMode tests for the PlayerLaneSwitchSpeed configuration pipeline.
/// RED phase: these tests reference code that does not exist yet.
/// </summary>
public class ConfigDataTests
{
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        ConfigurationUtils.Initialize();
    }

    /// <summary>
    /// Verifies ConfigurationDataValueName.PlayerLaneSwitchSpeed is defined.
    /// Spec: "The system MUST support a PlayerLaneSwitchSpeed configuration value."
    /// </summary>
    [Test]
    public void PlayerLaneSwitchSpeedEnumValueExists()
    {
        bool exists = System.Enum.IsDefined(typeof(ConfigurationDataValueName), "PlayerLaneSwitchSpeed");
        Assert.That(exists, Is.True,
            "ConfigurationDataValueName.PlayerLaneSwitchSpeed must be defined in the enum");
    }

    /// <summary>
    /// Verifies ConfigurationUtils.PlayerLaneSwitchSpeed returns 5f as the default.
    /// Spec: "PlayerLaneSwitchSpeed MUST default to 5 units per second."
    /// </summary>
    [Test]
    public void PlayerLaneSwitchSpeedDefaultIsFive()
    {
        float speed = ConfigurationUtils.PlayerLaneSwitchSpeed;
        Assert.That(speed, Is.EqualTo(5f),
            "PlayerLaneSwitchSpeed should default to 5f");
    }

    /// <summary>
    /// Verifies the accessor returns a positive value through ConfigurationUtils.
    /// Spec: "The lane switch speed MUST be a positive value."
    /// </summary>
    [Test]
    public void PlayerLaneSwitchSpeedAccessorReturnsPositiveValue()
    {
        float speed = ConfigurationUtils.PlayerLaneSwitchSpeed;
        Assert.That(speed, Is.GreaterThan(0f),
            "PlayerLaneSwitchSpeed must be accessible via ConfigurationUtils and return a positive value");
    }
}
