using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColocandoFlores : MonoBehaviour
{
    [SerializeField] private List<GameObject> floresDeColorAzul, floresInstanciadas;
    [SerializeField] private GameObject florAzul;
    private List<int> posicionesYaTomadas;

    private void Awake()
    {
        floresDeColorAzul = new List<GameObject>();
        floresInstanciadas = new List<GameObject>();
        posicionesYaTomadas = new List<int>();
    }
    private void Start()
    {
        foreach (GameObject flor in GameObject.FindGameObjectsWithTag("FlorAzul"))
        {
            floresDeColorAzul.Add(flor);
        }
    }

    public void ColocarFlor()
    {
        int posicionParaTomar = 0;
        do
        {
            posicionParaTomar = Random.Range(0, floresDeColorAzul.Count - 1);
        }while (posicionesYaTomadas.Contains(posicionParaTomar));

        posicionesYaTomadas.Add(posicionParaTomar);

        GameObject referencia = floresDeColorAzul[posicionParaTomar];
        floresInstanciadas.Add(Instantiate(florAzul, referencia.transform.position, referencia.transform.rotation, referencia.transform.parent));
        Debug.Log("Instancio");
    }
}
