﻿using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class PuntuacionUiController : MonoBehaviour
{
    [SerializeField] private PlayableDirector director;
    [SerializeField] private PlayableAsset entrada, salida;
    [SerializeField] private PlayerController player;
    [SerializeField] private TextMeshProUGUI texto;
    [SerializeField] private Image imagen;
    [SerializeField] private bool debeEntrarDeNuevo = true;
    [SerializeField] private float deltaTimeLocal;
    [SerializeField] private float tiempoDeEspera = 1;
    [SerializeField] private float aumentoDeTiempoDeEspera;
    private bool ejecutarCorrutina = true;

    private void Update()
    {
        texto.text = player.FlorEnPorcenajeParaEscribir + "%";
        imagen.fillAmount = player.FlorEnPorcentajeParaUi;

        if (!debeEntrarDeNuevo)
        {
            deltaTimeLocal += Time.deltaTime;
            if (deltaTimeLocal >= tiempoDeEspera)
            {
                director.playableAsset = salida;
                director.Play();
                if(ejecutarCorrutina)
                    StartCoroutine(ControladorDeSemaforo((float)salida.duration));
            }
        }
    }

    public void MarcoPuncuacion()
    {
        if (debeEntrarDeNuevo)
        {
            director.playableAsset = entrada;
            director.Play();
            debeEntrarDeNuevo = false;
        }
        tiempoDeEspera += aumentoDeTiempoDeEspera;
    }

    IEnumerator ControladorDeSemaforo(float tiempo)
    {
        ejecutarCorrutina = false;
        Debug.Log("se ejecuta al tiempo que la animacion de salida");
        yield return new WaitForSeconds(tiempo);
        Debug.Log("se ejecuta despues de la animacion de salida");
        deltaTimeLocal = 0;
        tiempoDeEspera = 1;
        debeEntrarDeNuevo = true;
        ejecutarCorrutina = true;
    }
}