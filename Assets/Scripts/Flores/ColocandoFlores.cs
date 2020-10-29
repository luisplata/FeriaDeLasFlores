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
        if (RevisarSiPuedoColocarUnaNuevaFlorDeEseTag(listaRecuperada))
        {
            int posicionParaTomar = 0;
            GameObject referencia = null;

            try
            {
                posicionParaTomar = SiguienteNumeroDisponibleDeFlor(listaRecuperada);
                referencia = listaRecuperada[posicionParaTomar];
                referencia.GetComponent<MeshRenderer>().enabled = true;
            }
            catch(SinEspacioParaPonerMasFloresException e)
            {
                Debug.LogWarning("No puede pasar nada porque ya no hay espacio "+e.Message);
            }
        }
    }

    private int SiguienteNumeroDisponibleDeFlor(List<GameObject> listaRecuperada)
    {
        int i = 0;
        foreach(GameObject flor in listaRecuperada)
        {
            if (!flor.GetComponent<MeshRenderer>().enabled)
            {
                return i;
            }
            i++;
        }
        throw new SinEspacioParaPonerMasFloresException("No hay mas espacio para colocar mas flores");
    }

    private bool RevisarSiPuedoColocarUnaNuevaFlorDeEseTag(List<GameObject> listaRecuperada)
    {
        foreach(GameObject flor in listaRecuperada)
        {
            if (!flor.GetComponent<MeshRenderer>().enabled)
            {
                return true;
            }
        }
        return false;
    }
}
