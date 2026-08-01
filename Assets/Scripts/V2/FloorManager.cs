using UnityEngine;
using UnityEngine.Events;

public class FloorManager : MonoBehaviour
{
    [Header("Referencias")]
    public WorldMover[] tiles;
    public Transform player;

    [Header("Configuración")]
    public float tileLength = 42f;
    public float recycleOffset = 0f;

    [Header("Eventos")]
    public UnityEvent OnRecycle;
    public UnityEvent<int> OnRecycleWithIndex;

    private int headIndex = 0;

    // Propiedad pública para acceder al índice reciclado
    public int LastRecycledIndex { get; private set; }

    public void Configurate(WorldMover[] tilesArray, Transform playerRef,
                            float tileLen, float offset)
    {
        tiles = tilesArray;
        player = playerRef;
        tileLength = tileLen;
        recycleOffset = offset;
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
            else
                Debug.LogError("FloorManager: No se encontró Player.");
        }

        if (tiles == null || tiles.Length == 0)
        {
            Debug.LogError("FloorManager: No hay tiles.");
            enabled = false;
            return;
        }

        System.Array.Sort(tiles, (a, b) => a.transform.position.z.CompareTo(b.transform.position.z));

        foreach (var t in tiles)
        {
            if (t == null) Debug.LogWarning("FloorManager: Tile null.");
            else if (t.GetComponent<WorldMover>() == null)
                Debug.LogWarning("FloorManager: " + t.name + " no tiene WorldMover.");
        }
    }

    private void Update()
    {
        if (player == null || tiles.Length < 2) return;

        int nextIndex = (headIndex + 1) % tiles.Length;
        float playerZ = player.position.z;

        if (tiles[nextIndex].transform.position.z <= playerZ + recycleOffset)
        {
            int recycledIndex = headIndex;
            RecycleTile(recycledIndex);
            headIndex = (headIndex + 1) % tiles.Length;

            LastRecycledIndex = recycledIndex;

            OnRecycle?.Invoke();
            OnRecycleWithIndex?.Invoke(recycledIndex);
        }
    }

    private void RecycleTile(int index)
    {
        int lastIndex = (headIndex + tiles.Length - 1) % tiles.Length;
        float newZ = tiles[lastIndex].transform.position.z + tileLength;
        Vector3 pos = tiles[index].transform.position;
        pos.z = newZ;
        tiles[index].transform.position = pos;
    }
}