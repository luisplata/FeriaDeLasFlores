using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Puntuacion : MonoBehaviour
{
    [SerializeField] private Image flor;
    [SerializeField] TextMeshProUGUI puntuacion;

    private void Awake()
    {
        flor.color = new Color(0,0,0,0);
        puntuacion.text = "";
    }

    public void ActualizarImagen(Sprite florDeImagen)
    {
        if(flor.color.a <= 10)
        {
            flor.color = new Color(1, 1, 1, 1);
        }
        flor.sprite = florDeImagen;
    }

    public void ActualizarPuntuacion(string puntuacion)
    {
        this.puntuacion.text = puntuacion;
    }
}
