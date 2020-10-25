using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool {
    public Queue<GameObject> objectPool = new Queue<GameObject>();
    public int numberOfInstancesPerRow;
    private int currentCycleObjectsInstanced;

    public ObjectPool(int numberOfInstancesPerRow)
    {
        this.numberOfInstancesPerRow = numberOfInstancesPerRow;
    }
    public void InstantiateObject(Vector3 position)
    {
        GameObject gameObject = Dequeue();
        gameObject.SetActive(true);
        gameObject.transform.position = position;
        Enqueue(gameObject);
        currentCycleObjectsInstanced += 1;
    }

    public GameObject Dequeue()
    {
        return objectPool.Dequeue();
    }

    public void Enqueue(GameObject gameObject)
    {
        objectPool.Enqueue(gameObject);
    }
}
