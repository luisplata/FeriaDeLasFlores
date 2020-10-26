using UnityEngine;

/// <summary>
/// Initializes the game
/// </summary>
public class GameInitializer : MonoBehaviour
{
	void Awake()
    {
        ConfigurationUtils.Initialize();
        EventManager.Initialize();
    }
}