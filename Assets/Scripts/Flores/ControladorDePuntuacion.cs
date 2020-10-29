using UnityEngine;

public class ControladorDePuntuacion : MonoBehaviour
{
    [SerializeField] private ColocandoFlores colocadorDeFlores;
    public void AumentoDePuntuacion(FlorDelMapa florTocada)
    {
        colocadorDeFlores.ColocarFlor(florTocada.gameObject.tag);
    }
}