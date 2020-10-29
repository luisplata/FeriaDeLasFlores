using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;

public class ControladorDePuntuacion : MonoBehaviour
{
    [SerializeField] private ColocandoFlores colocadorDeFlores;
    public void AumentoDePuntuacion(FlorDelMapa florTocada)
    {
        colocadorDeFlores.ColocarFlor(florTocada.gameObject.tag);
        ActualizarPuntuacion(florTocada);
    }

    [SerializeField] private TextMeshProUGUI puntuacion;
    private Dictionary<string, int> puntuacionesPorFlor = new Dictionary<string, int>();
    private void ActualizarPuntuacion(FlorDelMapa florTomada)
    {
        if(puntuacionesPorFlor.TryGetValue(florTomada.gameObject.tag, out int puntuacionActual))
        {
            puntuacionActual++;
            puntuacionesPorFlor.Remove(florTomada.gameObject.tag);
            puntuacionesPorFlor.Add(florTomada.gameObject.tag, puntuacionActual);
        }
        else
        {
            puntuacionesPorFlor.Add(florTomada.gameObject.tag, 1);
        }
        ActualizarPuntuacionUI(puntuacionesPorFlor);
    }

    private void ActualizarPuntuacionUI(Dictionary<string, int> puntuacionesPorFlor)
    {
        string puntuacionT = "Puntuacion: ";
        foreach (KeyValuePair<string, int> entry in puntuacionesPorFlor)
        {
            puntuacionT += entry.Key + ": " + entry.Value+" ";
        }
        puntuacion.text = puntuacionT;
    }
}