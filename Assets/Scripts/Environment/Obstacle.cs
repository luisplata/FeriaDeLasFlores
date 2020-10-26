using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    private void FixedUpdate()
    {
        if(transform.position.z < Camera.main.transform.position.z)
        {
            transform.SetParent(null);
            gameObject.SetActive(false);
        }
    }
}
