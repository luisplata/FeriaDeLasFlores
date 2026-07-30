using System.Collections.Generic;
using UnityEngine;

public class InstanciadorDeFlores : MonoBehaviour
{
    [SerializeField] internal List<GameObject> listaDeFLoresParaInstanciar;
    [SerializeField] internal List<Material> listaDeMaterialesParaLasFlores;
    [SerializeField] internal PlayerController playerController;
    [SerializeField] internal float escalar;
    [SerializeField] internal int poolSize = 24;
    [SerializeField] internal int spawnCount = 6;
    [SerializeField] internal int groups = 3;
    [SerializeField] internal float groupSpacing = 100f;
    [SerializeField] internal float spawnZ = 200f;
    [SerializeField] internal float spawnZSpread = 30f;
    [SerializeField] internal float recycleZ = -50f;
    [SerializeField] internal float spawnY = 0f;

    private Queue<GameObject> flowerPool;
    private bool initialized;

    private void Awake()
    {
        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>();

        flowerPool = new Queue<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject prefab = listaDeFLoresParaInstanciar[i % listaDeFLoresParaInstanciar.Count];
            GameObject flower = Instantiate(prefab, transform);
            flower.SetActive(false);
            flowerPool.Enqueue(flower);
        }

        initialized = true;
    }

    private void OnEnable()
    {
        SpawnFlowers(spawnCount);
    }

    private void Update()
    {
        if (!initialized) return;
        float speed = ConfigurationUtils.FloorMovementSpeed;
        if (Mathf.Approximately(speed, 0f)) return;

        Vector3 movement = new Vector3(0f, 0f, speed * Time.deltaTime);
        foreach (Transform child in transform)
        {
            if (!child.gameObject.activeSelf) continue;

            child.position += movement;

            // Recycle: deactivate when past recycle distance
            if (child.position.z < recycleZ)
                child.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Spawns flowers from the pool at lane positions with Z = spawnZ.
    /// </summary>
    internal void SpawnFlowers(int count = 4)
    {
        if (!initialized) return;
        if (playerController == null)
            playerController = FindObjectOfType<PlayerController>();
        if (playerController == null) return;

        int totalNeeded = count * groups;
        int spawned = 0;
        int checkedCount = 0;
        int maxChecks = flowerPool.Count;

        while (spawned < totalNeeded && checkedCount < maxChecks && flowerPool.Count > 0)
        {
            GameObject flower = flowerPool.Dequeue();
            checkedCount++;

            if (flower == null || flower.activeSelf)
            {
                flowerPool.Enqueue(flower);
                continue;
            }

            int groupIndex = spawned / count;
            int flowerInGroup = spawned % count;
            int laneIndex = flowerInGroup % 3;

            Transform laneTransform = playerController.GetLaneTransform(laneIndex);
            float baseZ = spawnZ + groupIndex * groupSpacing;
            float zOffset = spawnZSpread > 0f ? Random.Range(0f, spawnZSpread) : 0f;
            Vector3 pos = new Vector3(laneTransform.position.x, spawnY, baseZ + zOffset);

            flower.transform.position = pos;
            flower.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            flower.transform.localScale = Vector3.one * escalar;
            flower.GetComponent<MeshRenderer>().material =
                listaDeMaterialesParaLasFlores[Random.Range(0, listaDeMaterialesParaLasFlores.Count)];

            flower.SetActive(true);
            var renderer = flower.GetComponent<MeshRenderer>();
            if (renderer != null) renderer.enabled = true;
            spawned++;
            flowerPool.Enqueue(flower);
        }
    }
}