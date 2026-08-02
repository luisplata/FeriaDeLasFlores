using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

/// <summary>
/// EditMode tests for ObstacleSpawner, InstanciadorDeFlores, FlorDelMapa,
/// and ColocandoFlores.
/// Tests obstacle lane positioning, flower object pooling, and collection behavior.
/// </summary>
public class SpawnerTests
{
    // Lane X positions matching PlayerControllerTests
    private const float LeftX = -5f;
    private const float CenterX = 0f;
    private const float RightX = 5f;

    // Expected spawn constants
    private const float ObstacleY = 0.5f;
    private const float ObstacleZ = 200f;


    private GameObject playerObject;
    private PlayerController playerController;
    private Transform leftPos;
    private Transform centerPos;
    private Transform rightPos;

    private GameObject spawnerGO;
    private ObstacleSpawner spawner;
    private GameObject envGO;
    private EnvironmentMovement environmentMovement;
    private Floor nonVisibleFloor;
    private readonly List<GameObject> tempObstacles = new List<GameObject>();

    private GameObject flowerGO;
    private InstanciadorDeFlores instanciador;

    [SetUp]
    public void SetUp()
    {
        ConfigurationUtils.Initialize();

        // ── PlayerController setup (same pattern as PlayerControllerTests) ──
        playerObject = new GameObject("TestPlayer");
        playerController = playerObject.AddComponent<PlayerController>();
        playerObject.AddComponent<Rigidbody>();

        leftPos = CreatePosition("Left", LeftX);
        centerPos = CreatePosition("Center", CenterX);
        rightPos = CreatePosition("Right", RightX);

        playerController.leftPosition = leftPos;
        playerController.centerPosition = centerPos;
        playerController.rightPosition = rightPos;
        playerController.InitializeLaneSystem();

        // ── Environment setup ──
        envGO = new GameObject("EnvMovement");
        environmentMovement = envGO.AddComponent<EnvironmentMovement>();
        var floorGO = new GameObject("NonVisibleFloor");
        nonVisibleFloor = floorGO.AddComponent<Floor>();
        environmentMovement.NonVisibleFloor = nonVisibleFloor;

        // ── ObstacleSpawner setup ──
        spawnerGO = new GameObject("ObstacleSpawner");
        spawner = spawnerGO.AddComponent<ObstacleSpawner>();
        spawner.playerController = playerController;
        spawner.environmentMovement = environmentMovement;
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var obs in tempObstacles)
        {
            if (obs != null)
                Object.DestroyImmediate(obs);
        }
        tempObstacles.Clear();

        if (flowerGO != null)
            Object.DestroyImmediate(flowerGO);

        Object.DestroyImmediate(spawnerGO);
        Object.DestroyImmediate(envGO);
        Object.DestroyImmediate(playerObject);

