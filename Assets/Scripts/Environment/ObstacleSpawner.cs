using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] private EnvironmentMovement environmentMovement;
    [SerializeField] private GameObject[] spawnPositions;
    [SerializeField] private List<GameObject> obstaclesPrefabs = new List<GameObject>();

    private int poolObjectsInstances = 10;
    private float spawnSecondsMin = 2f;
    private float spawnSecondsMax = 3f;
    private List<ObstaclePool> obstacles = new List<ObstaclePool>();
    private class ObstaclePool
    {
        public List<GameObject> objectPool;
        private ObstacleSpawner obstacleSpawner;
        private CountdownTimer spawnTimer;

        public ObstaclePool(ObstacleSpawner obstacleSpawner, List<GameObject> objectPool, CountdownTimer spawnTimer)
        {
            this.obstacleSpawner = obstacleSpawner;
            this.objectPool = objectPool;
            this.spawnTimer = spawnTimer;
            spawnTimer.AddTimerFinishedEventListener(HandleSpawnTimerFinished);
        }

        private void HandleSpawnTimerFinished()
        {
            obstacleSpawner.SpawnObstacles(this);
            RunSpawnTimer();
        }

        public void RunSpawnTimer()
        {
            spawnTimer.Duration = Random.Range(obstacleSpawner.spawnSecondsMin, obstacleSpawner.spawnSecondsMax);
            spawnTimer.Run();
        }

        public GameObject GetAvailableObstacle()
        {
            foreach(GameObject gameObject in objectPool)
            {
                if (!gameObject.activeSelf)
                {
                    return gameObject;
                }
            }

            return null;
        }
    }

    private void Awake()
    {
        foreach(GameObject prefab in obstaclesPrefabs)
        {
            string name = prefab.name;
            List<GameObject> objectPool = new List<GameObject>();
            for(int i=0; i < poolObjectsInstances; i++)
            {
                GameObject currentInstantiatedObject = Instantiate(prefab);
                currentInstantiatedObject.SetActive(false);
                objectPool.Add(currentInstantiatedObject);
            }
            CountdownTimer spawnTimer = gameObject.AddComponent<CountdownTimer>();
            ObstaclePool newObstacle = new ObstaclePool(this, objectPool, spawnTimer);
            obstacles.Add(newObstacle);
        }
    }

    private void Start()
    {
        EventManager.AddListener(EventName.GameOverEvent, HandleGameOver);

        foreach(ObstaclePool obstacle in obstacles)
        {
            obstacle.RunSpawnTimer();
        }
    }

    private void SpawnObstacles(ObstaclePool obstacle)
    {
        int objectsToSpawn = Random.Range(1, spawnPositions.Length + 1);
        int spawnPositionIndex = Random.Range(0, spawnPositions.Length);
        
        for (int i=0; i < objectsToSpawn; i++)
        {
            GameObject spawnPosition = spawnPositions[spawnPositionIndex];
            GameObject obstacleGameObject = obstacle.GetAvailableObstacle();

            if (!obstacleGameObject)
            {
                return;
            }

            obstacleGameObject.transform.position = spawnPosition.transform.position;
            obstacleGameObject.SetActive(true);
            obstacleGameObject.transform.SetParent(environmentMovement.NonVisibleFloor.transform);

            spawnPositionIndex += 1;
            spawnPositionIndex %= spawnPositions.Length;
        }
    }

    private void HandleGameOver(int unused)
    {
        enabled = false;
    }
}
