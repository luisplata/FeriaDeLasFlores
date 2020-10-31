using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System;

public class ControladorDePuntuacion : MonoBehaviour
{
    [SerializeField] private ColocandoFlores colocadorDeFlores;
    [SerializeField] private PuntuacionUiController controladorDeUi;
    private Dictionary<string, PuntuacionUI> puntuacionesPorFlor = new Dictionary<string, PuntuacionUI>();

    public void AumentoDePuntuacion(FlorDelMapa florTocada)
    {
        colocadorDeFlores.ColocarFlor(florTocada.gameObject.tag);
        ActualizarPuntuacion(florTocada);
        controladorDeUi.MarcoPuncuacion();
    }

    private void ActualizarPuntuacion(FlorDelMapa florTomada)
    {
        if(puntuacionesPorFlor.TryGetValue(florTomada.gameObject.tag, out PuntuacionUI puntuacionActual))
        {
            puntuacionActual.puntuacion++;
            puntuacionesPorFlor.Remove(florTomada.gameObject.tag);
            puntuacionesPorFlor.Add(florTomada.gameObject.tag, puntuacionActual);
        }
        else
        {
            PuntuacionUI puntuacionActualQueNoEsta = new PuntuacionUI
            {
                imagenUI = florTomada.FlorUi(),
                puntuacion = 1
            };
            puntuacionesPorFlor.Add(florTomada.gameObject.tag, puntuacionActualQueNoEsta);
        }

        ActualizarPuntuacionUI(puntuacionesPorFlor);
    }

    private void ActualizarPuntuacionUI(Dictionary<string, PuntuacionUI> puntuacionesPorFlor)
    {
        int i = 0;
        foreach (KeyValuePair<string, PuntuacionUI> entry in puntuacionesPorFlor)
        {
            i++;
        }
    }
}