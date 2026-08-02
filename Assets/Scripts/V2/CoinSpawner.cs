using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CoinSpawner : MonoBehaviour
{
    public Action<int> OnCoinCollected;
    [Header("Referencias")] public FloorManager floorManager;
    public GameObject coinPrefab;

    [Header("Configuración")] public int coinsPerGroup = 3;
    public float groundY = 0.5f;
    public float airY = 2.5f;

    [Header("Carriles (X)")] public float leftLaneX = -3f;
    public float centerLaneX = 0f;
    public float rightLaneX = 3f;

    // Lista de posiciones locales (relativas al tile)
    private List<Vector3> localPositions = new List<Vector3>();

    // Grupos de monedas: cada grupo es una lista de Coin (corresponde a un tile)
    private List<List<Coin>> coinGroups = new List<List<Coin>>();

    // Contador de monedas recogidas
    public int totalCoins { get; private set; }

    private void Awake()
    {
        // Definir posiciones locales (dentro del tile)
        // Asumimos que el tile tiene origen en el centro en X y Z.
        // Ajusta zOffset si quieres desplazar las monedas en Z dentro del tile.
        float zOffset = 0f;
        localPositions.Clear();
        localPositions.Add(new Vector3(leftLaneX, groundY, zOffset));
        localPositions.Add(new Vector3(centerLaneX, groundY, zOffset));
        localPositions.Add(new Vector3(rightLaneX, groundY, zOffset));
        localPositions.Add(new Vector3(leftLaneX, airY, zOffset));
        localPositions.Add(new Vector3(centerLaneX, airY, zOffset));
        localPositions.Add(new Vector3(rightLaneX, airY, zOffset));
    }

    public void Configure()
    {
        if (floorManager == null)
            floorManager = FindObjectOfType<FloorManager>();

        if (floorManager == null)
        {
            Debug.LogError("CoinSpawner: No se encontró FloorManager.");
            enabled = false;
            return;
        }

        // Suscribirse al evento que se dispara cuando un tile es reciclado
        floorManager.OnRecycleWithIndex.AddListener(OnTileRecycled);

        // Crear un grupo de monedas para cada tile
        int tileCount = floorManager.tiles.Length;
        for (int i = 0; i < tileCount; i++)
        {
            List<Coin> group = new List<Coin>();
            for (int j = 0; j < coinsPerGroup; j++)
            {
                GameObject coinObj = Instantiate(coinPrefab);
                coinObj.SetActive(false);
                Coin coin = coinObj.GetComponent<Coin>();
                if (coin == null)
                    coin = coinObj.AddComponent<Coin>();
                coin.spawner = this;
                group.Add(coin);
            }

            coinGroups.Add(group);
        }

        // Poblar todos los tiles inicialmente, excepto el índice 0
        for (int i = 0; i < tileCount; i++)
        {
            if (i <= 1) continue; // Saltar el tile donde empieza el jugador y el siguiente
            Transform tileTransform = floorManager.tiles[i].transform;
            SpawnCoinsForGroup(i, tileTransform);
        }
    }

    // Cuando un tile es reciclado (se reposiciona al frente)
    private void OnTileRecycled(int tileIndex)
    {
        // Primero, limpiar el grupo de ese tile (desactivar monedas viejas)
        ClearGroup(tileIndex);

        // Luego, colocar nuevas monedas en ese tile (que ya ha sido reposicionado al frente)
        Transform tileTransform = floorManager.tiles[tileIndex].transform;
        SpawnCoinsForGroup(tileIndex, tileTransform);
    }

    // Limpia un grupo (desactiva todas las monedas)
    private void ClearGroup(int groupIndex)
    {
        if (groupIndex < 0 || groupIndex >= coinGroups.Count) return;
        foreach (Coin coin in coinGroups[groupIndex])
        {
            if (coin.gameObject.activeSelf)
                coin.gameObject.SetActive(false);
        }
    }

    // Coloca monedas en un tile específico, usando el grupo correspondiente
    private void SpawnCoinsForGroup(int groupIndex, Transform tileTransform)
    {
        List<Coin> group = coinGroups[groupIndex];
        // Desactivar todas por si acaso
        foreach (Coin coin in group)
            coin.gameObject.SetActive(false);

        // Seleccionar coinsPerGroup posiciones aleatorias sin repetir
        List<Vector3> selectedPositions = GetRandomPositions(coinsPerGroup);
        if (selectedPositions.Count < coinsPerGroup)
        {
            Debug.LogWarning("No hay suficientes posiciones para llenar el grupo.");
            return;
        }

        // Asignar posiciones a las monedas del grupo
        for (int i = 0; i < coinsPerGroup; i++)
        {
            if (i < group.Count)
            {
                // Hacer la moneda hija del tile para que se mueva con él
                group[i].transform.SetParent(tileTransform, false); // false = mantener posición local
                // Asignar posición local (relativa al tile)
                group[i].transform.localPosition = selectedPositions[i];
                group[i].transform.localRotation = Quaternion.identity;
                group[i].gameObject.SetActive(true);
            }
        }
    }

    // Selecciona N posiciones aleatorias de la lista localPositions sin repetir
    private List<Vector3> GetRandomPositions(int count)
    {
        List<Vector3> available = new List<Vector3>(localPositions);
        List<Vector3> result = new List<Vector3>();
        for (int i = 0; i < count && available.Count > 0; i++)
        {
            int idx = Random.Range(0, available.Count);
            result.Add(available[idx]);
            available.RemoveAt(idx);
        }

        return result;
    }

    // Método llamado cuando una moneda es recogida (desde Coin)
    public void CoinCollected(Coin coin)
    {
        totalCoins++;
        Debug.Log("Moneda recogida! Total: " + totalCoins);

        // Desactivar la moneda (no destruir)
        coin.gameObject.SetActive(false);
        OnCoinCollected?.Invoke(totalCoins);

        // Aquí puedes añadir efectos de sonido, partículas, actualizar UI, etc.
    }
}