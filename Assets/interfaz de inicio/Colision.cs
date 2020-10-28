using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Colision : MonoBehaviour
{
    [SerializeField] private Button botonJugar;
    [SerializeField] private int escenaCargar;
    // Start is called before the first frame update
    void Start()
    {
        botonJugar.onClick.AddListener(delegate { AcccionDeBoton(); });
    }

    private void AcccionDeBoton()
    {
        SceneManager.LoadScene(escenaCargar);
    }
  
}
