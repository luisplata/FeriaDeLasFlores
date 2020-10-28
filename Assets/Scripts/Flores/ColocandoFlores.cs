using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColocandoFlores : MonoBehaviour
{
    [SerializeField] private List<string> tagsDeFlores;
    private Dictionary<string, List<GameObject>> listasDeFlores;
    private List<int> posicionesYaTomadas;
    
    private void Start()
    {
        posicionesYaTomadas = new List<int>();
        listasDeFlores = new Dictionary<string, List<GameObject>>();
        foreach (string tagFlor in tagsDeFlores)
        {
            List<GameObject> lista = new List<GameObject>();
            foreach(GameObject florEncontrada in GameObject.FindGameObjectsWithTag(tagFlor))
            {
                florEncontrada.GetComponent<MeshRenderer>().enabled = false;
                lista.Add(florEncontrada);
            }
            listasDeFlores.Add(tagFlor, lista);
        }
    }

    public void ColocarFlor(string tagDeLaFlor)
    {
        listasDeFlores.TryGetValue(tagDeLaFlor, out List<GameObject> listaRecuperada);

        int posicionParaTomar = 0;
        do
        {
            posicionParaTomar = Random.Range(0, listaRecuperada.Count - 1);
        } while (posicionesYaTomadas.Contains(posicionParaTomar) && posicionesYaTomadas.Count < listaRecuperada.Count);

        posicionesYaTomadas.Add(posicionParaTomar);

        GameObject referencia = listaRecuperada[posicionParaTomar];
        referencia.GetComponent<MeshRenderer>().enabled = true;
        Debug.Log("Instancio");
    }
}
