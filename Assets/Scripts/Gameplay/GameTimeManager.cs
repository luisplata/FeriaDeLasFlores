using UnityEngine;

public class GameTimeManager : MonoBehaviour
{
    private GameTimer gameTimer;
    private float initialFloorMovementSpeed;

    private void Start()
    {
        initialFloorMovementSpeed = ConfigurationUtils.FloorMovementSpeed;
        gameTimer = gameObject.AddComponent<GameTimer>();
        gameTimer.Run();
    }

    private void Update()
    {
        ConfigurationUtils.PlaySeconds = (int)gameTimer.ElapsedSeconds;
        ConfigurationUtils.FloorMovementSpeed = initialFloorMovementSpeed - (gameTimer.ElapsedSeconds / 5);
    }
}
