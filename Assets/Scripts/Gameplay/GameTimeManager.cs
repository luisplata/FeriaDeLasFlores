using System.Collections;
using System.Collections.Generic;
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
        ConfigurationUtils.FloorMovementSpeed = initialFloorMovementSpeed - (gameTimer.ElapsedSeconds / 180) ;
    }
}
