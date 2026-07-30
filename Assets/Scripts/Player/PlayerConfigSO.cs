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

    [Header("Jump")]
    [Tooltip("Height in units of the jump arc peak relative to start Y")]
    public float jumpHeight = 3f;

    [Tooltip("Duration in seconds for a full jump arc (0 → peak → 0)")]
    public float jumpDuration = 0.5f;

    [Tooltip("Multiplier for descent speed when holding down mid-jump. 2 = twice as fast")]
    public float fastFallMultiplier = 2f;
}
