using UnityEngine;

/// <summary>
/// Configurable values for PlayerController.
/// Create an instance via Assets → Create → Config → PlayerConfig
/// and assign it to the PlayerController in the inspector.
/// </summary>
[CreateAssetMenu(fileName = "PlayerConfig", menuName = "Config/PlayerConfig")]
public class PlayerConfigSO : ScriptableObject
{
    [Header("Lane Switching")]
    [Tooltip("Speed in units per second when switching lanes")]
    public float laneSwitchSpeed = 5f;

    [Tooltip("X-axis distance threshold for snapping to target lane position")]
    public float movementTolerance = 0.05f;

    [Header("Movement (reserved for future use)")]
    public float movementSpeed = 15f;

    [Header("Jump (reserved for future use)")]
    public float jumpHeight = 3f;
}
