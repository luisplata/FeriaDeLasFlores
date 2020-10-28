using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColocandoFlores : MonoBehaviour
{
    [SerializeField] private List<string> tagsDeFlores;
    private Dictionary<string, List<GameObject>> listasDeFlores;
    
    private void Start()
    {
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

    }
}
