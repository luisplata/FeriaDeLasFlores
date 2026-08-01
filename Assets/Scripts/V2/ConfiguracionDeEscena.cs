using UnityEngine;
using V2;

public class ConfiguracionDeEscena : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Instance.ConfigurarCompletada();
        GameManager.Instance.StartGame();
    }
}