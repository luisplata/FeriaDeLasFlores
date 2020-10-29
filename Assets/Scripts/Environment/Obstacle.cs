using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    private void Start()
    {
        EventManager.AddListener(EventName.GameOverEvent, HandleGameOver);
    }

    private void FixedUpdate()
    {
        if(transform.position.z < Camera.main.transform.position.z)
        {
            transform.SetParent(null);
            gameObject.SetActive(false);
        }
    }

    private void HandleGameOver(int unused)
    {
        gameObject.SetActive(false);
    }
}
