using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstanciadorDeFlores : MonoBehaviour
{
    [SerializeField] private List<GameObject> listaDeFLoresParaInstanciar;
    [SerializeField] private List<Material> listaDeMaterialesParaLasFlores;
    [SerializeField] private List<GameObject> posicionesParaInstanciarFlores;
    [SerializeField] float escalar;

    private void OnEnable()
    {
        foreach(GameObject posicion in posicionesParaInstanciarFlores)
        {
            GameObject flor = listaDeFLoresParaInstanciar[Random.Range(0, listaDeFLoresParaInstanciar.Count)];
            flor = Instantiate(flor, posicion.transform);
            flor.transform.position = posicion.transform.position;
            flor.transform.localScale = new Vector3(escalar, escalar, escalar);
            flor.GetComponent<MeshRenderer>().material = listaDeMaterialesParaLasFlores[Random.Range(0, listaDeMaterialesParaLasFlores.Count)];
        }
    }
}
