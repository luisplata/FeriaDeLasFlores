using UnityEngine;

/// <summary>
/// Initializes the game
/// </summary>
public class GameInitializer : MonoBehaviour
{
    public const string HorizontalAxis = "Horizontal";
    public const string VerticalAxis = "Vertical";

    public const string LineTag = "Line";

    public const string PauseMenuPrefab = "PauseMenu";

    public const string MainMenuScene = "MainMenu";

	void Awake()
    {
        ConfigurationUtils.Initialize();
        EventManager.Initialize();
    }
}