        if (leftPos != null)
            Object.DestroyImmediate(leftPos.gameObject);
        if (centerPos != null)
            Object.DestroyImmediate(centerPos.gameObject);
        if (rightPos != null)
            Object.DestroyImmediate(rightPos.gameObject);
    }

    // ── Helpers ──────────────────────────────────────────────

    private Transform CreatePosition(string name, float x)
    {
        var go = new GameObject(name);
        go.transform.position = new Vector3(x, 0, 0);
        return go.transform;
    }

    /// <summary>
    /// Creates an ObstaclePool with the specified number of inactive GameObjects.
    /// The pool's timer is added to spawnerGO so it gets cleaned up with the spawner.
    /// </summary>
    private ObstacleSpawner.ObstaclePool CreatePool(int obstacleCount)
    {
        var poolList = new List<GameObject>();
        for (int i = 0; i < obstacleCount; i++)
        {
            var obs = new GameObject($"Obstacle{i}");
            obs.SetActive(false);
            poolList.Add(obs);
            tempObstacles.Add(obs);
        }
        var timer = spawnerGO.AddComponent<CountdownTimer>();
        return new ObstacleSpawner.ObstaclePool(spawner, poolList, timer);
    }

    /// <summary>
    /// Deactivates all obstacles in the given pool so they can be re-spawned.
    /// </summary>
    private void DeactivatePool(ObstacleSpawner.ObstaclePool pool)
    {
        foreach (var obs in pool.objectPool)
        {
            obs.SetActive(false);
        }
    }

    /// <summary>
    /// Returns the lane index (0, 1, 2) for a given X position.
    /// </summary>
    private int GetLaneIndex(float x)
    {
        if (Mathf.Approximately(x, LeftX)) return 0;
        if (Mathf.Approximately(x, CenterX)) return 1;
        if (Mathf.Approximately(x, RightX)) return 2;
        return -1; // unknown
    }

    // ──────────────────────────────────────────────
    //  Task 1 — ObstacleSpawner lane positioning
    // ──────────────────────────────────────────────

    /// <summary>
    /// Spec: "Obstacles MUST spawn at one of the 3 lane X positions
    /// (leftPosition, centerPosition, rightPosition) with y=0.5f and z=200f."
    /// </summary>
    [Test]
    public void ObstaclesSpawnAtLaneXPositions()
    {
        var pool = CreatePool(3);
        float[] laneXPositions = { LeftX, CenterX, RightX };

        spawner.SpawnObstacles(pool);

        int spawnedCount = 0;
        foreach (var obs in pool.objectPool)
        {
            if (!obs.activeSelf) continue;
            spawnedCount++;

            Assert.That(obs.transform.position.x,
                Is.EqualTo(LeftX).Or.EqualTo(CenterX).Or.EqualTo(RightX),
                $"Active obstacle X ({obs.transform.position.x}) must match a lane position");
            Assert.That(obs.transform.position.y, Is.EqualTo(ObstacleY).Within(0.001f),
                $"Obstacle Y must be {ObstacleY}");
            Assert.That(obs.transform.position.z, Is.EqualTo(ObstacleZ).Within(0.001f),
                $"Obstacle Z must be {ObstacleZ}");
        }

        Assert.That(spawnedCount, Is.GreaterThan(0),
            "At least one obstacle should have been spawned");
    }

    /// <summary>
    /// Triangulation: obstacles are parented to NonVisibleFloor after spawn.
    /// </summary>
    [Test]
    public void ObstaclesAreParentedToNonVisibleFloor()
    {
        var pool = CreatePool(3);

        spawner.SpawnObstacles(pool);

        bool anyParented = false;
        foreach (var obs in pool.objectPool)
        {
            if (!obs.activeSelf) continue;
            Assert.That(obs.transform.parent, Is.EqualTo(nonVisibleFloor.transform),
                "Active obstacle should be parented to NonVisibleFloor");
            anyParented = true;
        }

        Assert.That(anyParented, Is.True,
            "At least one obstacle should have been spawned and parented");
    }

    /// <summary>
    /// Triangulation: playerController fallback via FindObjectOfType works.
    /// </summary>
    [Test]
    public void ObstacleSpawnerFindsPlayerControllerViaFallback()
    {
        // Create a fresh spawner with NO playerController set
        var freshSpawnerGO = new GameObject("FreshSpawner");
        var freshSpawner = freshSpawnerGO.AddComponent<ObstacleSpawner>();
        freshSpawner.environmentMovement = environmentMovement;
        tempObstacles.Add(freshSpawnerGO); // ensure cleanup

        var pool = CreatePool(1);

        // The playerController is null, so SpawnObstacles should
        // fall back to FindObjectOfType<PlayerController>() — which finds our test player
        freshSpawner.SpawnObstacles(pool);

        foreach (var obs in pool.objectPool)
        {
            if (!obs.activeSelf) continue;
            Assert.That(obs.transform.position.x,
                Is.EqualTo(LeftX).Or.EqualTo(CenterX).Or.EqualTo(RightX),
                "Fallback-spawned obstacle should be at a lane position");
        }
    }

    // ──────────────────────────────────────────────
    //  Task 3 — Independent lane selection (not sequential)
    // ──────────────────────────────────────────────

    /// <summary>
    /// Spec: "Each obstacle picks a random lane INDEPENDENTLY — not
    /// consecutive-wrapped from a start index. With 3 obstacles spawning
    /// across multiple trials, we should observe duplicate lane indices
    /// (proving independence from old sequential logic)."
    /// </summary>
    [Test]
    public void ObstaclesUseIndependentRandomLanesNotSequential()
    {
        // With the old sequential logic, starting at a random position and
        // spawning 3 obstacles out of 3 positions would ALWAYS produce
        // one obstacle per lane (no duplicates). With independent random selection,
        // duplicates WILL occur.
        // We run many trials and check for any duplicate lane occurrence.

        bool foundDuplicateRun = false;
        const int trials = 30;

        for (int t = 0; t < trials; t++)
        {
            var pool = CreatePool(3);
            DeactivatePool(pool);

            spawner.SpawnObstacles(pool);

            // Collect lane indices of active obstacles
            var laneIndices = new List<int>();
            foreach (var obs in pool.objectPool)
            {
                if (!obs.activeSelf) continue;
                int lane = GetLaneIndex(obs.transform.position.x);
                laneIndices.Add(lane);
            }

            // Check for duplicates
            if (laneIndices.Count >= 2)
            {
                var unique = new HashSet<int>(laneIndices);
                if (unique.Count < laneIndices.Count)
                {
                    foundDuplicateRun = true;
                    break;
                }
            }
        }

        Assert.That(foundDuplicateRun, Is.True,
            "After 30 trials, at least one should have duplicate lane indices " +
            "(proving independence from sequential wrapping)");
    }

    /// <summary>
    /// Triangulation: across many spawn calls, we observe multiple distinct lane indices
    /// (not always the same lane).
    /// </summary>
    [Test]
    public void ObstaclesSpawnAcrossMultipleLanes()
    {
        var allLanesSeen = new HashSet<int>();
        const int trials = 30;

        for (int t = 0; t < trials; t++)
        {
            var pool = CreatePool(3);
            DeactivatePool(pool);

            spawner.SpawnObstacles(pool);

            foreach (var obs in pool.objectPool)
            {
                if (!obs.activeSelf) continue;
                int lane = GetLaneIndex(obs.transform.position.x);
                allLanesSeen.Add(lane);
            }
        }

        Assert.That(allLanesSeen.Count, Is.GreaterThan(1),
            "Across 30 trials, obstacles should appear in at least 2 different lanes " +
            $"(saw {allLanesSeen.Count} unique lanes, expected 2 or 3)");
    }

    // ──────────────────────────────────────────────
    //  Task 4 — InstanciadorDeFlores object pool (flower-environment-alignment)
    // ──────────────────────────────────────────────

    /// <summary>
    /// Spec: "Awake creates a pool of N inactive flowers parented to NonVisibleFloor."
    /// Verifies the pool is created during Awake with all flowers inactive.
    /// </summary>
    [Test]
    public void InstanciadorCreaPoolEnAwake()
    {
        flowerGO = new GameObject("FlowerSpawner");
        flowerGO.SetActive(false);
        instanciador = flowerGO.AddComponent<InstanciadorDeFlores>();
        instanciador.playerController = playerController;
        instanciador.escalar = 1f;
        instanciador.poolSize = 6;

        var flowerPrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
        flowerPrefab.SetActive(false);
        instanciador.listaDeFLoresParaInstanciar = new List<GameObject> { flowerPrefab };
        tempObstacles.Add(flowerPrefab);

        instanciador.listaDeMaterialesParaLasFlores = new List<Material>();
        instanciador.listaDeMaterialesParaLasFlores.Add(new Material(Shader.Find("Standard")));

        flowerGO.SetActive(true); // Triggers Awake

        int childCount = nonVisibleFloor.transform.childCount;
        Assert.That(childCount, Is.EqualTo(6),
            $"NonVisibleFloor should have {instanciador.poolSize} child flowers after Awake");

        foreach (Transform child in nonVisibleFloor.transform)
        {
            Assert.That(child.gameObject.activeSelf, Is.False,
                "Pooled flowers must be inactive after Awake");
        }
    }

    /// <summary>
    /// Spec: "SpawnFlowers activates flowers at lane X positions, Z=200."
    /// Verifies flowers are positioned correctly after SpawnFlowers call.
    /// </summary>
    [Test]
    public void SpawnFlowersActivaFloresEnZ200()
    {
        flowerGO = new GameObject("FlowerSpawner");
        flowerGO.SetActive(false);
        instanciador = flowerGO.AddComponent<InstanciadorDeFlores>();
        instanciador.playerController = playerController;
        instanciador.escalar = 1f;
        instanciador.poolSize = 6;
        instanciador.spawnZ = 200f;
        instanciador.spawnY = 0f;

        var flowerPrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
        flowerPrefab.SetActive(false);
        instanciador.listaDeFLoresParaInstanciar = new List<GameObject> { flowerPrefab };
        tempObstacles.Add(flowerPrefab);

        instanciador.listaDeMaterialesParaLasFlores = new List<Material>();
        instanciador.listaDeMaterialesParaLasFlores.Add(new Material(Shader.Find("Standard")));

        flowerGO.SetActive(true); // Triggers Awake

        instanciador.SpawnFlowers(3);

        int activeCount = 0;
        foreach (Transform child in nonVisibleFloor.transform)
        {
            if (!child.gameObject.activeSelf) continue;
            activeCount++;

            Assert.That(child.position.z, Is.EqualTo(200f).Within(0.001f),
                $"Spawned flower Z should be {instanciador.spawnZ}");

            Assert.That(child.position.x,
                Is.EqualTo(LeftX).Or.EqualTo(CenterX).Or.EqualTo(RightX),
                "Spawned flower X must be at a lane position");
        }

        Assert.That(activeCount, Is.GreaterThan(0),
            "At least one flower should be active after SpawnFlowers");
    }

    // ──────────────────────────────────────────────
    //  Task 5 — FlorDelMapa uses SetActive instead of Destroy
    // ──────────────────────────────────────────────

    /// <summary>
    /// Spec: "FlorDelMapa.OnTriggerEnter calls SetActive(false) instead of Destroy."
    /// Verifies the flower GameObject is deactivated but not destroyed after collection.
    /// </summary>
    [Test]
    public void OnTriggerEnterDesactivaEnLugarDeDestruir()
    {
        // ── Initialize AudioManager ──
        var audioGO = new GameObject("AudioSource");
        var source = audioGO.AddComponent<AudioSource>();
        AudioManager.Initialize(source);

        // Seed the audio clips dictionary with a dummy clip via reflection
        var clipsField = typeof(AudioManager).GetField("audioClips",
            BindingFlags.Static | BindingFlags.NonPublic);
        var clipsDict = clipsField.GetValue(null) as Dictionary<AudioClipName, AudioClip>;
        var dummyClip = AudioClip.Create("FlowerPickup", 1, 1, 44100, false);
        clipsDict[AudioClipName.FlowerPickup] = dummyClip;

        // ── Create PuntuacionUiController with debeEntrarDeNuevo=false to skip director ──
        var uiGO = new GameObject("PuntuacionUI");
        var uiController = uiGO.AddComponent<PuntuacionUiController>();
        typeof(PuntuacionUiController).GetField("debeEntrarDeNuevo",
            BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(uiController, false);

        // ── Create ColocandoFlores ──
        var colocadorGO = new GameObject("Colocador");
        colocadorGO.SetActive(false);
        var colocador = colocadorGO.AddComponent<ColocandoFlores>();
        typeof(ColocandoFlores).GetField("tagsDeFlores",
            BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(colocador, new List<string>());
        colocadorGO.SetActive(true);

        // ── Create ControladorDePuntuacion with private fields set via reflection ──
        var playerGO = new GameObject("Player", typeof(BoxCollider));
        playerGO.tag = "Player";
        var puntuacion = playerGO.AddComponent<ControladorDePuntuacion>();
        typeof(ControladorDePuntuacion).GetField("colocadorDeFlores",
            BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(puntuacion, colocador);
        typeof(ControladorDePuntuacion).GetField("controladorDeUi",
            BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(puntuacion, uiController);

        // ── Create FlorDelMapa ──
        var florGO = new GameObject("Flor", typeof(BoxCollider));
        var flor = florGO.AddComponent<FlorDelMapa>();

        // ── Trigger collection ──
        var collider = playerGO.GetComponent<BoxCollider>();
        flor.OnTriggerEnter(collider);

        // ── Assert: flower is inactive but still exists ──
        Assert.That(florGO.activeSelf, Is.False,
            "Flower should be deactivated (SetActive(false)), not destroyed");
        Assert.That(florGO, Is.Not.Null,
            "Flower GameObject should still exist (not destroyed)");

        // ── Cleanup ──
        Object.DestroyImmediate(florGO);
        Object.DestroyImmediate(playerGO);
        Object.DestroyImmediate(colocadorGO);
        Object.DestroyImmediate(uiGO);
        Object.DestroyImmediate(audioGO);
    }

    // ──────────────────────────────────────────────
    //  Task 6 — ColocandoFlores null guard in ColocarFlor
    // ──────────────────────────────────────────────

    /// <summary>
    /// Spec: "ColocarFlor with unknown tag returns gracefully without throwing."
    /// Verifies the null guard prevents NPE when the tag is not in the dictionary.
    /// </summary>
    [Test]
    public void ColocarFlorConTagDesconocidoRetornaSinError()
    {
        var go = new GameObject("Colocador");
        go.SetActive(false);
        var colocador = go.AddComponent<ColocandoFlores>();

        // Set tagsDeFlores to empty list so Start() doesn't NPE
        typeof(ColocandoFlores).GetField("tagsDeFlores",
            BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(colocador, new List<string>());

        go.SetActive(true); // Start runs with empty list

        // ColocarFlor with unknown tag should not throw
        Assert.DoesNotThrow(() => colocador.ColocarFlor("UnknownTag"),
            "ColocarFlor should return gracefully when tag is not in the dictionary");

        Object.DestroyImmediate(go);
    }
}
