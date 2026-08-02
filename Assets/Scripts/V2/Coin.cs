using UnityEngine;

public class Coin : MonoBehaviour
{
    [HideInInspector] public CoinSpawner spawner;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Avisar al spawner
            spawner?.CoinCollected(this);
        }
    }
}